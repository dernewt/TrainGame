using System.Collections.Generic;

namespace TrainGame.Rules
{
    public abstract class Rule
    {
        public abstract int PlayerTrainMinimum { get; }
        public abstract int PlayerTrainStart { get; }
        public abstract int PlayerAlternateRouteMinimum { get; }
        public abstract int TicketDrawMaximum { get; }
        public abstract int TicketShownMaximum { get; }
        public abstract int TicketAnyShownMaximum { get; }
        public abstract int TicketsPerColor { get; }
        public abstract int DestinationDrawMaximum { get; }
        public abstract int DestinationDrawMinimum { get; }
        public abstract int LongestDestinationWorth { get; }
        public abstract Dictionary<int, int> RouteLengthScore { get; }
    }
}
