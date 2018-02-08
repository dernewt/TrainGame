using System.Collections.Generic;

namespace TrainGame
{
    public interface IAgency
    {
        string DecideName();

        PlayerAction DecideAction(Game state);

        IEnumerable<DestinationCard> DecideDestinations(IEnumerable<DestinationCard> choices, Game g);

    }
}