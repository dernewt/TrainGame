using QuickGraph;
using QuickGraph.Algorithms.Search;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TrainGame.QuickGraphExtensions
{
    public static class QuickGraphExtensions
    {

        public static IEnumerable<TEdge> LongestPathDynamicProgramming<TVertex, TEdge>(this UndirectedGraph<TVertex, TEdge> graph, Func<TEdge, double> edgeWeights)
            where TEdge : IEdge<TVertex>
            where TVertex : class
        {
            var search = new UndirectedDepthFirstSearchAlgorithm<TVertex, TEdge>(graph);
            var bestPath = new List<TEdge>();
            var bestSum = 0d;
            foreach (var vertex in graph.Vertices)
            {
                if (new[] { bestPath.FirstOrDefault(), bestPath.LastOrDefault() }.Any(r => r.Source == vertex || r.Target == vertex))
                    continue;

                var currentPath = new List<TEdge>();
                var currentSum = 0d;

                var countEdge = new UndirectedEdgeAction<TVertex, TEdge>((sender, args) =>
                {
                    currentPath.Add(args.Edge);
                    currentSum += edgeWeights(args.Edge);
                });
                var unCountEdge = new UndirectedEdgeAction<TVertex, TEdge>((sender, args) =>
                {
                    currentPath.Remove(args.Edge);
                    currentSum -= edgeWeights(args.Edge);
                });

                //search.DiscoverVertex += new VertexAction<TVertex>(c => { });
                search.ExamineEdge += countEdge;
                //search.TreeEdge
                search.ForwardOrCrossEdge += unCountEdge;
                search.BackEdge += unCountEdge;

                search.Compute(vertex);
            }

            throw new NotImplementedException("TODO"); //TODO
            return bestPath;
        }

    }
}
