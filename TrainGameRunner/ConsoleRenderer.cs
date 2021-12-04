using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TrainGame;
using TrainGame.Players;
using TrainGame.Rules;
using QuickGraph;

namespace TrainGameRunner
{
    class ConsoleRenderer : IRenderHelper
    {

        public void RenderDisplay(Game current, HumanPlayer player)
        {
            Render(current.Board);

            foreach (var turn in current.Log.TakeLast(current.Players.Count() - 1))
            {
                Console.WriteLine($"{turn.By.Name} {turn.DidDescription}");
            }

            Console.WriteLine();

            Console.WriteLine("You are holding these tickets:");
            foreach (var groupedTickets in player.Tickets.GroupBy(t => t.Color).OrderBy(tc => tc.Key))
            {
                Console.WriteLine($"{groupedTickets.Count()} {groupedTickets.Key}");
            }

            Console.WriteLine();

            Console.WriteLine($"You have {player.Score} points (excluding routes) and {player.Trains} trains left.");
        }

        public string Ask(string prompt)
        {
            Console.WriteLine(prompt);
            return Console.ReadLine();
        }

        public PlayerAction Pick(IEnumerable<PlayerAction> choices)
            => Pick(choices, 1, 1, Render).Single();
        public IEnumerable<PlayerAction> Pick(IEnumerable<PlayerAction> choices, int count)
            => Pick(choices, count, count, Render);
        public IEnumerable<PlayerAction> Pick(IEnumerable<PlayerAction> choices, int countMinimum, int countMaximum)
            => Pick(choices, countMinimum, countMaximum, Render);

        public DestinationCard Pick(IEnumerable<DestinationCard> choices)
            => Pick(choices, 1, 1, Render).Single();
        public IEnumerable<DestinationCard> Pick(IEnumerable<DestinationCard> choices, int count)
            => Pick(choices, count, count, Render);
        public IEnumerable<DestinationCard> Pick(IEnumerable<DestinationCard> choices, int countMinimum, int countMaximum)
            => Pick(choices, countMinimum, countMaximum, Render);

        public Route Pick(IEnumerable<Route> choices)
            => Pick(choices, 1, 1, Render).Single();
        public IEnumerable<Route> Pick(IEnumerable<Route> choices, int count)
            => Pick(choices, count, count, Render);
        public IEnumerable<Route> Pick(IEnumerable<Route> choices, int countMinimum, int countMaximum)
            => Pick(choices, countMinimum, countMaximum, Render);

        public TrainCard Pick(IEnumerable<TrainCard> choices)
            => Pick(choices, 1, 1, Render).Single();
        public IEnumerable<TrainCard> Pick(IEnumerable<TrainCard> choices, int count)
            => Pick(choices, count, count, Render);
        public IEnumerable<TrainCard> Pick(IEnumerable<TrainCard> choices, int countMinimum, int countMaximum)
            => Pick(choices, countMinimum, countMaximum, Render);

        protected string RouteLabel(Route r)
        {
            if (r.Tag.Owner != null)
                return r.Tag.Owner.Name;
            return r.Tag.Length + r.Tag.Color.ToString();
        }

        protected void Render(RouteMap board)
        {
            var rows = CityFactory.CityLayout.Length;
            var cols = CityFactory.CityLayout.Max(r => r.Length);
            var horizontalSpacing = Console.WindowWidth / (cols * 2);

            Console.WriteLine(new string('*', Console.WindowWidth));

            foreach (var row in board.Map)
            {
                var nextLine = "";
                foreach (var currentCity in row)
                {
                    Console.Write(string.Format("{0,-" + horizontalSpacing + "}", currentCity));
                    var nextCity = board.NextOrDefault(currentCity);
                    var nRoutes = board.RoutesFrom(currentCity, nextCity)
                        .Select(r => "-" + RouteLabel(r) + "-")
                        .Aggregate("", (a, b) => a + b);
                    var nnRoutes = board.RoutesFrom(currentCity, board.Nexts(nextCity))
                        .Select(r => "=" + RouteLabel(r) + "=")
                        .Aggregate("", (a, b) => a + b);
                    Console.Write(string.Format("{0,-" + horizontalSpacing + "}", nRoutes + nnRoutes));

                    var belowCity = board.BelowOrDefault(currentCity);
                    var bRoutes = board.RoutesFrom(currentCity, belowCity)
                        .Select(r => "|" + RouteLabel(r) + "|")
                        .Aggregate("", (a, b) => a + b);
                    var bbRoutes = board.RoutesFrom(currentCity, board.Belows(belowCity))
                        .Select(r => "||" + RouteLabel(r) + "||")
                        .Aggregate("", (a, b) => a + b);
                    nextLine += string.Format("{0,-" + horizontalSpacing + "}", bRoutes + bbRoutes);

                    var nextDiag = board.NextDiagOrDefault(currentCity);
                    var cdRoutes = board.RoutesFrom(currentCity, nextDiag)
                        .Select(r => @"\" + RouteLabel(r) + @"\")
                        .Aggregate("", (a, b) => a + b);

                    var cdcdRoutes = board.RoutesFrom(currentCity, board.NextDiags(nextDiag))
                        .Select(r => @"\\" + RouteLabel(r) + @"\\")
                        .Aggregate("", (a, b) => a + b);


                    var ndRoutes = board.RoutesFrom(nextCity, belowCity)
                        .Select(r => "/" + RouteLabel(r) + "/")
                        .Aggregate("", (a, b) => a + b);
                    var ndndRoutes = board.RoutesFrom(nextCity, board.PrevDiags(belowCity))
                        .Select(r => "//" + RouteLabel(r) + "//")
                        .Aggregate("", (a, b) => a + b);

                    nextLine += string.Format("{0,-" + horizontalSpacing + "}", cdRoutes + ndRoutes); // + cdcdRoutes + ndndRoutes  these are bad?
                }
                Console.WriteLine();
                Console.WriteLine(nextLine);
            }

            Console.WriteLine(new String('*', Console.WindowWidth));
        }
        protected void Render(PlayerAction subject)
        {
            Console.WriteLine($"{subject}");
        }

        protected void Render(DestinationCard subject)
        {
            Console.WriteLine($"{subject.Start} to {subject.End} worth {subject.Points}");
        }

        protected void Render(Route subject)
        {
            Console.WriteLine($"{subject.Source} to {subject.Target} {subject.Tag.Color} costing {subject.Tag.Length}");
        }

        protected void Render(TrainCard subject)
        {
            Console.WriteLine($"{subject.Color}");
        }

        protected IEnumerable<T> Pick<T>(IEnumerable<T> choices, int countMinimum, int countMaximum, Action<T> render)
        {
            var pickableChoices = choices.ToArray();
            foreach (var choice in pickableChoices.Select((Value, Index) => new { Value, Index }))
            {
                Console.Write($"{choice.Index}) ");
                render(choice.Value);
            }

            var choiceRules = countMaximum == countMinimum ?
                $"{countMaximum}" :
                $"at least {countMinimum} and at most {countMaximum}";

            Console.Write($"Choose {choiceRules} by entering their number(s)");
            IEnumerable<T> picks;
            do
            {
                var pickedIndex = new Regex(@"\D+").Split(Console.ReadLine())
                    .Select(pick => string.IsNullOrEmpty(pick) ? int.MinValue : int.Parse(pick));

                picks = choices.Where((c, i) => pickedIndex.Contains(i));
            } while (picks.Count() < countMinimum || picks.Count() > countMaximum);

            return picks;
        }

    }

}
