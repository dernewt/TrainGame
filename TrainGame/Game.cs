using System;
using System.Collections.Generic;
using System.Linq;

namespace TrainGame
{
    public class Game
    {
        public static int PlayerTrainMinimum { get; } = 3;
        public static int PlayerAlternateRouteMinimum { get; } = 3;
        public static int TicketDrawMaximum { get; } = 2;
        public static int TicketShownMaximum { get; } = 5;
        public static int TicketAnyShownMaximum { get; } = 3;
        public static int TicketsPerColor { get; } = 45;
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

            Board = new RouteMap(GenerateRoutes(),
                Players.Length > PlayerAlternateRouteMinimum);

            TicketDeck = new DiscardableDeck<TrainCard>(Entropy, GenerateTickets());
            TicketDeck.Shuffle();

            DestinationDeck = new Deck<DestinationCard>(Entropy, GenerateDestinations());
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
            var cards = new List<TrainCard>(cardTypes.Length * TicketsPerColor);

            foreach (var cardType in cardTypes)
                cards.AddRange(Enumerable.Repeat(new TrainCard(cardType), TicketsPerColor));

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
