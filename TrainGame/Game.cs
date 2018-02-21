using System;
using System.Collections.Generic;
using System.Linq;

namespace TrainGame
{
    public class Game
    {
        public const int MaxTicketDraw = 2;
        public Player[] Players { get; }
        public Deck<TrainCard> TicketDeck { get; }
        public TrainCard[] TicketDisplay { get; }
        public Deck<DestinationCard> DestinationDeck { get; }
        public RouteMap Board { get; }

        protected Random Entropy;

        public Game(Player[] players, int? seed = null)
        {
            Entropy = new Random(seed ?? Guid.NewGuid().GetHashCode());

            Players = players;

            TicketDeck = new Deck<TrainCard>(Entropy, 240);
            foreach (Color color in Enum.GetValues(typeof(Color)))
            {
                TicketDeck.AddRange(Enumerable.Repeat(new TrainCard(color), 45));
            }
            TicketDeck = TicketDeck.Shuffle();

            DestinationDeck = new Deck<DestinationCard>(Entropy, 30)
            {
                new DestinationCard(City.LasVegas, City.Phoenix, 10, 0),
            };
            DestinationDeck = DestinationDeck.Shuffle();

            TicketDisplay = TicketDeck.Take(5).ToArray();

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

        public void Claim(IEnumerable<DestinationCard> picks, Player player)
        {
            player.Destinations.AddRange(picks);
        }

        public bool Claim(TrainCard pick, Player player, int left)
        {
            var resolvedPick = pick ?? TicketDeck.Take(1).Single();
            player.Tickets.Add(resolvedPick);

            if ((pick == null || pick.Type != Color.Any) && left > 0)
                return true;
            else
                return false;

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
