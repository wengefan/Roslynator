// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.CodeFixes
{
    internal static class AddCastExpressionRefactoring
    {
        public static Task<Document> RefactorAsync(
            Document document,
            ExpressionSyntax expression,
            ITypeSymbol destinationType,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            CastExpressionSyntax newNode = SyntaxFactory.CastExpression(
                destinationType.ToMinimalTypeSyntax(semanticModel, expression.SpanStart),
                expression
                    .WithoutTrivia()
                    .Parenthesize()
                    .WithSimplifierAnnotation());

            newNode = newNode.WithTriviaFrom(expression);

            return document.ReplaceNodeAsync(expression, newNode, cancellationToken);
        }
    }
}