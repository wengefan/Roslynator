// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Syntax
{
    public struct SimpleAssignmentExpressionInfo
    {
        private static SimpleAssignmentExpressionInfo Default { get; } = new SimpleAssignmentExpressionInfo();

        private SimpleAssignmentExpressionInfo(
            AssignmentExpressionSyntax assignmentExpression,
            ExpressionSyntax left,
            ExpressionSyntax right)
        {
            AssignmentExpression = assignmentExpression;
            Left = left;
            Right = right;
        }

        public AssignmentExpressionSyntax AssignmentExpression { get; }

        public ExpressionSyntax Left { get; }

        public ExpressionSyntax Right { get; }

        public bool Success
        {
            get { return AssignmentExpression != null; }
        }

        internal static SimpleAssignmentExpressionInfo Create(
            SyntaxNode node,
            SyntaxInfoOptions options = null)
        {
            options = options ?? SyntaxInfoOptions.Default;

            return CreateCore(options.Walk(node) as AssignmentExpressionSyntax, options);
        }

        internal static SimpleAssignmentExpressionInfo Create(
            AssignmentExpressionSyntax assignmentExpression,
            SyntaxInfoOptions options = null)
        {
            return CreateCore(assignmentExpression, options ?? SyntaxInfoOptions.Default);
        }

        internal static SimpleAssignmentExpressionInfo CreateCore(
            AssignmentExpressionSyntax assignmentExpression,
            SyntaxInfoOptions options = null)
        {
            if (assignmentExpression?.Kind() != SyntaxKind.SimpleAssignmentExpression)
                return Default;

            ExpressionSyntax left = options.WalkAndCheck(assignmentExpression.Left);

            if (left == null)
                return Default;

            ExpressionSyntax right = options.WalkAndCheck(assignmentExpression.Right);

            if (right == null)
                return Default;

            return new SimpleAssignmentExpressionInfo(assignmentExpression, left, right);
        }

        public override string ToString()
        {
            return AssignmentExpression?.ToString() ?? base.ToString();
        }
    }
}
