using Xunit;
using TrainGame;
using TrainGame.Players;
using TrainGame.Rules;

namespace TrainGameTest
{
    public class RouteMapTest
    {
        [Fact]
        public void LongestDestinationOfNothing()
        {
            var map = new RouteMap(new[] { new Route(City.Pittsburgh, City.Raleigh, Color.Red, 1)});
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
            });
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
            });
            var result = map.LongestDestination(player);
            Assert.Equal(3, result.Length);
        }
    }
}
