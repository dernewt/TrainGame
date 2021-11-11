using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TrainGame;
using TrainGame.Players;

var cts = new CancellationTokenSource();
var ct = cts.Token;

var tasks = Enumerable.Range(0, 5)
    .Select(_ => Task.Run(() =>Stuff.FindBestSeed(150), ct));

var winner = await await Task.WhenAny<int>(tasks);
cts.Cancel();
Console.Write(winner);

//var bestSeed = Stuff.FindBestSeed(50);
//Stuff.ExampleGame();

public static class Stuff
{
    public static void ExampleGame()
    {
        var game = new Game(new Player[] {
                new RandomPlayer(),
                new RandomPlayer(),
                //new HumanPlayer(new ConsoleRenderer() )
            });

        var leaderboard = game.Play();

        foreach (var turn in game.Log)
        {
            Console.WriteLine($"{turn.By.Name} {turn.Did}");
        }

        foreach (var player in leaderboard)
        {
            Console.WriteLine($"{player.Name} scored {player.Score}!" +
                $" Claimed {game.Board.OwnedRoutes(player).Count()} with {player.Trains} trains left" +
                $" Chain of {game.Board.LongestDestination(player).Length} and {player.Destinations.Count(d => game.Board.IsMet(d, player))}/{player.Destinations.Count} Destinations");
        }
    }

    public static int FindBestSeed(int rounds)
    {
        var bestSeed = 415540345;
        var bestStreak = 0;
        var streak = 0;

        while (bestStreak < rounds)
        {
            var game = new Game(new Player[] {
                new RandomPlayer(bestSeed),
                new RandomPlayer()
            });

            var leaderboard = game.Play();

            var winner = leaderboard
                .OrderByDescending(p => p.Score)
                .Cast<RandomPlayer>()
                .First();

            if (winner.Seed == bestSeed)
            {
                streak++;
            }
            else
            {
                if (streak > bestStreak)
                {
                    bestStreak = streak;
                    Console.WriteLine($"{bestSeed} achieved {bestStreak}!");
                    if (bestStreak >= rounds)
                        continue;
                }

                streak = 0;
                bestSeed = winner.Seed;
            }
        }

        Console.WriteLine($"{bestSeed} is the winner");
        return bestSeed;
    }
}