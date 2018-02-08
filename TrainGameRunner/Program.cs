using System;
using System.Linq;
using TrainGame;

namespace TrainGameRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            var g = new Game(new[] { new Player(new RandomAgent()), new Player(new RandomAgent()) });

            foreach (var player in g.Players)
            {
                
                switch(player.Brain.DecideAction(g))
                {
                    case PlayerAction.ClaimRoute:
                        break;

                    case PlayerAction.DrawDestination:
                        var choices = g.DestinationDeck.Draw();
                        var picks = player.Brain.DecideDestinations(choices, g);
                        player.TakeDestinations(picks);
                        g.DestinationDeck.ReturnDraw(choices.Except(picks));
                        break;

                    case PlayerAction.DrawTicket:
                        break;
                }
            }

            var leaderboard = g.End();

            foreach (var player in leaderboard)
            {
                Console.WriteLine($"{player.Name} had {player.Score}");
            }
            Console.ReadKey();
        }
    }
}
