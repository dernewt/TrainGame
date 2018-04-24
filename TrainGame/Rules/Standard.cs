using System.Collections.Generic;

namespace TrainGame.Rules
{
    class Standard : Rule
    {
        public override int PlayerTrainMinimum { get; } = 3;
        public override int PlayerTrainStart { get; } = 45;
        public override int PlayerAlternateRouteMinimum { get; } = 3;
        public override int TicketDrawMaximum { get; } = 2;
        public override int TicketShownMaximum { get; } = 5;
        public override int TicketAnyShownMaximum { get; } = 3;
        public override int TicketsPerColor { get; } = 45;
        public override int DestinationDrawMaximum { get; } = 3;
        public override int DestinationDrawMinimum { get; } = 2;
        public override int LongestDestinationWorth { get; } = 10;
        public override Dictionary<int, int> RouteLengthScore { get; } = new Dictionary<int, int>
        {
            { 1, 1 },
            { 2, 2 },
            { 3, 4 },
            { 4, 7 },
            { 5, 10 },
            { 6, 15 },
        };
    }
}
