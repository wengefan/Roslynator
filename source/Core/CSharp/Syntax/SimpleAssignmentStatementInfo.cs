// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Syntax
{
    public struct SimpleAssignmentStatementInfo
    {
        private static SimpleAssignmentStatementInfo Default { get; } = new SimpleAssignmentStatementInfo();

        private SimpleAssignmentStatementInfo(
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

        public ExpressionStatementSyntax ExpressionStatement
        {
            get { return (ExpressionStatementSyntax)AssignmentExpression?.Parent; }
        }

        public bool Success
        {
            get { return AssignmentExpression != null; }
        }

        internal static SimpleAssignmentStatementInfo Create(
            SyntaxNode node,
            SyntaxInfoOptions options)
        {
            return Create(node as ExpressionStatementSyntax, options);
        }

        internal static SimpleAssignmentStatementInfo Create(
            ExpressionStatementSyntax expressionStatement,
            SyntaxInfoOptions options)
        {
            options = options ?? SyntaxInfoOptions.Default;

            ExpressionSyntax expression = options.WalkAndCheck(expressionStatement?.Expression);

            if (expression?.Kind() != SyntaxKind.SimpleAssignmentExpression)
                return Default;

            var assignmentExpression = (AssignmentExpressionSyntax)expression;

            ExpressionSyntax left = options.WalkAndCheck(assignmentExpression.Left);

            if (left == null)
                return Default;

            ExpressionSyntax right = options.WalkAndCheck(assignmentExpression.Right);

            if (right == null)
                return Default;

            return new SimpleAssignmentStatementInfo(assignmentExpression, left, right);
        }

        public override string ToString()
        {
            return ExpressionStatement?.ToString() ?? base.ToString();
        }
    }
}
