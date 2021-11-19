using System.Collections.Generic;
using TrainGame.Players;
using TrainGame.Rules;

namespace TrainGame;

public interface IRenderHelper
{
    void RenderDisplay(Game current, HumanPlayer humanPlayer);

    string Ask(string prompt);

    PlayerAction Pick(IEnumerable<PlayerAction> choices);
    IEnumerable<PlayerAction> Pick(IEnumerable<PlayerAction> choices, int count);
    IEnumerable<PlayerAction> Pick(IEnumerable<PlayerAction> choices, int countMinimum, int countMaximum);

    DestinationCard Pick(IEnumerable<DestinationCard> choices);
    IEnumerable<DestinationCard> Pick(IEnumerable<DestinationCard> choices, int count);
    IEnumerable<DestinationCard> Pick(IEnumerable<DestinationCard> choices, int countMinimum, int countMaximum);

    Route Pick(IEnumerable<Route> choices);
    IEnumerable<Route> Pick(IEnumerable<Route> choices, int count);
    IEnumerable<Route> Pick(IEnumerable<Route> choices, int countMinimum, int countMaximum);

    TrainCard Pick(IEnumerable<TrainCard> choices);
    IEnumerable<TrainCard> Pick(IEnumerable<TrainCard> choices, int count);
    IEnumerable<TrainCard> Pick(IEnumerable<TrainCard> choices, int countMinimum, int countMaximum);

}
