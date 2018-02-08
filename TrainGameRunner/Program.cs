using System;
using System.Linq;
using TrainGame;

namespace TrainGameRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            var g = new Game(new[] { new Player("rando"), new Player("rando2") });

            foreach (var player in g.Players)
            {
                
                switch(player.DecideAction(g))
                {
                    case PlayerAction.ClaimRoute:
                        break;

                    case PlayerAction.DrawDestination:
                        var choices = g.DestinationDeck.Draw();
                        var picks = player.DecideDestinations(g, choices);
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
