// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Syntax
{
    public struct BinaryExpressionInfo
    {
        private static BinaryExpressionInfo Default { get; } = new BinaryExpressionInfo();

        private BinaryExpressionInfo(
            BinaryExpressionSyntax binaryExpression,
            ExpressionSyntax left,
            ExpressionSyntax right)
        {
            BinaryExpression = binaryExpression;
            Left = left;
            Right = right;
        }

        public BinaryExpressionSyntax BinaryExpression { get; }

        public ExpressionSyntax Left { get; }

        public ExpressionSyntax Right { get; }

        public SyntaxKind Kind
        {
            get { return BinaryExpression?.Kind() ?? SyntaxKind.None; }
        }

        public bool Success
        {
            get { return BinaryExpression != null; }
        }

        internal static BinaryExpressionInfo Create(
            SyntaxNode node,
            SyntaxInfoOptions options = null)
        {
            options = options ?? SyntaxInfoOptions.Default;

            return CreateCore(
                options.Walk(node) as BinaryExpressionSyntax,
                options);
        }

        internal static BinaryExpressionInfo Create(
            BinaryExpressionSyntax binaryExpression,
            SyntaxInfoOptions options = null)
        {
            return CreateCore(binaryExpression, options ?? SyntaxInfoOptions.Default);
        }

        internal static BinaryExpressionInfo CreateCore(
            BinaryExpressionSyntax binaryExpression,
            SyntaxInfoOptions options)
        {
            if (binaryExpression == null)
                return Default;

            ExpressionSyntax left = options.Walk(binaryExpression.Left);

            if (!options.Check(left))
                return Default;

            ExpressionSyntax right = options.Walk(binaryExpression.Right);

            if (!options.Check(right))
                return Default;

            return new BinaryExpressionInfo(binaryExpression, left, right);
        }

        public override string ToString()
        {
            return BinaryExpression?.ToString() ?? base.ToString();
        }
    }
}