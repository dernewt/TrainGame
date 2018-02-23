
namespace TrainGame
{
    public class DestinationCard
    {
        public City Start { get; }
        public City End { get; }
        public int Points { get; }
        public int Length { get; }

        public DestinationCard(City start, City end, int points, int length = 0)
        {
            Start = start;
            End = end;
            Points = points;
            Length = length;
        }
    }
    
}