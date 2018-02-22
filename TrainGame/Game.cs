using System;
using System.Collections.Generic;
using System.Linq;

namespace TrainGame
{
    public class Game
    {
        public static int PlayerTrainMinimum { get; } = 3;
        public static int TicketDrawMaximum { get; } = 2;
        public static int TicketShownMaximum { get; } = 5;
        public static int TicketAnyShownMaximum { get; } = 3;
        public static int DestinationDrawMaximum { get; } = 3;
        public static int DestinationDrawMinimum { get; } = 2;

        public Player[] Players { get; }
        public DiscardableDeck<TrainCard> TicketDeck { get;}
        public TrainCard[] TicketDisplay { get; }
        public Deck<DestinationCard> DestinationDeck { get; }
        public RouteMap Board { get; }

        protected Random Entropy;

        public Game(Player[] players, int? seed = null)
        {
            Entropy = new Random(seed ?? Guid.NewGuid().GetHashCode());

            Players = players;

            TicketDeck = new DiscardableDeck<TrainCard>(Entropy, 240);
            foreach (Color color in Enum.GetValues(typeof(Color)))
            {
                TicketDeck.AddRange(Enumerable.Repeat(new TrainCard(color), 45));
            }
            TicketDeck.Shuffle();

            DestinationDeck = new Deck<DestinationCard>(Entropy, 30)
            {
                new DestinationCard(City.LasVegas, City.Phoenix, 10, 0),
            };
            DestinationDeck.Shuffle();

            TicketDisplay = TicketDeck.Draw(TicketShownMaximum).ToArray();
            EnsureTicketDisplayValid();

        }

        public IOrderedEnumerable<Player> End()
        {
            var routeWinner = Players.OrderBy(p => Board.LongestDestination(p).Length).First();
            routeWinner.Destinations.Add(Board.LongestDestination(routeWinner));

            foreach (var player in Players)
            {
                foreach (var destination in player.Destinations)
                {
                    if (Board.IsMet(destination, player))
                        player.Score += destination.Points;
                    else
                        player.Score -= destination.Points;
                }
            }

            return Players.OrderByDescending(p => p.Score)
                .ThenByDescending(p=> Board.LongestDestination(p).Length);
        }


        public void Claim(Route route, Player player)
        {
            throw new NotImplementedException();

            //player.Trains -= 
            //player.Score += route.length

            //if (!AllowMultipleRoutes)
            //    Claim(otherRoute, new DisabledPlayer());
        }

        public void Claim(IEnumerable<DestinationCard> picks, Player player)
        {
            player.Destinations.AddRange(picks);
        }

        public bool Claim(TrainCard pick, Player player, int left)
        {
            if (pick == null)
            {
                player.Tickets.Add(TicketDeck.Draw());
                return left > 0;
            }

            player.Tickets.Add(pick);

            TicketDisplay[TicketDisplay.FindIndex(pick)] = TicketDeck.Draw();
            EnsureTicketDisplayValid();



            return pick.Type != Color.Any && left > 0;
        }

        protected void EnsureTicketDisplayValid()
        {
            var isDeckBigEnough = TicketDeck.Concat(TicketDeck.Discard).Count(c => c.Type != Color.Any) >= TicketDisplay.Length - TicketAnyShownMaximum;

            while (TicketDisplay.Count(c => c.Type == Color.Any) < TicketAnyShownMaximum && isDeckBigEnough)
            {
                TicketDeck.Discard.AddRange(TicketDisplay);
                var index = 0;
                foreach (var item in TicketDeck.Draw(TicketDisplay.Length))
                    TicketDisplay[index++] = item;
            }
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
    };

    public enum Color
    {
        Any,
        Purple,
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
