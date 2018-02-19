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

            //Scope can narrow if they make moves that leak game secrets
            var choiceScope = new Action<Action>((a) => Policy.Handle<ArgumentException>().RetryForever().Execute(a));

            foreach (var player in current.Players)
            {

                choiceScope(() =>
                    {
                        switch (player.DecideAction(current))
                        {
                            case PlayerAction.ClaimRoute:
                                var route = player.NextClaim(current);
                                current.Board.Claim(route, player);
                                break;

                            case PlayerAction.DrawDestination:
                                var choices = current.DestinationDeck.Draw();
                                choiceScope(()=>{
                                    var picks = player.DecideDestinations(choices, current);
                                    current.Claim(picks, player);
                                    current.DestinationDeck.ReturnDraw(choices.Except(picks));
                                });
                                
                                break;

                            case PlayerAction.DrawTicket:
                                TrainCard pick;
                                choiceScope(() =>
                                {
                                    var left = Game.MaxTicketDraw;
                                    do
                                    {
                                        pick = player.DecideTicket(current.TicketDisplay, current);
                                    } while (current.Claim(pick, player, --left));
                                });
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
