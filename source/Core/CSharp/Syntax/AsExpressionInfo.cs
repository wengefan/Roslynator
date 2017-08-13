// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Syntax
{
    public struct AsExpressionInfo
    {
        private static AsExpressionInfo Default { get; } = new AsExpressionInfo();

        private AsExpressionInfo(
            ExpressionSyntax expression,
            TypeSyntax type)
        {
            Expression = expression;
            Type = type;
        }

        public BinaryExpressionSyntax AsExpression
        {
            get { return Expression?.FirstAncestor<BinaryExpressionSyntax>(); }
        }

        public ExpressionSyntax Expression { get; }

        public TypeSyntax Type { get; }

        public bool Success
        {
            get { return Expression != null; }
        }

        internal static AsExpressionInfo Create(
            SyntaxNode node,
            SyntaxInfoOptions options = null)
        {
            options = options ?? SyntaxInfoOptions.Default;

            return CreateCore(
                options.Walk(node) as BinaryExpressionSyntax,
                options);
        }

        internal static AsExpressionInfo Create(
            BinaryExpressionSyntax binaryExpression,
            SyntaxInfoOptions options = null)
        {
            return CreateCore(binaryExpression, options ?? SyntaxInfoOptions.Default);
        }

        internal static AsExpressionInfo CreateCore(
            BinaryExpressionSyntax binaryExpression,
            SyntaxInfoOptions options)
        {
            if (binaryExpression?.Kind() != SyntaxKind.AsExpression)
                return Default;

            ExpressionSyntax expression = options.Walk(binaryExpression.Left);

            if (!options.Check(expression))
                return Default;

            var type = binaryExpression.Right as TypeSyntax;

            if (!options.Check(type))
                return Default;

            return new AsExpressionInfo(expression, type);
        }

        public override string ToString()
        {
            return AsExpression.ToString() ?? base.ToString();
        }
    }
}