using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using TrainGame.Players;
using TrainGame.Rules;

namespace TrainGame;
public class Game
{
    public Player[] Players { get; }
    public Rule RuleSet { get; }
    public DiscardableDeck<TrainCard> TicketDeck { get; }
    public TrainCard[] TicketDisplay { get; }
    public Deck<DestinationCard> DestinationDeck { get; }
    public RouteMap Board { get; }
    public List<Turn> Log { get; }

    protected Random Entropy;

    public Game(Player[] players, Rule ruleSet = null, int? seed = null)
    {
        RuleSet = ruleSet ?? new Standard();

        Entropy = new Random(seed ?? Guid.NewGuid().GetHashCode());

        Log = new List<Turn>();

        Players = players;
        foreach (var player in Players)
        {
            player.Trains = RuleSet.PlayerTrainStart;
            player.Name = player.DecideName();
        }

        Board = new RouteMap(RouteFactory.Routes,
            CityFactory.CityLayout,
            Players.Length > RuleSet.PlayerAlternateRouteMinimum);

        TicketDeck = new DiscardableDeck<TrainCard>(Entropy, RuleSet, GenerateTickets());
        TicketDeck.Shuffle();

        DestinationDeck = new Deck<DestinationCard>(Entropy, RuleSet, DestinationCardFactory.DestinationCards);
        DestinationDeck.Shuffle();

        TicketDisplay = TicketDeck.Draw(RuleSet.TicketShownMaximum).ToArray();
        EnsureTicketDisplayValid();

    }

    /// <summary>
    /// Game actions can throw if they are not legal, catch these and rety.
    /// Nest to prevent leaking of game secrets
    /// </summary>
    /// <param name="block"></param>
    protected void ScopeChoices(Action block)
    {
        Policy.Handle<ArgumentException>()
            .RetryForever()
            .Execute(block);
    }

    /// <summary>
    /// Really feel weird about this
    /// </summary>
    /// <returns></returns>
    public IOrderedEnumerable<Player> Play()
    {

        var gameActive = true;
        foreach (var player in Players.Cycle())
        {
            ScopeChoices(() =>
            {
                var move = player.Destinations.Any() ? player.DecideAction(this) : PlayerAction.DrawDestination;
                var movePublicInfo = "";

                switch (move)
                {
                    case PlayerAction.ClaimRoute:
                        var route = player.NextClaim(this);
                        Claim(route, player);
                        if (player.Trains < RuleSet.PlayerTrainMinimum)
                            gameActive = false;
                        break;

                    case PlayerAction.DrawDestination:
                        var choices = DestinationDeck.DrawOptions();
                        ScopeChoices(() =>
                        {
                            var picks = player.DecideDestinations(choices, this);
                            Claim(picks, player);
                            DestinationDeck.ReturnOptions(choices.Except(picks));
                            movePublicInfo = "Picked " + picks.Count();
                        });

                        break;

                    case PlayerAction.DrawTicket:
                        TrainCard pick;
                        if (TicketDisplay.All(c => c == null))
                            throw new ArgumentException("Nothing left to draw from");
                        ScopeChoices(() =>
                        {
                            var left = RuleSet.TicketDrawMaximum;
                            do
                            {
                                pick = player.DecideTicket(TicketDisplay, this);
                                movePublicInfo += (movePublicInfo.Any() ? " " : "Picked ") + pick.Color;
                            } while (Claim(pick, player, --left));
                        });
                        break;
                };
                Log.Add(new Turn(player, move, movePublicInfo));
            });

            if (!gameActive)
                break;
        }

        return ScoreDestinations();
    }

    public IOrderedEnumerable<Player> ScoreDestinations()
    {
        var longestDestinationLength = new Dictionary<Player, int>();

        foreach (var player in Players)
        {
            foreach (var destination in player.Destinations)
            {
                if (Board.IsMet(destination, player))
                    player.Score += destination.Points;
                else
                    player.Score -= destination.Points;
            }

            longestDestinationLength.Add(player, Board.LongestDestination(player).Length);
        }

        longestDestinationLength.OrderByDescending(d => d.Value)
            .First()
            .Key.Score += RuleSet.LongestDestinationWorth;

        return Players.OrderByDescending(p => p.Score)
            .ThenByDescending(p => longestDestinationLength[p]);
    }

    public void Claim(Route route, Player player)
    {
        if (player.Trains < route.Tag.Length)
            throw new ArgumentException("you don't have enough trains");
        //TODO allow for explicit spending of tickets 
        var spent = player.Tickets
            .Where(t => route.Tag.Color == Color.Any || t.Color == route.Tag.Color || t.Color == Color.Any)
            .OrderByDescending(t => t.Color) //GOTCHA  Color.Any == 0 which is always last
            .Take(route.Tag.Length);

        if (spent.Count() < route.Tag.Length)
            throw new ArgumentException("you don't have enough tickets");

        Board.Claim(route, player); //GOTCHA can throw ArgumentException, do this first before changing things
        player.Tickets.RemoveAll(t => spent.Contains(t));
        player.Trains -= route.Tag.Length;
        player.Score += RuleSet.RouteLengthScore[route.Tag.Length];
    }

    public void Claim(IEnumerable<DestinationCard> picks, Player player)
    {
        player.Destinations.AddRange(picks);
    }

    public bool Claim(TrainCard pick, Player player, int claimsLeft)
    {
        if (pick == null)
        {
            if (!TicketDeck.CanDraw())
                throw new ArgumentException("There is nothing left to draw");

            player.Tickets.Add(TicketDeck.Draw());
            return claimsLeft > 0;
        }

        player.Tickets.Add(pick);

        TicketDisplay[TicketDisplay.FindIndex(pick)] = null;

        EnsureTicketDisplayValid();

        return pick.Color != Color.Any && TicketDisplay.Any(c => c?.Color != Color.Any) && claimsLeft > 0;
    }

    protected void EnsureTicketDisplayValid()
    {
        for (int i = 0; i < TicketDisplay.Length; i++)
        {
            if (TicketDisplay[i] == null && TicketDeck.CanDraw())
                TicketDisplay[i] = TicketDeck.Draw();
        }

        var isDeckBigEnough = TicketDeck.CanDraw(TicketDisplay.Length - RuleSet.TicketAnyShownMaximum,
            c => c.Color != Color.Any);

        if (isDeckBigEnough)
        {
            while (TicketDisplay.Count(c => c.Color == Color.Any) >= RuleSet.TicketAnyShownMaximum)
            {
                TicketDeck.Discard.AddRange(TicketDisplay);
                var index = 0;
                foreach (var item in TicketDeck.Draw(TicketDisplay.Length))
                    TicketDisplay[index++] = item;
            }
        }
    }

    protected List<TrainCard> GenerateTickets()
    {
        var cardTypes = (Color[])Enum.GetValues(typeof(Color));
        var cards = new List<TrainCard>(cardTypes.Length * RuleSet.TicketsPerColor);

        foreach (var cardType in cardTypes)
            cards.AddRange(Enumerable.Repeat(new TrainCard(cardType), RuleSet.TicketsPerColor));

        return cards;
    }
}

public static class Extensions
{
    public static IEnumerable<Player> Cycle(this Player[] list)
    {
        var count = list.Count();
        var index = 0;

        while (true)
        {
            yield return list.ElementAt(index);
            index = (index + 1) % count;
        }
    }
}