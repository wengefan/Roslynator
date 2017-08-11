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
            return Create(
                (node as ExpressionSyntax)?.WalkDownParenthesesIf((options ?? SyntaxInfoOptions.Default).WalkDownParentheses) as AssignmentExpressionSyntax,
                options);
        }

        internal static SimpleAssignmentExpressionInfo Create(
            AssignmentExpressionSyntax assignmentExpression,
            SyntaxInfoOptions options = null)
        {
            options = options ?? SyntaxInfoOptions.Default;

            if (assignmentExpression?.Kind() != SyntaxKind.SimpleAssignmentExpression)
                return Default;

            ExpressionSyntax left = assignmentExpression.Left?.WalkDownParenthesesIf(options.WalkDownParentheses);

            if (!options.CheckNode(left))
                return Default;

            ExpressionSyntax right = assignmentExpression.Right?.WalkDownParenthesesIf(options.WalkDownParentheses);

            if (!options.CheckNode(right))
                return Default;

            return new SimpleAssignmentExpressionInfo(assignmentExpression, left, right);
        }

        public override string ToString()
        {
            return AssignmentExpression?.ToString() ?? base.ToString();
        }
    }
}
