using System;
using System.Collections.Generic;
using TrainGame.Players;

namespace TrainGame
{
    public interface IRender
    {
        string Ask(string prompt);
        T Pick<T>(IEnumerable<T> choices);
        IEnumerable<T> Pick<T>(IEnumerable<T> choices, int count);
        IEnumerable<T> Pick<T>(IEnumerable<T> choices, int countMinimum, int countMaximum);
        void Render(Game current, HumanPlayer humanPlayer);
    }
}