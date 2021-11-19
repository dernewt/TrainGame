using System;
using System.Linq;
using System.Collections.Generic;
using QuickGraph;
using QuickGraph.Algorithms.ShortestPath;
using TrainGame.QuickGraphExtensions;
using QuickGraph.Algorithms.Search;
using TrainGame.Players;
using TrainGame.Rules;
using QuickGraph.Algorithms.Observers;

namespace TrainGame
{
    public class RouteMap
    {
        public UndirectedGraph<City, Route> Map;
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

            if(playerMap.Vertices.Contains(destination.Start))
                search.Compute(destination.Start);

            return foundEnd;
        }

        public IEnumerable<Route> OwnedRoutes(Player target)
        => MapFactory(Map.Edges.Where(e => e.Tag.Owner == target)).Edges;

        public IEnumerable<Route> AvailableRoutes(Color key = Color.Any, int ticketCount = int.MaxValue)
        {
            return Map.Edges.Where(e => (e.Tag.Color == key || e.Tag.Color == Color.Any || key == Color.Any)
            && e.Tag.Length <= ticketCount
            && e.Tag.Owner == null);
        }

        public IEnumerable<Route> ShortestRoute(DestinationCard path, Player target)
        {
            var map = MapFactory(Map.Edges.Where(e => e.Tag.Owner == null || e.Tag.Owner == target));

            IEnumerable<Route> shortestRoute = new Route[0];

            var search = new UndirectedDijkstraShortestPathAlgorithm<City, Route>(map, e => 10 + e.Tag.Length * -1);
            var distance = new UndirectedVertexPredecessorRecorderObserver<City, Route>();
            using (distance.Attach(search))
            {
                search.Compute(path.Start);
                distance.TryGetPath(path.End, out shortestRoute);
            }
            return shortestRoute;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public DestinationCard LongestDestination(Player target)
        {
            var playerMap = MapFactory(Map.Edges.Where(e => e.Tag.Owner == target));

            var longestPath = playerMap.LongestPathBruteDepthFirst(r=>r.Tag.Length);

            if(longestPath.Any())
                return new DestinationCard(longestPath.First().Source, longestPath.Last().Target, 0, longestPath.Sum(r=>r.Tag.Length));

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