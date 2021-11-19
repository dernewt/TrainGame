namespace TrainGame.Rules;

public static class DestinationCardFactory
{
    public static DestinationCard[] DestinationCards => new[]
        {
        new DestinationCard(City.Denver, City.ElPaso, 4),
        new DestinationCard(City.KansasCity, City.Houston, 5),
        new DestinationCard(City.NewYorkCity, City.Atlanta, 6),
        new DestinationCard(City.Chicago, City.NewOrelans, 7),
        new DestinationCard(City.Calgary, City.SaltLakeCity, 7),
        new DestinationCard(City.Helena, City.LosAngeles, 8),
        new DestinationCard(City.Duluth, City.Houston, 8),
        new DestinationCard(City.SaultSteMarie, City.Nashville, 8),
        new DestinationCard(City.Montreal, City.Atlanta, 9),
        new DestinationCard(City.SaultSteMarie, City.OklahomaCity, 9),
        new DestinationCard(City.Seattle, City.LosAngeles, 9),
        new DestinationCard(City.Chicago, City.SantaFe, 9),
        new DestinationCard(City.Duluth, City.ElPaso, 10),
        new DestinationCard(City.Toronto, City.Miami, 10),
        new DestinationCard(City.Portland, City.Phoenix, 11),
        new DestinationCard(City.Winnipeg, City.Houston, 12),
        new DestinationCard(City.Boston, City.Miami, 12),
        new DestinationCard(City.Vancouver, City.SantaFe, 13),
        new DestinationCard(City.Calgary, City.Phoenix, 13),
        new DestinationCard(City.Montreal, City.NewOrelans, 13),
        new DestinationCard(City.LosAngeles, City.Chicago, 16),
        new DestinationCard(City.SanFranscisco, City.Atlanta, 17),
        new DestinationCard(City.Portland, City.Nashville, 17),
        new DestinationCard(City.Vancouver, City.Montreal, 20),
        new DestinationCard(City.LosAngeles, City.Miami, 20),
        new DestinationCard(City.LosAngeles, City.NewYorkCity, 21),
        new DestinationCard(City.Seattle, City.NewYorkCity, 22),
    };
}