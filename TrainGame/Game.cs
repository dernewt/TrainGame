using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using TrainGame.Players;
using TrainGame.Rules;

namespace TrainGame
{
    public class Game
    {
        public Player[] Players { get; }
        public Rule RuleSet { get; }
        public DiscardableDeck<TrainCard> TicketDeck { get;}
        public TrainCard[] TicketDisplay { get; }
        public Deck<DestinationCard> DestinationDeck { get; }
        public RouteMap Board { get; }

        protected Random Entropy;

        public Game(Player[] players, Rule ruleSet = null, int? seed = null)
        {
            RuleSet = ruleSet ?? new Standard();

            Entropy = new Random(seed ?? Guid.NewGuid().GetHashCode());

            Players = players;
            foreach (var player in Players)
            {
                player.Trains = RuleSet.PlayerTrainStart;
                player.Name = player.DecideName();
            }

            Board = new RouteMap(GenerateRoutes(),
                Players.Length > RuleSet.PlayerAlternateRouteMinimum);

            TicketDeck = new DiscardableDeck<TrainCard>(Entropy, RuleSet, GenerateTickets());
            TicketDeck.Shuffle();

            DestinationDeck = new Deck<DestinationCard>(Entropy, RuleSet, GenerateDestinations());
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
            while (gameActive)
            {
                var player = Players.Cycle().First();

                ScopeChoices(() =>
                {
                    switch (player.Destinations.Any() ? player.DecideAction(this) : PlayerAction.DrawDestination)
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
                            });

                            break;

                        case PlayerAction.DrawTicket:
                            TrainCard pick;
                            if (TicketDisplay.All(c=>c==null))
                                throw new ArgumentException("Nothing left to draw from");
                            ScopeChoices(() =>
                            {
                                var left = RuleSet.TicketDrawMaximum;
                                do
                                {
                                    pick = player.DecideTicket(TicketDisplay, this);
                                } while (Claim(pick, player, --left));
                            });
                            break;
                    };
                });
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
                .ThenByDescending(p=> longestDestinationLength[p]);
        }

        public void Claim(Route route, Player player)
        {
            if (player.Trains < route.Tag.Length)
                throw new ArgumentException("you don't have enough trains");

            var spent = player.Tickets
                .Where(t => t.Color == route.Tag.Color || t.Color == Color.Any)
                .OrderByDescending(t=>t.Color) //GOTCHA  Color.Any == 0 which is always last
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

            return pick.Color != Color.Any && TicketDisplay.Any(c=>c?.Color != Color.Any) && claimsLeft > 0;
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

            if(isDeckBigEnough)
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

        protected Route[] GenerateRoutes()
        {
            var routes = new [] {
                new Route(City.LosAngeles, City.SanFranscisco, Color.Yellow, 3),
                new Route(City.LosAngeles, City.SanFranscisco, Color.Pink, 3),
                new Route(City.Portland, City.SanFranscisco, Color.Pink, 5),
                new Route(City.Portland, City.SanFranscisco, Color.Green, 5),
                new Route(City.Portland, City.Seattle, Color.Any, 1),
                new Route(City.Portland, City.Seattle, Color.Any, 1),
                new Route(City.Vancouver, City.Seattle, Color.Any, 1),
                new Route(City.Vancouver, City.Seattle, Color.Any, 1),
                new Route(City.Vancouver, City.Calgary, Color.Any, 3),
                new Route(City.Calgary, City.Winnipeg, Color.White, 6),
                new Route(City.Calgary, City.Seattle, Color.Any, 4),
                new Route(City.Calgary, City.Helena, Color.Green, 4),
                new Route(City.Helena, City.Seattle, Color.Yellow, 6),
                new Route(City.Helena, City.Winnipeg, Color.Blue, 4),
                new Route(City.Helena, City.Duluth, Color.Orange, 6),
                new Route(City.Helena, City.Omaha, Color.Red, 5),
                new Route(City.Helena, City.Denver, Color.Green, 4),
                new Route(City.Helena, City.SaltLakeCity, Color.Pink, 3),
                new Route(City.SaltLakeCity, City.Portland, Color.Blue, 6),
                new Route(City.SaltLakeCity, City.SanFranscisco, Color.Orange, 5),
                new Route(City.SaltLakeCity, City.SanFranscisco, Color.Yellow, 5),
                new Route(City.SaltLakeCity, City.LasVegas, Color.Blue, 3),
                new Route(City.SaltLakeCity, City.Denver, Color.Red, 3),
                new Route(City.SaltLakeCity, City.Denver, Color.Yellow, 3),
                new Route(City.LosAngeles, City.LasVegas, Color.Any, 2),
                new Route(City.Phoenix, City.LosAngeles, Color.Any, 3),
                new Route(City.Phoenix, City.Denver, Color.White, 5),
                new Route(City.Phoenix, City.SantaFe, Color.Blue, 3),
                new Route(City.Phoenix, City.ElPaso, Color.Any, 3),
                new Route(City.LosAngeles, City.ElPaso, Color.Black, 6),
                new Route(City.ElPaso, City.SantaFe, Color.Any, 2),
                new Route(City.ElPaso, City.Dallas, Color.Red, 4),
                new Route(City.ElPaso, City.OklahomaCity, Color.Yellow, 5),
                new Route(City.ElPaso, City.Houston, Color.Green, 6),
                new Route(City.SantaFe, City.Denver, Color.Any, 2),
                new Route(City.SantaFe, City.OklahomaCity, Color.Blue, 3),
                new Route(City.Denver, City.OklahomaCity, Color.Red, 3),
                new Route(City.Denver, City.KansasCity, Color.Black, 4),
                new Route(City.Denver, City.KansasCity, Color.Orange, 4),
                new Route(City.Denver, City.Omaha, Color.Pink, 4),
                new Route(City.Winnipeg, City.SaultSteMarie, Color.Any, 6),
                new Route(City.Winnipeg, City.Duluth, Color.Black, 4),
                new Route(City.Duluth, City.SaultSteMarie, Color.Yellow, 3),
                new Route(City.Duluth, City.Toronto, Color.Pink, 6),
                new Route(City.Duluth, City.Chicago, Color.Red, 3),
                new Route(City.Duluth, City.Omaha, Color.Any, 2),
                new Route(City.Duluth, City.Omaha, Color.Green, 2),
                new Route(City.Omaha, City.Chicago, Color.Blue, 4),
                new Route(City.Omaha, City.KansasCity, Color.Any, 1),
                new Route(City.Omaha, City.KansasCity, Color.Any, 1),
                new Route(City.KansasCity, City.SaintLouis, Color.Green, 2),
                new Route(City.KansasCity, City.SaintLouis, Color.Pink, 2),
                new Route(City.KansasCity, City.OklahomaCity, Color.Any, 2),
                new Route(City.KansasCity, City.OklahomaCity, Color.Yellow, 2),
                new Route(City.OklahomaCity, City.LittleRock, Color.Black, 2),
                new Route(City.OklahomaCity, City.Dallas, Color.Any, 2),
                new Route(City.OklahomaCity, City.Dallas, Color.Any, 2),
                new Route(City.Dallas, City.Houston, Color.Any, 1),
                new Route(City.Dallas, City.Houston, Color.Any, 1),
                new Route(City.Dallas, City.LittleRock, Color.Any, 2),
                new Route(City.Houston, City.NewOrelans, Color.Yellow, 2),
                new Route(City.NewOrelans, City.LittleRock, Color.Green, 3),
                new Route(City.NewOrelans, City.Atlanta, Color.Orange, 4),
                new Route(City.NewOrelans, City.Atlanta, Color.Yellow, 4),
                new Route(City.NewOrelans, City.Miami, Color.Red, 6),
                new Route(City.LittleRock, City.SaintLouis, Color.Any, 2),
                new Route(City.LittleRock, City.Nashville, Color.White, 3),
                new Route(City.SaintLouis, City.Nashville, Color.Any, 2),
                new Route(City.SaintLouis, City.Chicago, Color.Green, 2),
                new Route(City.SaintLouis, City.Chicago, Color.White, 2),
                new Route(City.SaintLouis, City.Pittsburgh, Color.Orange, 5),
                new Route(City.Chicago, City.Toronto, Color.White, 4),
                new Route(City.Chicago, City.Pittsburgh, Color.Orange, 3),
                new Route(City.Chicago, City.Pittsburgh, Color.Black, 3),
                new Route(City.SaultSteMarie, City.Toronto, Color.Orange, 2),
                new Route(City.SaultSteMarie, City.Montreal, Color.Black, 5),
                new Route(City.Montreal, City.Toronto, Color.Any, 3),
                new Route(City.Montreal, City.Boston, Color.Any, 2),
                new Route(City.Montreal, City.Boston, Color.Orange, 2),
                new Route(City.Montreal, City.NewYorkCity, Color.Blue, 3),
                new Route(City.Toronto, City.Pittsburgh, Color.Any, 2),
                new Route(City.Boston, City.NewYorkCity, Color.Yellow, 2),
                new Route(City.Boston, City.NewYorkCity, Color.Red, 2),
                new Route(City.NewYorkCity, City.Pittsburgh, Color.White, 2),
                new Route(City.NewYorkCity, City.Pittsburgh, Color.Green, 2),
                new Route(City.NewYorkCity, City.WashingtonDc, Color.Orange, 2),
                new Route(City.NewYorkCity, City.WashingtonDc, Color.Black, 2),
                new Route(City.Pittsburgh, City.Nashville, Color.Yellow, 4),
                new Route(City.Pittsburgh, City.WashingtonDc, Color.Any, 2),
                new Route(City.Pittsburgh, City.Raleigh, Color.Any, 2),
                new Route(City.WashingtonDc, City.Raleigh, Color.Any, 2),
                new Route(City.WashingtonDc, City.Raleigh, Color.Any, 2),
                new Route(City.Raleigh, City.Nashville, Color.Black, 3),
                new Route(City.Raleigh, City.Charleston, Color.Any, 2),
                new Route(City.Raleigh, City.Atlanta, Color.Any, 2),
                new Route(City.Raleigh, City.Atlanta, Color.Black, 2),
                new Route(City.Nashville, City.Atlanta, Color.Any, 1),
                new Route(City.Atlanta, City.Charleston, Color.Any, 2),
                new Route(City.Atlanta, City.Miami, Color.Black, 5),
                new Route(City.Charleston, City.Miami, Color.Pink, 4),
            };
            return routes;
        }

        protected DestinationCard[] GenerateDestinations()
        {
            var cards = new[]
            {
                new DestinationCard(City.Denver, City.ElPaso, 4),
                new DestinationCard(City.KansasCity, City.Houston, 5),
                new DestinationCard(City.NewYorkCity, City.Atlanta, 6),
                new DestinationCard(City.Chicago, City.NewOrelans, 7),
                new DestinationCard(City.Calgary, City.SaltLakeCity, 7),
                new DestinationCard(City.Helena, City.LosAngeles, 8),
                new DestinationCard(City.Duluth, City.Houston, 8),
                new DestinationCard(City.SaultSteMarie, City.Nashville, 8),
                new DestinationCard(City.Montreal, City.Atlanta, 9),
                new DestinationCard(City.SaultSteMarie, City.OklahomaCity, 9),
                new DestinationCard(City.Seattle, City.LosAngeles, 9),
                new DestinationCard(City.Chicago, City.SantaFe, 9),
                new DestinationCard(City.Duluth, City.ElPaso, 10),
                new DestinationCard(City.Toronto, City.Miami, 10),
                new DestinationCard(City.Portland, City.Phoenix, 11),
                new DestinationCard(City.Winnipeg, City.Houston, 12),
                new DestinationCard(City.Boston, City.Miami, 12),
                new DestinationCard(City.Vancouver, City.SantaFe, 13),
                new DestinationCard(City.Calgary, City.Phoenix, 13),
                new DestinationCard(City.Montreal, City.NewOrelans, 13),
                new DestinationCard(City.LosAngeles, City.Chicago, 16),
                new DestinationCard(City.SanFranscisco, City.Atlanta, 17),
                new DestinationCard(City.Portland, City.Nashville, 17),
                new DestinationCard(City.Vancouver, City.Montreal, 20),
                new DestinationCard(City.LosAngeles, City.Miami, 20),
                new DestinationCard(City.LosAngeles, City.NewYorkCity, 21),
                new DestinationCard(City.Seattle, City.NewYorkCity, 22),
            };
            return cards;
        }

        protected List<TrainCard> GenerateTickets()
        {
            var cardTypes = (Color[]) Enum.GetValues(typeof(Color));
            var cards = new List<TrainCard>(cardTypes.Length * RuleSet.TicketsPerColor);

            foreach (var cardType in cardTypes)
                cards.AddRange(Enumerable.Repeat(new TrainCard(cardType), RuleSet.TicketsPerColor));

            return cards;
        }
    }

    public enum PlayerAction
    {
        DrawDestination,
        DrawTicket,
        ClaimRoute
    }

    public enum City
    {
        LosAngeles,
        Phoenix,
        LasVegas,
        SanFranscisco,
        Denver,
        Houston,
        ElPaso,
        NewYorkCity,
        Atlanta,
        KansasCity,
        Chicago,
        NewOrelans,
        Calgary,
        SaltLakeCity,
        Helena,
        Duluth,
        SaultSteMarie,
        Nashville,
        Montreal,
        OklahomaCity,
        Pittsburgh,
        Seattle,
        SantaFe,
        Miami,
        Toronto,
        Portland,
        Winnipeg,
        Vancouver,
        Boston,
        Omaha,
        Dallas,
        SaintLouis,
        LittleRock,
        WashingtonDc,
        Raleigh,
        Charleston,
    };

    public enum Color
    {
        Any,
        Pink,
        White,
        Blue,
        Yellow,
        Orange,
        Black,
        Red,
        Green
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
}
