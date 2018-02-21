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

        public T Draw()
            => Draw(1).Single();

        public IEnumerable<T> Draw(int count)
        {
            var cards = this.Take(count);
            RemoveRange(0, count);
            return cards;
        }

        //TODO some sort of .ToDeck operator?

        public Deck<T> Shuffle()
        {
            return new Deck<T>(Entropy, Enumerable.OrderBy(this, p => Entropy.Next()).ToList());
        }
    }

    public static class DestinationDeckExtentions
    {
        public static IEnumerable<DestinationCard> DrawOptions(this Deck<DestinationCard> deck)
        => deck.Draw(3);

        public static void ReturnOptions(this Deck<DestinationCard> deck, IEnumerable<DestinationCard> cards)
        {
            if (cards.Count() > 2)
                throw new ArgumentException("You must keep at least one card");

            deck.AddRange(cards);
        }
    }
}
