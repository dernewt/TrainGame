using System;
using System.Collections.Generic;

namespace TrainGame
{
    public class DiscardableDeck<T> : Deck<T>
    {
        public List<T> Discard { get; }

        public DiscardableDeck(Random entropy, int capacity) : base(entropy, capacity)
        {
            Discard = new List<T>(capacity);
        }

        public DiscardableDeck(Random entropy, IEnumerable<T> data) : base(entropy, data)
        {
            Discard = new List<T>(Capacity);
        }

        public override IEnumerable<T> Draw(int count)
        {
            if(count > Count)
                Shuffle();

            return base.Draw(count);
        }

        public override void Shuffle()
        {
            AddRange(Discard);
            Discard.Clear();
            base.Shuffle();
        }
    }
}
