using System;
using TrainGame;
using TrainGame.Players;

namespace TrainGameRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            var game = new Game(new[] { new RandomPlayer(), new RandomPlayer() });

            var leaderboard = game.Play();

            foreach (var player in leaderboard)
            {
                Console.WriteLine($"{player.Name} had {player.Score}");
            }
            Console.ReadKey();
        }
    }
}
