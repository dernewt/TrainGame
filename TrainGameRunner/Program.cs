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
            var game = new Game(new[] { new RandomPlayer(), new RandomPlayer() });

            //Scope can narrow if they make moves that leak game secrets
            var choiceScope = new Action<Action>((a) => Policy.Handle<ArgumentException>().RetryForever().Execute(a));

            var gameActive = true;
            while (gameActive)
            {
                var player = game.Players.Cycle().First();

                choiceScope(() =>
                    {
                        switch (player.Destinations.Any() ? player.DecideAction(game) : PlayerAction.DrawDestination)
                        {
                            case PlayerAction.ClaimRoute:
                                var route = player.NextClaim(game);
                                game.Claim(route, player);
                                if (player.Trains < Game.PlayerTrainMinimum)
                                    gameActive = false;
                                break;

                            case PlayerAction.DrawDestination:
                                var choices = game.DestinationDeck.DrawOptions();
                                choiceScope(() =>
                                {
                                    var picks = player.DecideDestinations(choices, game);
                                    game.Claim(picks, player);
                                    game.DestinationDeck.ReturnOptions(choices.Except(picks));
                                });

                                break;

                            case PlayerAction.DrawTicket:
                                TrainCard pick;
                                choiceScope(() =>
                                {
                                    var left = Game.TicketDrawMaximum;
                                    do
                                    {
                                        pick = player.DecideTicket(game.TicketDisplay, game);
                                    } while (game.Claim(pick, player, --left));
                                });
                                break;
                        };
                    });
            }

            var leaderboard = game.End();

            foreach (var player in leaderboard)
            {
                Console.WriteLine($"{player.Name} had {player.Score}");
            }
            Console.ReadKey();
        }
    }
}
