// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.Utilities;

namespace Roslynator.CSharp.Syntax
{
    public struct NullCheckExpressionInfo
    {
        private static NullCheckExpressionInfo Default { get; } = new NullCheckExpressionInfo();

        private NullCheckExpressionInfo(
            ExpressionSyntax node,
            ExpressionSyntax expression,
            NullCheckKind kind)
        {
            Node = node;
            Expression = expression;
            Kind = kind;
        }

        //TODO: 
        public ExpressionSyntax Node { get; }

        public ExpressionSyntax Expression { get; }

        public NullCheckKind Kind { get; }

        public bool IsCheckingNull
        {
            get { return (Kind & NullCheckKind.IsNull) != 0; }
        }

        public bool IsCheckingNotNull
        {
            get { return (Kind & NullCheckKind.IsNotNull) != 0; }
        }

        public bool Success
        {
            get { return Kind != NullCheckKind.None; }
        }

        internal static NullCheckExpressionInfo Create(
            SyntaxNode node,
            SyntaxInfoOptions options = null,
            NullCheckKind allowedKinds = NullCheckKind.All,
            SemanticModel semanticModel = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (semanticModel == null
                && (allowedKinds & NullCheckKind.HasValueProperty) != 0)
            {
                return Default;
            }

            options = options ?? SyntaxInfoOptions.Default;

            ExpressionSyntax expression = options.WalkAndCheck(node);

            if (expression == null)
                return Default;

            SyntaxKind kind = expression.Kind();

            switch (kind)
            {
                case SyntaxKind.EqualsExpression:
                case SyntaxKind.NotEqualsExpression:
                    {
                        var binaryExpression = (BinaryExpressionSyntax)expression;

                        ExpressionSyntax left = options.WalkAndCheck(binaryExpression.Left);

                        if (left == null)
                            break;

                        ExpressionSyntax right = options.WalkAndCheck(binaryExpression.Right);

                        if (right == null)
                            break;

                        NullCheckExpressionInfo info = Create(binaryExpression, kind, left, right, options, allowedKinds, semanticModel, cancellationToken);

                        if (info.Success)
                        {
                            return info;
                        }
                        else
                        {
                            return Create(binaryExpression, kind, right, left, options, allowedKinds, semanticModel, cancellationToken);
                        }
                    }
                case SyntaxKind.SimpleMemberAccessExpression:
                    {
                        if ((allowedKinds & NullCheckKind.HasValue) == 0)
                            break;

                        var memberAccessExpression = (MemberAccessExpressionSyntax)expression;

                        if (!IsPropertyOfNullableOfT(memberAccessExpression.Name, "HasValue", semanticModel, cancellationToken))
                            break;

                        return new NullCheckExpressionInfo(expression, memberAccessExpression.Expression, NullCheckKind.HasValue);
                    }
                case SyntaxKind.LogicalNotExpression:
                    {
                        if ((allowedKinds & NullCheckKind.NotHasValue) == 0)
                            break;

                        var logicalNotExpression = (PrefixUnaryExpressionSyntax)expression;

                        ExpressionSyntax operand = options.WalkAndCheck(logicalNotExpression.Operand);

                        if (!(operand is MemberAccessExpressionSyntax memberAccessExpression))
                            break;

                        if (memberAccessExpression.Kind() != SyntaxKind.SimpleMemberAccessExpression)
                            break;

                        if (!IsPropertyOfNullableOfT(memberAccessExpression.Name, "HasValue", semanticModel, cancellationToken))
                            break;

                        return new NullCheckExpressionInfo(expression, memberAccessExpression.Expression, NullCheckKind.NotHasValue);
                    }
            }

            return Default;
        }

        private static NullCheckExpressionInfo Create(
            BinaryExpressionSyntax binaryExpression,
            SyntaxKind binaryExpressionKind,
            ExpressionSyntax expression1,
            ExpressionSyntax expression2,
            SyntaxInfoOptions options,
            NullCheckKind allowedKinds,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            switch (expression1.Kind())
            {
                case SyntaxKind.NullLiteralExpression:
                    {
                        NullCheckKind kind = (binaryExpressionKind == SyntaxKind.EqualsExpression) ? NullCheckKind.EqualsToNull : NullCheckKind.NotEqualsToNull;

                        if ((allowedKinds & kind) == 0)
                            break;

                        return new NullCheckExpressionInfo(
                            binaryExpression,
                            expression2,
                            kind);
                    }
                case SyntaxKind.TrueLiteralExpression:
                    {
                        NullCheckKind kind = (binaryExpressionKind == SyntaxKind.EqualsExpression) ? NullCheckKind.HasValue : NullCheckKind.NotHasValue;

                        return Create(
                            binaryExpression,
                            expression2,
                            kind,
                            options,
                            allowedKinds,
                            semanticModel,
                            cancellationToken);
                    }
                case SyntaxKind.FalseLiteralExpression:
                    {
                        NullCheckKind kind = (binaryExpressionKind == SyntaxKind.EqualsExpression) ? NullCheckKind.NotHasValue : NullCheckKind.HasValue;

                        return Create(
                            binaryExpression,
                            expression2,
                            kind,
                            options,
                            allowedKinds,
                            semanticModel,
                            cancellationToken);
                    }
            }

            return Default;
        }

        private static NullCheckExpressionInfo Create(
            BinaryExpressionSyntax binaryExpression,
            ExpressionSyntax expression,
            NullCheckKind kind,
            SyntaxInfoOptions options,
            NullCheckKind allowedKinds,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            if ((allowedKinds & (NullCheckKind.HasValueProperty)) == 0)
                return Default;

            if (!(expression is MemberAccessExpressionSyntax memberAccessExpression))
                return Default;

            if (memberAccessExpression.Kind() != SyntaxKind.SimpleMemberAccessExpression)
                return Default;

            if (!IsPropertyOfNullableOfT(memberAccessExpression.Name, "HasValue", semanticModel, cancellationToken))
                return Default;

            if ((allowedKinds & kind) == 0)
                return Default;

            ExpressionSyntax expression2 = memberAccessExpression.Expression;

            if (!options.Check(expression2))
                return Default;

            return new NullCheckExpressionInfo(binaryExpression, expression2, kind);
        }

        private static bool IsPropertyOfNullableOfT(
            ExpressionSyntax expression,
            string name,
            SemanticModel semanticModel,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return expression?.IsKind(SyntaxKind.IdentifierName) == true
                && string.Equals(((IdentifierNameSyntax)expression).Identifier.ValueText, name, StringComparison.Ordinal)
                && SemanticUtilities.IsPropertyOfNullableOfT(expression, name, semanticModel, cancellationToken);
        }

        public override string ToString()
        {
            return Node?.ToString() ?? base.ToString();
        }
    }
}
