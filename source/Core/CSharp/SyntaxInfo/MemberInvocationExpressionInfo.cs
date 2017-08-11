// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Syntax
{
    public struct MemberInvocationExpressionInfo
    {
        public MemberInvocationExpressionInfo(ExpressionSyntax expression, SimpleNameSyntax name, ArgumentListSyntax argumentList)
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

        public static MemberInvocationExpressionInfo Create(InvocationExpressionSyntax invocationExpression)
        {
            if (invocationExpression == null)
                throw new ArgumentNullException(nameof(invocationExpression));

            ExpressionSyntax expression = invocationExpression.Expression;

            if (expression == null
                || !expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
            {
                throw new ArgumentException("", nameof(invocationExpression));
            }

            var memberAccessExpression = (MemberAccessExpressionSyntax)expression;

            return new MemberInvocationExpressionInfo(
                memberAccessExpression.Expression,
                memberAccessExpression.Name,
                invocationExpression.ArgumentList);
        }

        public static bool TryCreate(SyntaxNode invocationExpression, out MemberInvocationExpressionInfo info)
        {
            if (invocationExpression?.IsKind(SyntaxKind.InvocationExpression) == true)
                return TryCreateCore((InvocationExpressionSyntax)invocationExpression, out info);

            info = default(MemberInvocationExpressionInfo);
            return false;
        }

        public static bool TryCreate(InvocationExpressionSyntax invocationExpression, out MemberInvocationExpressionInfo info)
        {
            if (invocationExpression != null)
                return TryCreateCore(invocationExpression, out info);

            info = default(MemberInvocationExpressionInfo);
            return false;
        }

        private static bool TryCreateCore(InvocationExpressionSyntax invocationExpression, out MemberInvocationExpressionInfo info)
        {
            ExpressionSyntax expression = invocationExpression.Expression;

            if (expression?.IsKind(SyntaxKind.SimpleMemberAccessExpression) == true)
            {
                var memberAccess = (MemberAccessExpressionSyntax)expression;

                info = new MemberInvocationExpressionInfo(
                    memberAccess.Expression,
                    memberAccess.Name,
                    invocationExpression.ArgumentList);

                return true;
            }

            info = default(MemberInvocationExpressionInfo);
            return false;
        }

        public override string ToString()
        {
            return InvocationExpression?.ToString() ?? base.ToString();
        }
    }
}
