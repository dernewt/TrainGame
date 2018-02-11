using System;
using Polly;
using System.Linq;
using TrainGame;

namespace TrainGameRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            var current = new Game(new[] { new RandomPlayer(), new RandomPlayer() });

            foreach (var player in current.Players)
            {

                Policy.Handle<ArgumentException>()
                    .RetryForever()
                    .Execute(() =>
                    {
                        switch (player.DecideAction(current))
                        {
                            case PlayerAction.ClaimRoute:
                                var route = player.NextClaim(current);
                                current.Board.Claim(route, player);
                                break;

                            case PlayerAction.DrawDestination:
                                var choices = current.DestinationDeck.Draw();
                                var picks = player.DecideDestinations(choices, current);
                                player.TakeDestinations(picks);
                                current.DestinationDeck.ReturnDraw(choices.Except(picks));
                                break;

                            case PlayerAction.DrawTicket:
                                break;
                        };
                    });
            }

            var leaderboard = current.End();

            foreach (var player in leaderboard)
            {
                Console.WriteLine($"{player.Name} had {player.Score}");
            }
            Console.ReadKey();
        }
    }
}
