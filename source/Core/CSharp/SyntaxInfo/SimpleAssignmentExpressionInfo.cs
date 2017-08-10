// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Roslynator.CSharp.SyntaxInfo.SyntaxInfoHelper;

namespace Roslynator.CSharp.SyntaxInfo
{
    public struct SimpleAssignmentExpressionInfo
    {
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

        public static SimpleAssignmentExpressionInfo Create(
            AssignmentExpressionSyntax assignmentExpression,
            bool allowNullOrMissing = false,
            bool walkDownParentheses = true)
        {
            if (assignmentExpression == null)
                throw new ArgumentNullException(nameof(assignmentExpression));

            if (assignmentExpression.Kind() != SyntaxKind.SimpleAssignmentExpression)
                throw new ArgumentException("", nameof(assignmentExpression));

            ExpressionSyntax left = assignmentExpression.Left?.WalkDownParenthesesIf(walkDownParentheses);

            if (!CheckNode(left, allowNullOrMissing))
                throw new ArgumentException("", nameof(assignmentExpression));

            ExpressionSyntax right = assignmentExpression.Right?.WalkDownParenthesesIf(walkDownParentheses);

            if (!CheckNode(right, allowNullOrMissing))
                throw new ArgumentException("", nameof(assignmentExpression));

            return new SimpleAssignmentExpressionInfo(assignmentExpression, assignmentExpression.Left, assignmentExpression.Right);
        }

        public static bool TryCreate(
            SyntaxNode node,
            out SimpleAssignmentExpressionInfo info,
            bool allowNullOrMissing = false,
            bool walkDownParentheses = true)
        {
            return TryCreate(
                (node as ExpressionSyntax)?.WalkDownParenthesesIf(walkDownParentheses) as AssignmentExpressionSyntax,
                out info,
                allowNullOrMissing,
                walkDownParentheses);
        }

        public static bool TryCreate(
            AssignmentExpressionSyntax assignmentExpression,
            out SimpleAssignmentExpressionInfo info,
            bool allowNullOrMissing = false,
            bool walkDownParentheses = true)
        {
            if (assignmentExpression?.Kind() == SyntaxKind.SimpleAssignmentExpression)
            {
                ExpressionSyntax left = assignmentExpression.Left?.WalkDownParenthesesIf(walkDownParentheses);

                if (CheckNode(left, allowNullOrMissing))
                {
                    ExpressionSyntax right = assignmentExpression.Right?.WalkDownParenthesesIf(walkDownParentheses);

                    if (CheckNode(right, allowNullOrMissing))
                    {
                        info = new SimpleAssignmentExpressionInfo(assignmentExpression, left, right);
                        return true;
                    }
                }
            }

            info = default(SimpleAssignmentExpressionInfo);
            return false;
        }

        public override string ToString()
        {
            return AssignmentExpression?.ToString() ?? base.ToString();
        }
    }
}
