// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Refactorings
{
    internal static class UseDefaultValueInsteadOfDefaultExpressionRefactoring
    {
        public static void AnalyzeDefaultExpression(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.SpanContainsDirectives())
                return;

            var defaultExpression = (DefaultExpressionSyntax)context.Node;

            TypeSyntax type = defaultExpression.Type;

            if (type != null)
            {
                ITypeSymbol typeSymbol = context.SemanticModel.GetTypeSymbol(type, context.CancellationToken);

                if (typeSymbol.IsPredefinedValueType())
                {
                    context.ReportDiagnostic(
                        DiagnosticDescriptors.UseDefaultValueInsteadOfDefaultExpression,
                        defaultExpression);
                }
            }
        }

        public static async Task<Document> RefactorAsync(
            Document document,
            DefaultExpressionSyntax defaultExpression,
            CancellationToken cancellationToken)
        {
            TypeSyntax type = defaultExpression.Type;

            SemanticModel semanticModel = await document.GetSemanticModelAsync().ConfigureAwait(false);

            ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(type, cancellationToken);

            ExpressionSyntax defaultValue = typeSymbol.ToDefaultValueSyntax(type);

            CastExpressionSyntax castExpression = SyntaxFactory.CastExpression(type.TrimTrivia(), defaultValue)
                .WithTriviaFrom(defaultExpression)
                .WithSimplifierAnnotation();

            return await document.ReplaceNodeAsync(defaultExpression, castExpression, cancellationToken).ConfigureAwait(false);
        }
    }
}
