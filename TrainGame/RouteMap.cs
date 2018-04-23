using System;
using System.Linq;
using System.Collections.Generic;
using QuickGraph;
using QuickGraph.Algorithms.ShortestPath;
using QuickGraph.Algorithms;
using TrainGame.QuickGraphExtensions;
using QuickGraph.Algorithms.Search;
using TrainGame.Players;

namespace TrainGame
{
    public class RouteMap
    {
        protected UndirectedGraph<City, Route> Map;
        protected readonly bool AllowParallelRoutes;



        public RouteMap(IEnumerable<Route> connections, bool allowParalellRoutes = false)
        {
            Map = MapFactory(connections);
            AllowParallelRoutes = allowParalellRoutes;
        }
        public bool IsMet(DestinationCard destination, Player target)
        {
            var playerMap = MapFactory(Map.Edges.Where(e => e.Tag.Owner == target));

            var search = new UndirectedBreadthFirstSearchAlgorithm<City, Route>(playerMap);

            var foundEnd = false;
            search.DiscoverVertex += new VertexAction<City>(c =>
            {
                if (c == destination.End)
                {
                    foundEnd = true;
                    search.Abort();
                }
            });
            search.Compute(destination.Start);
            return foundEnd;
        }

        public IEnumerable<Route> AvailableRoutes(Color key, int ticketCount = int.MaxValue)
        {
            return Map.Edges.Where(e => (e.Tag.Color == key || e.Tag.Color == Color.Any)
            && e.Tag.Length <= ticketCount
            && e.Tag.Owner == null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public DestinationCard LongestDestination(Player target)
        {
            var playerMap = MapFactory(Map.Edges.Where(e => e.Tag.Owner == target));

            var longestPath = playerMap.LongestPathDynamicProgramming(r=>r.Tag.Length);

            if(longestPath.Any())
                return new DestinationCard(longestPath.First().Source, longestPath.Last().Target, longestPath.Sum(r=>r.Tag.Length));

            return DestinationCard.DestinationCardNull;
        }

        public void Claim(Route route, Player player)
        {
            if (route.Tag.Owner != null)
                throw new ArgumentException("Already claimed?!");

            route.Tag.Owner = player;

            if (!AllowParallelRoutes)
            {
                var paralellEdges = Map.Edges.Where(
                    e =>
                    e.Source == route.Source
                    && e.Target == route.Target
                    && e != route);

                foreach (var edge in paralellEdges)
                {
                    edge.Tag.Owner = new Disabled();
                }
            }
        }

        protected UndirectedGraph<City, Route> MapFactory(IEnumerable<Route> edges)
        {
            var map = new UndirectedGraph<City, Route>(true);
            map.AddVerticesAndEdgeRange(edges);
            return map;
        }

        protected class Disabled : Player
        {
            public override PlayerAction DecideAction(Game state)
                => throw new NotSupportedException();

            public override IEnumerable<DestinationCard> DecideDestinations(IEnumerable<DestinationCard> choices, Game g)
                => throw new NotSupportedException();

            public override string DecideName()
                => throw new NotSupportedException();

            public override TrainCard DecideTicket(TrainCard[] TicketDisplay, Game current)
                => throw new NotSupportedException();

            public override Route NextClaim(Game current)
                => throw new NotSupportedException();
        }

        
    }

    public class Route : TaggedUndirectedEdge<City, EdgeProperties>
    {
        public Route(City source, City target, EdgeProperties tag)
            : base(source, target, tag)
        {
        }

        public Route(City source, City target, Color color, int length)
            : this(source, target, new EdgeProperties(color, length))
        {
        }
    }

    public class EdgeProperties
    {
        public Color Color { get; }
        public int Length { get; }
        public Player Owner { get; set; } = null;

        public EdgeProperties(Color color, int length)
        {
            Color = color;
            Length = length;
        }
    }
}