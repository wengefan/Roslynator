﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.Refactorings
{
    internal static class UseElementAccessInsteadOfLastRefactoring
    {
        public static bool CanRefactor(
            MemberInvocationExpressionInfo memberInvocation,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            if (memberInvocation.Expression?.IsMissing != false)
                return false;

            if (!semanticModel.TryGetExtensionMethodInfo(memberInvocation.InvocationExpression, out MethodInfo methodInfo, ExtensionMethodKind.Reduced, cancellationToken))
                return false;

            if (!methodInfo.IsLinqExtensionOfIEnumerableOfTWithoutParameters("Last", allowImmutableArrayExtension: true))
                return false;

            ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(memberInvocation.Expression, cancellationToken);

            return CSharpUtility.HasAccessibleIndexer(typeSymbol, semanticModel, memberInvocation.InvocationExpression.SpanStart);
        }

        public static Task<Document> RefactorAsync(
            Document document,
            InvocationExpressionSyntax invocation,
            string propertyName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ArgumentListSyntax argumentList = invocation.ArgumentList;

            var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;

            ExpressionSyntax expression = memberAccess.Expression;

            IEnumerable<SyntaxTrivia> trivia = memberAccess.DescendantTrivia(TextSpan.FromBounds(memberAccess.Expression.Span.End, memberAccess.Name.FullSpan.End));

            if (trivia.All(f => f.IsWhitespaceOrEndOfLineTrivia()))
            {
                expression = expression.WithoutTrailingTrivia();
            }
            else
            {
                expression = expression.WithTrailingTrivia(trivia);
            }

            ExpressionSyntax argumentExpression = SubtractExpression(
                SimpleMemberAccessExpression(
                    expression,
                    IdentifierName(propertyName)),
                NumericLiteralExpression(1));

            ElementAccessExpressionSyntax elementAccess = ElementAccessExpression(
                expression,
                BracketedArgumentList(
                    OpenBracketToken().WithTriviaFrom(argumentList.OpenParenToken),
                    SingletonSeparatedList(Argument(argumentExpression)),
                    CloseBracketToken().WithTriviaFrom(argumentList.CloseParenToken)));

            return document.ReplaceNodeAsync(invocation, elementAccess, cancellationToken);
        }
    }
}