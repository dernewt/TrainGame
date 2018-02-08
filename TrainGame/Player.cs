using System;
using System.Collections.Generic;

namespace TrainGame
{
    public class Player
    {
        public List<DestinationCard> Destinations { get; }
        public int Score { get; set; }
        public string Name { get; }

        public Player(string name)
        {
            Name = string.IsNullOrWhiteSpace(name) ? GetHashCode().ToString() : name;
        }

        public PlayerAction DecideAction(Game state)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DestinationCard> DecideDestinations(Game g, IEnumerable<DestinationCard> choices)
        {
            throw new NotImplementedException();
        }

        public void TakeDestinations(IEnumerable<DestinationCard> picks)
        {
            Destinations.AddRange(picks);
        }
    }
}