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
        protected UndirectedGraph<City, Route> Routes;
        public readonly City[][] Map = CityFactory.CityLayout;
        protected Dictionary<City, (int, int)> MapMap;
        protected readonly bool AllowParallelRoutes;

        public RouteMap(IEnumerable<Route> connections, bool allowParalellRoutes = false)
        {
            MapMap = CityFactory.CityLayout.SelectMany((cities, row) => cities.Select((city, col) => new { city, row, col }))
                .ToDictionary(x => x.city, x => (x.row, x.col));
            Routes = MapFactory(connections);
            AllowParallelRoutes = allowParalellRoutes;
        }

        public City? NextOrDefault(City? source) => Nexts(source).FirstOrDefault();
        public IEnumerable<City> Nexts(City? source)
        {
            if (!source.HasValue || !MapMap.ContainsKey(source.Value))
                return Enumerable.Empty<City>();

            var (row, col) = MapMap[source.Value];

            var nextCol = col + 1;
            if (nextCol == Map[row].Length)
                return Enumerable.Empty<City>();

            return Map[row][nextCol..];
        }

        public City? BelowOrDefault(City? source) => Belows(source).FirstOrDefault();
        public IEnumerable<City> Belows(City? source)
        {
            if (!source.HasValue || !MapMap.ContainsKey(source.Value))
                return Enumerable.Empty<City>();

            var (row, col) = MapMap[source.Value];

            var nextRow = row + 1;
            if (nextRow == Map.Length)
                return Enumerable.Empty<City>();

            return Map[nextRow..].Select(r=>r[col]);
        }

        public City? NextDiagOrDefault(City? source) => NextDiags(source).FirstOrDefault();
        public IEnumerable<City> NextDiags(City? source)
        {
            if (!source.HasValue || !MapMap.ContainsKey(source.Value))
                return Enumerable.Empty<City>();

            var (row, col) = MapMap[source.Value];

            var nextRow = row + 1;
            if (nextRow == Map.Length)
                return Enumerable.Empty<City>();
            var nextCol = col + 1;
            if (nextCol == Map[row].Length) //todo need better jagged array checking
                return Enumerable.Empty<City>();

            return Map[nextRow..].Select(r=> r[nextCol..]).SelectMany(c => c);
        }

        public City? PrevDiagOrDefault(City? source)
        {
            if (!source.HasValue || !MapMap.ContainsKey(source.Value))
                return null;

            var (row, col) = MapMap[source.Value];
            return Map.ElementAtOrDefault(row - 1)
                ?.ElementAtOrDefault(col - 1);
        }

        public IEnumerable<City> PrevDiags(City? source)
        {
            if (!source.HasValue || !MapMap.ContainsKey(source.Value))
                return Enumerable.Empty<City>();

            var (row, col) = MapMap[source.Value];

            var prevRow = row - 1;
            if (prevRow < 0)
                return Enumerable.Empty<City>();
            var prevCol = col - 1;
            if (prevCol < 0) //todo need better jagged array checking
                return Enumerable.Empty<City>();

            return Map[prevRow..].Select(r=>r[prevCol..]).SelectMany(c => c);
        }

        public bool IsMet(DestinationCard destination, Player target)
        {
            var playerMap = MapFactory(Routes.Edges.Where(e => e.Tag.Owner == target));

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

            if (playerMap.Vertices.Contains(destination.Start))
                search.Compute(destination.Start);

            return foundEnd;
        }

        public IEnumerable<Route> RoutesFrom(City source)
        {
            return Routes.Edges
                .Where(r => r.IsAdjacent(source));
        }

        public IEnumerable<Route> RoutesFrom(City? source, City? target) => RoutesFrom(source, target.HasValue ? new[] { target.Value } : Enumerable.Empty<City>());

        public IEnumerable<Route> RoutesFrom(City? source, IEnumerable<City> targets)
        {
            var targetList = targets.ToArray();

            if (!source.HasValue || !targetList.Any())
                return Enumerable.Empty<Route>();

            return Routes.Edges
                .Where(r => r.IsAdjacent(source.Value) && targetList.Any(t => r.IsAdjacent(t)));
        }

        public IEnumerable<Route> OwnedRoutes(Player target)
        => MapFactory(Routes.Edges.Where(e => e.Tag.Owner == target)).Edges;

        public IEnumerable<Route> AvailableRoutes(Color key = Color.Any, int ticketCount = int.MaxValue)
        {
            return Routes.Edges.Where(e => (e.Tag.Color == key || e.Tag.Color == Color.Any || key == Color.Any)
            && e.Tag.Length <= ticketCount
            && e.Tag.Owner == null);
        }

        public IEnumerable<Route> ShortestRoute(DestinationCard path, Player target)
        {
            var map = MapFactory(Routes.Edges.Where(e => e.Tag.Owner == null || e.Tag.Owner == target));

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
            var playerMap = MapFactory(Routes.Edges.Where(e => e.Tag.Owner == target));

            var longestPath = playerMap.LongestPathBruteDepthFirst(r => r.Tag.Length);

            if (longestPath.Any())
                return new DestinationCard(longestPath.First().Source, longestPath.Last().Target, 0, longestPath.Sum(r => r.Tag.Length));

            return DestinationCard.DestinationCardNull;
        }

        public void Claim(Route route, Player player)
        {
            if (route.Tag.Owner != null)
                throw new ArgumentException("Already claimed?!");

            route.Tag.Owner = player;

            if (!AllowParallelRoutes)
            {
                var paralellEdges = Routes.Edges.Where(
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

        public override bool Equals(object obj)
        {
            var route = obj as Route;
            if (route == null)
                return base.Equals(obj);

            return route.Tag == Tag && route.IsAdjacent(Source) && route.IsAdjacent(Target);
        }

        public override int GetHashCode()
        {
            return Source.GetHashCode() ^ Target.GetHashCode() ^ Tag.GetHashCode(); //Note xor is communitive
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