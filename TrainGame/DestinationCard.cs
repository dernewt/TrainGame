using System;
using System.Collections.Generic;
using System.Linq;

namespace TrainGame
{
    public class DestinationCard
    {
        public City Start { get; }
        public City End { get; }
        public int Points { get; }
        public int Length { get; }

        public DestinationCard(City start, City end, int points, int length)
        {
            Start = start;
            End = end;
            Points = points;
            Length = length;
        }
    }

    public static class DeckExtentions
    {
        public static IEnumerable<DestinationCard> Draw(this List<DestinationCard> deck)
        {
            return deck.Take(3);
        }

        public static void ReturnDraw(this List<DestinationCard> deck, IEnumerable<DestinationCard> cards)
        {
            if (cards.Count() > 2)
                throw new ArgumentException("You must keep at least one card");

            deck.AddRange(cards);
        }
    }
}