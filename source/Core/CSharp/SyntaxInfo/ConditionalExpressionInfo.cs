// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Syntax
{
    public struct ConditionalExpressionInfo
    {
        private static ConditionalExpressionInfo Default { get; } = new ConditionalExpressionInfo();

        public ConditionalExpressionInfo(
            ExpressionSyntax condition,
            ExpressionSyntax whenTrue,
            ExpressionSyntax whenFalse)
        {
            Condition = condition;
            WhenTrue = whenTrue;
            WhenFalse = whenFalse;
        }

        public ConditionalExpressionSyntax ConditionalExpression
        {
            get { return Condition.FirstAncestor<ConditionalExpressionSyntax>(); }
        }

        public ExpressionSyntax Condition { get; }

        public ExpressionSyntax WhenTrue { get; }

        public ExpressionSyntax WhenFalse { get; }

        public bool Success
        {
            get { return Condition != null; }
        }

        internal static ConditionalExpressionInfo Create(
            SyntaxNode node,
            SyntaxInfoOptions options = null)
        {
            return Create(
                (node as ExpressionSyntax)?.WalkDownParenthesesIf((options ?? SyntaxInfoOptions.Default).WalkDownParentheses) as ConditionalExpressionSyntax,
                options);
        }

        internal static ConditionalExpressionInfo Create(
            ConditionalExpressionSyntax conditionalExpression,
            SyntaxInfoOptions options = null)
        {
            options = options ?? SyntaxInfoOptions.Default;

            if (conditionalExpression == null)
                return Default;

            ExpressionSyntax condition = conditionalExpression.Condition?.WalkDownParenthesesIf(options.WalkDownParentheses);

            if (!options.CheckNode(condition))
                return Default;

            ExpressionSyntax whenTrue = conditionalExpression.WhenTrue?.WalkDownParenthesesIf(options.WalkDownParentheses);

            if (!options.CheckNode(whenTrue))
                return Default;

            ExpressionSyntax whenFalse = conditionalExpression.WhenFalse?.WalkDownParenthesesIf(options.WalkDownParentheses);

            if (!options.CheckNode(whenFalse))
                return Default;

            return new ConditionalExpressionInfo(condition, whenTrue, whenFalse);
        }

        public override string ToString()
        {
            return ConditionalExpression?.ToString() ?? base.ToString();
        }
    }
}