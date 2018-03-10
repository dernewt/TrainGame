using System.Collections.Generic;

namespace TrainGame.Players
{
    public abstract class Player
    {
        public List<DestinationCard> Destinations { get; } = new List<DestinationCard>(10);
        public List<TrainCard> Tickets { get; } = new List<TrainCard>(30);
        public int Trains { get; set; } = 0;
        public int Score { get; set; } = 0;
        public string Name { get; set; }

        public Player()
        {
        }

        public abstract string DecideName();
        public abstract PlayerAction DecideAction(Game state);
        public abstract IEnumerable<DestinationCard> DecideDestinations(IEnumerable<DestinationCard> choices, Game g);
        public abstract Route NextClaim(Game current);
        public abstract TrainCard DecideTicket(TrainCard[] TicketDisplay, Game current);
    }
}