using System.Collections.Generic;

namespace TrainGame
{
    public abstract class Player
    {
        public List<DestinationCard> Destinations { get; }
        public int Score { get; set; }
        public string Name { get; }

        public Player()
        {
            Name = DecideName();
        }

        public void TakeDestinations(IEnumerable<DestinationCard> picks)
        {
            Destinations.AddRange(picks);
        }

        public abstract string DecideName();
        public abstract PlayerAction DecideAction(Game state);
        public abstract IEnumerable<DestinationCard> DecideDestinations(IEnumerable<DestinationCard> choices, Game g);
        public abstract Route NextClaim(Game current);
    }
}