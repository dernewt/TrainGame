using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using TrainGame.Rules;

namespace TrainGame
{
    public class Deck<T> : List<T>
    {
        protected Random Entropy { get; }
        public Rule RuleSet { get; }

        public Deck(Random entropy, Rule ruleSet, int capacity)
            : base(capacity)
        {
            Entropy = entropy;
            RuleSet = ruleSet;
        }

        public Deck(Random entropy, Rule ruleSet, IEnumerable<T> data)
            : base(data)
        {
            Entropy = entropy;
            RuleSet = ruleSet;
        }

        public virtual bool CanDraw(int count = 1, Func<T, bool> having = null)
            => having == null ? Count >= count : this.Count(having) >= count;

        public virtual T Draw()
            => Draw(1).Single();

        public virtual IEnumerable<T> Draw(int count)
        {
            var cards = this.Take(count).ToArray();
            RemoveRange(0, count);
            return cards;
        }

        public virtual void Shuffle()
        {
            var newDeck = MoreLinq.Extensions.ShuffleExtension.Shuffle(this, Entropy)
                .ToArray();
            Clear();
            AddRange(newDeck);
        }
    }

    public static class DeckExtentions
    {
        public static int FindIndex<T>(this T[] deck, T item) where T : class
        {
            for (int position = 0; position < deck.Length; position++)
            {
                if (deck[position] == item)
                    return position;
            }
            throw new ArgumentException("Not Found", nameof(item));
        }

        public static IEnumerable<DestinationCard> DrawOptions(this Deck<DestinationCard> deck)
        {
            if (!deck.Any())
                throw new ArgumentException("There is nothing left to draw from");

            var hand = deck.RuleSet.DestinationDrawMaximum < deck.Count ?
                deck.RuleSet.DestinationDrawMaximum
                : deck.Count;

            return deck.Draw(hand);
        }


        public static void ReturnOptions(this Deck<DestinationCard> deck, IEnumerable<DestinationCard> cards)
        {
            if (cards.Count() > deck.RuleSet.DestinationDrawMinimum)
                throw new ArgumentException("You must keep at least one card");

            deck.AddRange(cards);
        }
    }
}
