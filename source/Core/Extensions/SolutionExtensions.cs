// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Roslynator
{
    internal static class SolutionExtensions
    {
        public static async Task<Solution> ReplaceNodesAsync<TNode>(
            this Solution solution,
            IEnumerable<TNode> nodes,
            Func<TNode, TNode, SyntaxNode> computeReplacementNodes,
            CancellationToken cancellationToken = default(CancellationToken)) where TNode : SyntaxNode
        {
            Solution newSolution = solution;

            foreach (IGrouping<SyntaxTree, TNode> grouping in nodes.GroupBy(f => f.SyntaxTree))
            {
                Document document = solution.GetDocument(grouping.Key);

                SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

                SyntaxNode newRoot = root.ReplaceNodes(grouping, computeReplacementNodes);

                newSolution = newSolution.WithDocumentSyntaxRoot(document.Id, newRoot);
            }

            return newSolution;
        }
    }
}
