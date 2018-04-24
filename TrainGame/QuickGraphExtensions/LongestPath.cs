using QuickGraph;
using QuickGraph.Algorithms.Search;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TrainGame.QuickGraphExtensions
{
    public static class QuickGraphExtensions
    {

        public static IEnumerable<TEdge> LongestPathBruteDepthFirst<TVertex, TEdge>
            (
                this UndirectedGraph<TVertex, TEdge> graph,
                Func<TEdge,double> getEdgeWeight
            )
            where TEdge : IEdge<TVertex>
        {
            var search = new UndirectedDepthFirstSearchAlgorithm<TVertex, TEdge>(graph);
            var bestPath = new TEdge[0];
            var bestSum = 0d;
            foreach (var vertex in graph.Vertices)
            {
                if (bestPath.Any() && new[] { bestPath.First(), bestPath.Last() }.Any(r => r.Source.Equals(vertex) || r.Target.Equals(vertex)))
                    continue;

                var currentPath = new Stack<TEdge>();
                var currentSum = 0d;

                Action unCount = () =>
                {
                    if(currentPath.Any())
                    {
                        var edge = currentPath.Pop();
                        currentSum -= getEdgeWeight(edge);
                    }
                };

                UndirectedEdgeAction<TVertex, TEdge> countEdge = (sender, args) =>
                {
                    currentPath.Push(args.Edge);
                    currentSum += getEdgeWeight(args.Edge);

                    if (currentSum > bestSum)
                    {
                        bestPath = currentPath.ToArray();
                        bestSum = currentSum;
                    }
                };

                UndirectedEdgeAction<TVertex, TEdge> countUncountEdge = (sender, args) =>
                {
                    countEdge(sender, args);
                    unCount();
                };

                search.TreeEdge += countEdge;
                search.ForwardOrCrossEdge += countUncountEdge;
                search.BackEdge += countUncountEdge;
                search.FinishVertex += delegate { unCount(); };

                search.Compute(vertex);
            }

            return bestPath;
        }

    }
}
