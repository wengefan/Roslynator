// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Roslynator.CSharp.Refactorings.ReduceIfNesting
{
    internal struct ReduceIfNestingAnalysis
    {
        public ReduceIfNestingAnalysis(SyntaxNode topNode, SyntaxKind jumpKind)
        {
            Debug.Assert(jumpKind == SyntaxKind.ReturnStatement
                || jumpKind == SyntaxKind.NullLiteralExpression
                || jumpKind == SyntaxKind.FalseLiteralExpression
                || jumpKind == SyntaxKind.TrueLiteralExpression
                || jumpKind == SyntaxKind.BreakStatement
                || jumpKind == SyntaxKind.ContinueStatement
                || jumpKind == SyntaxKind.ThrowStatement
                || jumpKind == SyntaxKind.YieldBreakStatement, jumpKind.ToString());

            TopNode = topNode;
            JumpKind = jumpKind;
        }

        public SyntaxNode TopNode { get; }

        public SyntaxKind JumpKind { get; }

        public bool Success
        {
            get { return JumpKind != SyntaxKind.None; }
        }
    }
}
