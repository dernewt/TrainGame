using Xunit;
using TrainGame;
using TrainGame.Players;
using TrainGame.Rules;
using System;
using System.Linq;
using QuickGraph;

namespace TrainGameTest;

public class RouteMapTest
{
    [Fact]
    public void LongestDestinationOfNothing()
    {
        var map = new RouteMap(new[] { new Route(City.Pittsburgh, City.Raleigh, Color.Red, 1) }, Array.Empty<City[]>());
        var result = map.LongestDestination(new RandomPlayer());
        Assert.Equal(0, result.Length);
    }

    [Fact]
    public void LongestDestinationOfTree()
    {
        var player = new RandomPlayer();
        var map = new RouteMap(new[] {
                new Route(City.Pittsburgh, City.Raleigh, new EdgeProperties(Color.Red, 1) { Owner =  player}),
                new Route(City.Raleigh, City.Charleston, new EdgeProperties(Color.Red, 1) { Owner =  player}),
                new Route(City.Charleston, City.Miami, new EdgeProperties(Color.Red, 3) { Owner =  player}),
                new Route(City.Charleston, City.Atlanta, new EdgeProperties(Color.Red, 2) { Owner =  player}),
            }, Array.Empty<City[]>());
        var result = map.LongestDestination(player);
        Assert.Equal(5, result.Length);
    }

    [Fact]
    public void LongestDestinationOfCycle()
    {
        var player = new RandomPlayer();
        var map = new RouteMap(new[] {
                new Route(City.Pittsburgh, City.Raleigh, new EdgeProperties(Color.Red, 1) { Owner =  player}),
                new Route(City.Raleigh, City.Charleston, new EdgeProperties(Color.Red, 1) { Owner =  player}),
                new Route(City.Charleston, City.Pittsburgh, new EdgeProperties(Color.Red, 1) { Owner =  player}),
            }, Array.Empty<City[]>());
        var result = map.LongestDestination(player);
        Assert.Equal(3, result.Length);
    }

    [Fact]
    public void PrevDiagsTest()
    {
        var player = new RandomPlayer();
        var map = new RouteMap(RouteFactory.Routes, CityFactory.CityLayout);

        Assert.True(map.RoutesFrom(City.Helena, map.PrevDiags(City.Helena)).Single().IsAdjacent(City.SaltLakeCity));

        //new[] { City.Vancouver, City.Calgary, City.Winnipeg, City.SaultSteMarie, City.Montreal, City.Boston },
        //new[] { City.Seattle, City.Helena, City.Duluth, City.Chicago, City.Toronto, City.NewYorkCity },
        //new[] { City.Portland, City.Omaha, City.KansasCity, City.SaintLouis, City.Pittsburgh, City.WashingtonDc },
        //new[] { City.SaltLakeCity, City.Denver, City.OklahomaCity, City.LittleRock, City.Nashville, City.Raleigh },
        //new[] { City.SanFranscisco, City.LasVegas, City.SantaFe, City.Dallas, City.Atlanta, City.Charleston },
        //new[] { City.LosAngeles, City.Phoenix, City.ElPaso, City.Houston, City.NewOrelans, City.Miami }

    }
}
