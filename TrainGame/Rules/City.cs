namespace TrainGame.Rules;
public enum City
{
    EMPTY = 0,
    LosAngeles,
    Phoenix,
    LasVegas,
    SanFranscisco,
    Denver,
    Houston,
    ElPaso,
    NewYorkCity,
    Atlanta,
    KansasCity,
    Chicago,
    NewOrelans,
    Calgary,
    SaltLakeCity,
    Helena,
    Duluth,
    SaultSteMarie,
    Nashville,
    Montreal,
    OklahomaCity,
    Pittsburgh,
    Seattle,
    SantaFe,
    Miami,
    Toronto,
    Portland,
    Winnipeg,
    Vancouver,
    Boston,
    Omaha,
    Dallas,
    SaintLouis,
    LittleRock,
    WashingtonDc,
    Raleigh,
    Charleston,
}; //36

public static class CityFactory
{
    public static City[][] CityLayout = { //6,6 = 36
        new [] { City.Vancouver, City.Calgary, City.Winnipeg, City.SaultSteMarie, City.Montreal, City.Boston },
        new [] { City.Seattle, City.Helena, City.Duluth, City.Chicago, City.Toronto, City.NewYorkCity},
        new [] { City.Portland, City.Omaha, City.KansasCity, City.SaintLouis, City.Pittsburgh, City.WashingtonDc},
        new [] { City.SaltLakeCity, City.Denver, City.OklahomaCity, City.LittleRock, City.Nashville, City.Raleigh},
        new [] { City.SanFranscisco, City.LasVegas, City.SantaFe, City.Dallas, City.Atlanta, City.Charleston},
        new [] { City.LosAngeles, City.Phoenix, City.ElPaso, City.Houston, City.NewOrelans, City.Miami}
    };
}