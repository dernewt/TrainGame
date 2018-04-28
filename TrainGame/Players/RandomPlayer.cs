﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace TrainGame.Players
{
    public class RandomPlayer : Player
    {
        protected Random Entropy;
        protected int Seed { get; }

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
            switch (Entropy.Next(100))
            {
                case int i when (i >= 50):
                    return PlayerAction.DrawDestination;
                case int i when (i <= 3):
                    return PlayerAction.DrawTicket;
                default:
                    return PlayerAction.ClaimRoute;
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
            var mostToLeast = Tickets.GroupBy(t => t.Color)
                .Select(t => new { Count=t.Count(), Color=t.Key})
                .OrderBy(t=>t.Count);

            foreach (var ticket in mostToLeast)
            {
                var routes = current.Board.AvailableRoutes(ticket.Color, Trains < ticket.Count ? Trains : ticket.Count);
                if (routes.Any())
                    return routes.First();
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
