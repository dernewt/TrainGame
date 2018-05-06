using System;
using System.Collections.Generic;
using System.Linq;
using TrainGame;
using TrainGame.Players;

namespace TrainGameRunner
{
    class ConsoleRenderer : IRender
    {
        public string Ask(string prompt)
        {
            Console.WriteLine(prompt);
            return Console.ReadLine();
        }

        public T Pick<T>(IEnumerable<T> choices)
            => Pick<T>(choices, 1, 1).Single();

        public IEnumerable<T> Pick<T>(IEnumerable<T> choices, int count)
            => Pick<T>(choices, count, count);

        public IEnumerable<T> Pick<T>(IEnumerable<T> choices, int countMinimum, int countMaximum)
        {
            throw new NotImplementedException();
        }

        public void Render(Game current, HumanPlayer humanPlayer)
        {
            throw new NotImplementedException();
        }
    }
}
