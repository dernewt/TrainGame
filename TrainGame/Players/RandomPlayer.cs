using System;
using System.Collections.Generic;
using System.Linq;

namespace TrainGame.Players
{
    public class RandomPlayer : Player
    {
        protected Random Entropy;
        public int Seed { get; }

        public RandomPlayer()
            : this(Guid.NewGuid().GetHashCode())
        {
        }

        public RandomPlayer(int seed)
            :base()
        {
            Seed = seed;
            Entropy = new Random(Seed);
        }

        public override string DecideName()
        {
            return GetType().Name + "_" + Seed;
        }

        /// <remarks>
        /// Relies on game error handling to catch and repeat asking.
        /// </remarks>
        /// <param name="state">Not even used</param>
        /// <returns>Any action (may not even be valid)</returns>
        public override PlayerAction DecideAction(Game state = null)
        {
            switch (Entropy.Next(1000))
            {
                
                case int i when (i > 100+Tickets.Count*10):
                    return PlayerAction.DrawTicket;
                case int i when (i > 7):
                    return PlayerAction.ClaimRoute;
                default:
                    return PlayerAction.DrawDestination;
                    
            }

        }

        /// <remarks>
        /// Relies on game error handling to catch and repeat asking.
        /// </remarks>
        /// <param name="choices">cards to pick from</param>
        /// <param name="g">Not even used</param>
        /// <returns>Any number of cards (may not even be valid)</returns>
        public override IEnumerable<DestinationCard> DecideDestinations(IEnumerable<DestinationCard> choices, Game g = null)
        {
            return choices.Take(Entropy.Next(2, 3));
        }
        
        public override Route NextClaim(Game current)
        {

            //focus on unmet destinations then any big route
            foreach (var destination in Destinations.Concat(new[]{ DestinationCard.DestinationCardNull}))
            {
                IOrderedEnumerable<Route> routes;
                if (destination == DestinationCard.DestinationCardNull)
                {
                    routes = current.Board.AvailableRoutes().OrderByDescending(r => r.Tag.Length);
                } else
                {
                    if (current.Board.IsMet(destination, this))
                        continue;

                    routes = current.Board.ShortestRoute(destination, this)
                        .Where(r => r.Tag.Owner == null)
                        .OrderByDescending(r => r.Tag.Length);
                }

                if (!routes.Any())
                    continue;

                foreach (var route in routes)
                {
                    var resources = Tickets.Count(t => t.Color == Color.Any || t.Color == route.Tag.Color);
                    resources = resources < Trains ? resources : Trains;
                    if (route.Tag.Length <= resources)
                        return route;
                }
            }

            //there are no available claims
            throw new ArgumentException("Stupid me, there was nothing I could claim!");
        }

        public override TrainCard DecideTicket(TrainCard[] from, Game current)
        {
            var pick = Entropy.Next(0, from.Length); //GOTCHA ArgumentRangeException translates to "draw" choice
            
            return pick >= from.Length ? null : from[pick];
        }
    }
}
