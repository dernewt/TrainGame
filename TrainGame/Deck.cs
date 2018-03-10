using System;
using System.Collections.Generic;
using System.Linq;
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
            :base(data)
        {
            Entropy = entropy;
            RuleSet = ruleSet;
        }

        public T Draw()
            => Draw(1).Single();

        public virtual IEnumerable<T> Draw(int count)
        {
            var cards = this.Take(count);
            RemoveRange(0, count);
            return cards;
        }

        public virtual void Shuffle()
        {
            //probbaly not a good idea, but it's easy!
            Sort((x,y)=>Entropy.Next(-1,2));
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
        => deck.Draw(deck.RuleSet.DestinationDrawMaximum);

        public static void ReturnOptions(this Deck<DestinationCard> deck, IEnumerable<DestinationCard> cards)
        {
            if (cards.Count() > deck.RuleSet.DestinationDrawMinimum)
                throw new ArgumentException("You must keep at least one card");

            deck.AddRange(cards);
        }
    }
}
