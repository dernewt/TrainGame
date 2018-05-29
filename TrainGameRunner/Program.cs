using System;
using System.Diagnostics;
using System.Linq;
using TrainGame;
using TrainGame.Players;

namespace TrainGameRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            var timer = new Stopwatch();
            var game = new Game(new Player[] {
                new RandomPlayer(),
                new RandomPlayer(),
                new HumanPlayer(new ConsoleRenderer() )
            });

            timer.Start();
            var leaderboard = game.Play();
            timer.Stop();

            Console.WriteLine($"Game took {timer.Elapsed.Seconds} seconds");

            foreach (var player in leaderboard)
            {
                Console.WriteLine($"{player.Name} scored {player.Score}!" +
                    $" Claimed {game.Board.OwnedRoutes(player).Count()} with {player.Trains} trains left" +
                    $" Chain of {game.Board.LongestDestination(player).Length} and {player.Destinations.Count(d=>game.Board.IsMet(d, player))}/{player.Destinations.Count} Destinations");
            }
            Console.ReadKey();
        }
    }
}
