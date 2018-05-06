using System;
using System.Collections.Generic;
using System.Linq;
using TrainGame;
using TrainGame.Players;

namespace TrainGameRunner
{
    class ConsoleRenderer : IRender
    {

        public void RenderDisplay(Game current, HumanPlayer humanPlayer)
        {
            throw new NotImplementedException();
        }

        public string Ask(string prompt)
        {
            Console.WriteLine(prompt);
            return Console.ReadLine();
        }

        public PlayerAction Pick(IEnumerable<PlayerAction> choices)
            => Pick(choices, 1, 1).Single();

        public IEnumerable<PlayerAction> Pick(IEnumerable<PlayerAction> choices, int count)
            => Pick(choices, count, count);

        public IEnumerable<PlayerAction> Pick(IEnumerable<PlayerAction> choices, int countMinimum, int countMaximum)
        {
            throw new NotImplementedException();
        }

        public DestinationCard Pick(IEnumerable<DestinationCard> choices)
            => Pick(choices, 1, 1).Single();

        public IEnumerable<DestinationCard> Pick(IEnumerable<DestinationCard> choices, int count)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DestinationCard> Pick(IEnumerable<DestinationCard> choices, int countMinimum, int countMaximum)
        {
            throw new NotImplementedException();
        }

        public Route Pick(IEnumerable<Route> choices)
            => Pick(choices, 1, 1).Single();

        public IEnumerable<Route> Pick(IEnumerable<Route> choices, int count)
            => Pick(choices, count, count);

        public IEnumerable<Route> Pick(IEnumerable<Route> choices, int countMinimum, int countMaximum)
        {
            throw new NotImplementedException();
        }

        public TrainCard Pick(IEnumerable<TrainCard> choices)
            => Pick(choices, 1, 1).Single();

        public IEnumerable<TrainCard> Pick(IEnumerable<TrainCard> choices, int count)
            => Pick(choices, count, count);

        public IEnumerable<TrainCard> Pick(IEnumerable<TrainCard> choices, int countMinimum, int countMaximum)
        {
            throw new NotImplementedException();
        }


    }
}
