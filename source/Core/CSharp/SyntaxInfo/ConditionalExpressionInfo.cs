// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Roslynator.CSharp.SyntaxInfo.SyntaxInfoHelper;

namespace Roslynator.CSharp.SyntaxInfo
{
    public struct ConditionalExpressionInfo
    {
        public ConditionalExpressionInfo(
            ExpressionSyntax condition,
            ExpressionSyntax whenTrue,
            ExpressionSyntax whenFalse)
        {
            Condition = condition;
            WhenTrue = whenTrue;
            WhenFalse = whenFalse;
        }

        public ExpressionSyntax Condition { get; }

        public ExpressionSyntax WhenTrue { get; }

        public ExpressionSyntax WhenFalse { get; }

        public ConditionalExpressionSyntax Node
        {
            get { return Condition.FirstAncestor<ConditionalExpressionSyntax>(); }
        }

        public static ConditionalExpressionInfo Create(
            ConditionalExpressionSyntax conditionalExpression,
            bool allowNullOrMissing = false,
            bool walkDownParentheses = true)
        {
            if (conditionalExpression == null)
                throw new ArgumentNullException(nameof(conditionalExpression));

            ExpressionSyntax condition = conditionalExpression.Condition?.WalkDownParenthesesIf(walkDownParentheses);

            if (!CheckNode(condition, allowNullOrMissing))
                throw new ArgumentException("", nameof(conditionalExpression));

            ExpressionSyntax whenTrue = conditionalExpression.WhenTrue?.WalkDownParenthesesIf(walkDownParentheses);

            if (!CheckNode(whenTrue, allowNullOrMissing))
                throw new ArgumentException("", nameof(conditionalExpression));

            ExpressionSyntax whenFalse = conditionalExpression.WhenFalse?.WalkDownParenthesesIf(walkDownParentheses);

            if (!CheckNode(whenFalse, allowNullOrMissing))
                throw new ArgumentException("", nameof(conditionalExpression));

            return new ConditionalExpressionInfo(condition, whenTrue, whenFalse);
        }

        public static bool TryCreate(
            SyntaxNode node,
            out ConditionalExpressionInfo info,
            bool allowNullOrMissing = false,
            bool walkDownParentheses = true)
        {
            ExpressionSyntax expression = (node as ExpressionSyntax)?.WalkDownParenthesesIf(walkDownParentheses);

            if (expression?.IsKind(SyntaxKind.ConditionalExpression) == true)
                return TryCreate((ConditionalExpressionSyntax)expression, out info, allowNullOrMissing: allowNullOrMissing, walkDownParentheses: walkDownParentheses);

            info = default(ConditionalExpressionInfo);
            return false;
        }

        public static bool TryCreate(
            ConditionalExpressionSyntax conditionalExpression,
            out ConditionalExpressionInfo info,
            bool allowNullOrMissing = false,
            bool walkDownParentheses = true)
        {
            if (conditionalExpression != null)
            {
                ExpressionSyntax condition = conditionalExpression.Condition?.WalkDownParenthesesIf(walkDownParentheses);

                if (CheckNode(condition, allowNullOrMissing))
                {
                    ExpressionSyntax whenTrue = conditionalExpression.WhenTrue?.WalkDownParenthesesIf(walkDownParentheses);

                    if (CheckNode(whenTrue, allowNullOrMissing))
                    {
                        ExpressionSyntax whenFalse = conditionalExpression.WhenFalse?.WalkDownParenthesesIf(walkDownParentheses);

                        if (CheckNode(whenFalse, allowNullOrMissing))
                        {
                            info = new ConditionalExpressionInfo(condition, whenTrue, whenFalse);
                            return true;
                        }
                    }
                }
            }

            info = default(ConditionalExpressionInfo);
            return false;
        }

        public override string ToString()
        {
            return Node?.ToString() ?? base.ToString();
        }
    }
}