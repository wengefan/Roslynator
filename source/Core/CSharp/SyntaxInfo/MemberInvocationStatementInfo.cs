// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Roslynator.CSharp.SyntaxInfo.SyntaxInfoHelper;

namespace Roslynator.CSharp.SyntaxInfo
{
    public struct MemberInvocationStatementInfo
    {
        public MemberInvocationStatementInfo(
            InvocationExpressionSyntax invocationExpression,
            ExpressionSyntax expression,
            SimpleNameSyntax name,
            ArgumentListSyntax argumentList)
        {
            InvocationExpression = invocationExpression;
            Expression = expression;
            Name = name;
            ArgumentList = argumentList;
        }

        public InvocationExpressionSyntax InvocationExpression { get; }

        public ExpressionSyntax Expression { get; }

        public SimpleNameSyntax Name { get; }

        public ArgumentListSyntax ArgumentList { get; }

        public ExpressionStatementSyntax ExpressionStatement
        {
            get { return (ExpressionStatementSyntax)InvocationExpression?.Parent; }
        }

        public MemberAccessExpressionSyntax MemberAccessExpression
        {
            get { return (MemberAccessExpressionSyntax)Expression?.Parent; }
        }

        public string NameText
        {
            get { return Name?.Identifier.ValueText; }
        }

        public static MemberInvocationStatementInfo Create(
            ExpressionStatementSyntax expressionStatement,
            bool allowNullOrMissing = false)
        {
            if (expressionStatement == null)
                throw new ArgumentNullException(nameof(expressionStatement));

            var invocationExpression = expressionStatement.Expression as InvocationExpressionSyntax;

            if (invocationExpression == null)
                throw new ArgumentException("", nameof(expressionStatement));

            ExpressionSyntax expression = invocationExpression.Expression;

            if (expression?.IsKind(SyntaxKind.SimpleMemberAccessExpression) != true)
                throw new ArgumentException("", nameof(expressionStatement));

            var memberAccessExpression = (MemberAccessExpressionSyntax)expression;

            ExpressionSyntax expression2 = memberAccessExpression.Expression;

            if (!CheckNode(expression2, allowNullOrMissing))
                throw new ArgumentException("", nameof(expressionStatement));

            SimpleNameSyntax name = memberAccessExpression.Name;

            if (!CheckNode(name, allowNullOrMissing))
                throw new ArgumentException("", nameof(expressionStatement));

            return new MemberInvocationStatementInfo(
                invocationExpression,
                expression2,
                name,
                invocationExpression.ArgumentList);
        }

        public static bool TryCreate(
            SyntaxNode invocationStatement,
            out MemberInvocationStatementInfo info,
            bool allowNullOrMissing = false)
        {
            if (invocationStatement?.IsKind(SyntaxKind.ExpressionStatement) == true)
                return TryCreateCore((ExpressionStatementSyntax)invocationStatement, out info, allowNullOrMissing: allowNullOrMissing);

            info = default(MemberInvocationStatementInfo);
            return false;
        }

        public static bool TryCreate(
            ExpressionStatementSyntax invocationStatement,
            out MemberInvocationStatementInfo info,
            bool allowNullOrMissing = false)
        {
            if (invocationStatement != null)
                return TryCreateCore(invocationStatement, out info, allowNullOrMissing: allowNullOrMissing);

            info = default(MemberInvocationStatementInfo);
            return false;
        }

        private static bool TryCreateCore(
            ExpressionStatementSyntax expressionStatement,
            out MemberInvocationStatementInfo info,
            bool allowNullOrMissing = false)
        {
            if (expressionStatement.Expression is InvocationExpressionSyntax invocationExpression)
            {
                ExpressionSyntax expression = invocationExpression.Expression;

                if (expression?.IsKind(SyntaxKind.SimpleMemberAccessExpression) == true)
                {
                    var memberAccessExpression = (MemberAccessExpressionSyntax)expression;

                    ExpressionSyntax expression2 = memberAccessExpression.Expression;

                    if (CheckNode(expression2, allowNullOrMissing))
                    {
                        SimpleNameSyntax name = memberAccessExpression.Name;

                        if (CheckNode(name, allowNullOrMissing))
                        {
                            info = new MemberInvocationStatementInfo(
                                invocationExpression,
                                expression2,
                                name,
                                invocationExpression.ArgumentList);

                            return true;
                        }
                    }
                }
            }

            info = default(MemberInvocationStatementInfo);
            return false;
        }

        public override string ToString()
        {
            return ExpressionStatement?.ToString() ?? base.ToString();
        }
    }
}
