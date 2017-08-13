// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Syntax
{
    public struct ConditionalExpressionInfo
    {
        private static ConditionalExpressionInfo Default { get; } = new ConditionalExpressionInfo();

        private ConditionalExpressionInfo(
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
            get { return Condition?.FirstAncestor<ConditionalExpressionSyntax>(); }
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
            options = options ?? SyntaxInfoOptions.Default;

            return CreateCore(
                options.Walk(node) as ConditionalExpressionSyntax,
                options);
        }

        internal static ConditionalExpressionInfo Create(
            ConditionalExpressionSyntax conditionalExpression,
            SyntaxInfoOptions options = null)
        {
            return CreateCore(conditionalExpression, options ?? SyntaxInfoOptions.Default);
        }

        internal static ConditionalExpressionInfo CreateCore(
            ConditionalExpressionSyntax conditionalExpression,
            SyntaxInfoOptions options)
        {
            if (conditionalExpression == null)
                return Default;

            ExpressionSyntax condition = options.WalkAndCheck(conditionalExpression.Condition);

            if (condition == null)
                return Default;

            ExpressionSyntax whenTrue = options.WalkAndCheck(conditionalExpression.WhenTrue);

            if (whenTrue == null)
                return Default;

            ExpressionSyntax whenFalse = options.WalkAndCheck(conditionalExpression.WhenFalse);

            if (whenFalse == null)
                return Default;

            return new ConditionalExpressionInfo(condition, whenTrue, whenFalse);
        }

        public override string ToString()
        {
            return ConditionalExpression?.ToString() ?? base.ToString();
        }
    }
}