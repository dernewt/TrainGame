using System.Collections.Generic;

namespace TrainGame
{
    public class Player
    {
        public List<DestinationCard> Destinations { get; }
        public int Score { get; set; }
        public string Name { get; }
        public IAgency Brain { get; }

        public Player(IAgency brain)
        {
            Brain = brain;
            Name = Brain.DecideName();
        }

        public void TakeDestinations(IEnumerable<DestinationCard> picks)
        {
            Destinations.AddRange(picks);
        }
    }
}