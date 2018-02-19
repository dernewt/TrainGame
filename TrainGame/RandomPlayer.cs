using System;
using System.Collections.Generic;
using System.Linq;

namespace TrainGame
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
            var allActions = Enum.GetValues(typeof(PlayerAction));

            return (PlayerAction)allActions.GetValue(
                Entropy.Next(allActions.Length));
        }

        /// <remarks>
        /// Relies on game error handling to catch and repeat asking.
        /// </remarks>
        /// <param name="choices">cards to pick from</param>
        /// <param name="g">Not even used</param>
        /// <returns>Any number of cards (may not even be valid)</returns>
        public override IEnumerable<DestinationCard> DecideDestinations(IEnumerable<DestinationCard> choices, Game g = null)
        {
            return new Deck<DestinationCard>(Entropy, choices).Shuffle().Take(Entropy.Next(1, choices.Count()));
        }
        
        public override Route NextClaim(Game current)
        {
            throw new NotImplementedException();
        }

        public override TrainCard DecideTicket(TrainCard[] from, Game current)
        {
            throw new NotImplementedException();
        }
    }
}
