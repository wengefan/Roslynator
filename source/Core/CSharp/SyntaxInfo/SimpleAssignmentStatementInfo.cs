// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Roslynator.CSharp.SyntaxInfo.SyntaxInfoHelper;

namespace Roslynator.CSharp.SyntaxInfo
{
    public struct SimpleAssignmentStatementInfo
    {
        public SimpleAssignmentStatementInfo(
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

        public static SimpleAssignmentStatementInfo Create(
            ExpressionStatementSyntax expressionStatement,
            bool allowNullOrMissing = false,
            bool walkDownParentheses = true)
        {
            if (expressionStatement == null)
                throw new ArgumentNullException(nameof(expressionStatement));

            ExpressionSyntax expression = expressionStatement.Expression;

            if (expression.Kind() != SyntaxKind.SimpleAssignmentExpression)
                throw new ArgumentException("", nameof(expressionStatement));

            var assignmentExpression = (AssignmentExpressionSyntax)expression;

            ExpressionSyntax left = assignmentExpression.Left?.WalkDownParenthesesIf(walkDownParentheses);

            if (!CheckNode(left, allowNullOrMissing))
                throw new ArgumentException("", nameof(expressionStatement));

            ExpressionSyntax right = assignmentExpression.Right?.WalkDownParenthesesIf(walkDownParentheses);

            if (!CheckNode(right, allowNullOrMissing))
                throw new ArgumentException("", nameof(expressionStatement));

            return new SimpleAssignmentStatementInfo(assignmentExpression, assignmentExpression.Left, assignmentExpression.Right);
        }

        public static bool TryCreate(
            SyntaxNode node,
            out SimpleAssignmentStatementInfo info,
            bool allowNullOrMissing = false,
            bool walkDownParentheses = true)
        {
            return TryCreate(
                node as ExpressionStatementSyntax,
                out info,
                allowNullOrMissing: allowNullOrMissing,
                walkDownParentheses: walkDownParentheses);
        }

        public static bool TryCreate(
            ExpressionStatementSyntax expressionStatement,
            out SimpleAssignmentStatementInfo info,
            bool allowNullOrMissing = false,
            bool walkDownParentheses = true)
        {
            ExpressionSyntax expression = expressionStatement?.Expression?.WalkDownParenthesesIf(walkDownParentheses);

            if (expression?.Kind() == SyntaxKind.SimpleAssignmentExpression)
            {
                var assignmentExpression = (AssignmentExpressionSyntax)expression;

                ExpressionSyntax left = assignmentExpression.Left?.WalkDownParenthesesIf(walkDownParentheses);

                if (CheckNode(left, allowNullOrMissing))
                {
                    ExpressionSyntax right = assignmentExpression.Right?.WalkDownParenthesesIf(walkDownParentheses);

                    if (CheckNode(right, allowNullOrMissing))
                    {
                        info = new SimpleAssignmentStatementInfo(assignmentExpression, left, right);
                        return true;
                    }
                }
            }

            info = default(SimpleAssignmentStatementInfo);
            return false;
        }

        public override string ToString()
        {
            return ExpressionStatement?.ToString() ?? base.ToString();
        }
    }
}
