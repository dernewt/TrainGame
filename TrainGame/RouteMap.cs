using System;
using System.Linq;
using System.Collections.Generic;
using QuickGraph;
using QuickGraph.Algorithms;

namespace TrainGame
{
    public class RouteMap
    {
        protected UndirectedGraph<City, Route> Map;
        protected readonly bool AllowMultipleRoutes;

        public RouteMap(IEnumerable<Route> connections, bool allowMultipleRoutes = false)
        {
            AllowMultipleRoutes = allowMultipleRoutes;

            Map.AddVerticesAndEdgeRange(connections);
        }
        public bool IsMet(DestinationCard destination, Player player)
        {
            throw new NotImplementedException();
        }

        public DestinationCard LongestDestination(Player target)
        {
            throw new NotImplementedException();
        }

        protected class DisabledPlayer : Player
        {
            public override PlayerAction DecideAction(Game state)
                => throw new ApplicationException();

            public override IEnumerable<DestinationCard> DecideDestinations(IEnumerable<DestinationCard> choices, Game g)
                => throw new ApplicationException();

            public override string DecideName()
                => throw new ApplicationException();

            public override TrainCard DecideTicket(TrainCard[] from, Game current)
                => throw new ApplicationException();

            public override Route NextClaim(Game current) 
                => throw new ApplicationException();
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
        Color Color { get; }
        int Length { get; }
        Player Owner { get; set; }

        public EdgeProperties(Color color, int length)
        {
            Color = color;
            Length = length;
        }
    }
}