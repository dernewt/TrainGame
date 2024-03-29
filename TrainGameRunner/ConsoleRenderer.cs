﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TrainGame;
using TrainGame.Players;
using TrainGame.Rules;
using System.Text;
using QuickGraph;

namespace TrainGameRunner;

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
        var cols = CityFactory.CityLayout.Max(r => r.Length);
        var horizontalSpacing = Console.WindowWidth / (cols * 2 - 1);

        Console.WriteLine(new string('*', Console.WindowWidth));

        foreach (var row in board.Map)
        {
            var currentLine = new StringBuilder(Console.WindowWidth);
            var nextLine = new StringBuilder(Console.WindowWidth);
            foreach (var currentCity in row)
            {
                var nextCitys = board.Nexts(currentCity);
                var nextRoutes = board.RoutesFrom(currentCity, nextCitys)
                    .Select(r => r.IsAdjacent(nextCitys.FirstOrDefault())
                    ? $"-{RouteLabel(r)}-"
                    : $"={RouteLabel(r)}=");

                currentLine.AppendColumn(horizontalSpacing, currentCity);

                var belowCities = board.Belows(currentCity);
                var belowRoutes = board.RoutesFrom(currentCity, belowCities)
                .Select(r => r.IsAdjacent(belowCities.FirstOrDefault())
                ? $"|{RouteLabel(r)}|"
                : $"||{RouteLabel(r)}||");

                var diagnalCities = board.NextDiags(currentCity);
                var diagnalRoutes = board.RoutesFrom(currentCity, diagnalCities)
                    .Select(r => r.IsAdjacent(diagnalCities.FirstOrDefault())
                    ? $"\\{RouteLabel(r)}\\"
                    : $"\\\\{RouteLabel(r)}\\\\");


                var reverseDiagnalCities = board.PrevDiags(nextCitys.FirstOrDefault());
                var reverseDiagnalRoutes = board.RoutesFrom(nextCitys.FirstOrDefault(), reverseDiagnalCities)
                    .Select(r => r.IsAdjacent(reverseDiagnalCities.FirstOrDefault())
                    ? $"/{RouteLabel(r)}/"
                    : $"//{RouteLabel(r)}//");

                nextLine.AppendColumn(horizontalSpacing, belowRoutes);

                if (nextCitys.Any())
                {
                    currentLine.AppendColumn(horizontalSpacing, nextRoutes);
                    nextLine.AppendColumn(horizontalSpacing, diagnalRoutes, reverseDiagnalRoutes);
                }
            }

            Console.WriteLine(currentLine);
            if (row != board.Map.Last())
                Console.WriteLine(nextLine);

        }

        Console.WriteLine(new string('*', Console.WindowWidth));
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

public static class ConsoleHelper
{
    public static StringBuilder AppendColumn(this StringBuilder target, int width, City city)
        => AppendColumn(target, width, new[] { city.ToString() });
    public static StringBuilder AppendColumn(this StringBuilder target, int width, params IEnumerable<string>[] routes)
    {
        var routeString = routes.SelectMany(r => r).Aggregate(new StringBuilder(), (a, b) => a.Append(b)).ToString();

        if (routeString.Length > width)
            routeString = routeString[..width];

        return target.AppendFormat($"{{0,-{width}}}", routeString);
    }
}