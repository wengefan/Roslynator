// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Syntax
{
    public struct MemberInvocationExpressionInfo
    {
        private static MemberInvocationExpressionInfo Default { get; } = new MemberInvocationExpressionInfo();

        private MemberInvocationExpressionInfo(
            ExpressionSyntax expression,
            SimpleNameSyntax name,
            ArgumentListSyntax argumentList)
        {
            Expression = expression;
            Name = name;
            ArgumentList = argumentList;
        }

        public ExpressionSyntax Expression { get; }

        public SimpleNameSyntax Name { get; }

        public ArgumentListSyntax ArgumentList { get; }

        public InvocationExpressionSyntax InvocationExpression
        {
            get { return (InvocationExpressionSyntax)ArgumentList?.Parent; }
        }

        public MemberAccessExpressionSyntax MemberAccessExpression
        {
            get { return (MemberAccessExpressionSyntax)Expression?.Parent; }
        }

        public SeparatedSyntaxList<ArgumentSyntax> Arguments
        {
            get { return ArgumentList?.Arguments ?? default(SeparatedSyntaxList<ArgumentSyntax>); }
        }

        public SyntaxToken OperatorToken
        {
            get { return MemberAccessExpression?.OperatorToken ?? default(SyntaxToken); }
        }

        public string NameText
        {
            get { return Name?.Identifier.ValueText; }
        }

        public bool Success
        {
            get { return Expression != null; }
        }

        internal static MemberInvocationExpressionInfo Create(
            SyntaxNode node,
            SyntaxInfoOptions options = null)
        {
            options = options ?? SyntaxInfoOptions.Default;

            return CreateCore(
                options.Walk(node) as InvocationExpressionSyntax,
                options);
        }

        internal static MemberInvocationExpressionInfo Create(
            InvocationExpressionSyntax invocationExpression,
            SyntaxInfoOptions options = null)
        {
            return CreateCore(invocationExpression, options ?? SyntaxInfoOptions.Default);
        }

        internal static MemberInvocationExpressionInfo CreateCore(
            InvocationExpressionSyntax invocationExpression,
            SyntaxInfoOptions options)
        {
            if (!(invocationExpression?.Expression is MemberAccessExpressionSyntax memberAccessExpression))
                return Default;

            if (memberAccessExpression.Kind() != SyntaxKind.SimpleMemberAccessExpression)
                return Default;

            ExpressionSyntax expression = memberAccessExpression.Expression;

            if (!options.Check(expression))
                return Default;

            SimpleNameSyntax name = memberAccessExpression.Name;

            if (!options.Check(name))
                return Default;

            ArgumentListSyntax argumentList = invocationExpression.ArgumentList;

            if (argumentList == null)
                return Default;

            return new MemberInvocationExpressionInfo(
                expression,
                name,
                argumentList);
        }

        public override string ToString()
        {
            return InvocationExpression?.ToString() ?? base.ToString();
        }
    }
}
