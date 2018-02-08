using System;
using System.Collections.Generic;
using System.Linq;

namespace TrainGame
{
    public class Deck<T> : List<T>
    {
        protected Random Entropy { get; }

        public Deck(Random entropy, int capacity)
            : base(capacity)
        {
            Entropy = entropy;
        }

        public Deck(Random entropy, IEnumerable<T> data)
            :base(data)
        {
            Entropy = entropy;
        }

        //TODO some sort of .ToDeck operator?

        public Deck<T> Shuffle()
        {
            return new Deck<T>(Entropy, Enumerable.OrderBy(this, p => Entropy.Next()).ToList());
        }
    }
}
