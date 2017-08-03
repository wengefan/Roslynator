// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Refactorings
{
    internal static class SimplifyConditionalExpression2Refactoring
    {
        public static void AnalyzeConditionalExpression(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.ContainsDiagnostics)
                return;

            if (context.Node.SpanContainsDirectives())
                return;

            var conditionalExpression = (ConditionalExpressionSyntax)context.Node;

            ConditionalExpressionInfo conditionalExpressionInfo;
            if (ConditionalExpressionInfo.TryCreate(conditionalExpression, out conditionalExpressionInfo))
            {
                ExpressionSyntax whenTrue = conditionalExpressionInfo.WhenTrue;
                ExpressionSyntax whenFalse = conditionalExpressionInfo.WhenFalse;

                SyntaxKind whenTrueKind = whenTrue.Kind();
                SyntaxKind whenFalseKind = whenFalse.Kind();

                if (whenTrueKind == whenFalseKind
                    && ArgumentListInfo.TryCreate(whenTrue, whenTrueKind, out ArgumentListInfo argumentListInfo)
                    && argumentListInfo.Arguments.Count == 1
                    && ArgumentListInfo.TryCreate(whenFalse, whenFalseKind, out ArgumentListInfo argumentListInfo2)
                    && argumentListInfo2.Arguments.Count == 1)
                {
                    ISymbol symbol = context.SemanticModel.GetSymbol(argumentListInfo.SymbolExpression, context.CancellationToken);

                    if (symbol?.IsErrorType() == false)
                    {
                        ISymbol symbol2 = context.SemanticModel.GetSymbol(argumentListInfo2.SymbolExpression, context.CancellationToken);

                        if (symbol.Equals(symbol2)
                            && SyntaxComparer.AreEquivalent(argumentListInfo.Expression, argumentListInfo2.Expression, requireNotNull: true))
                        {
                            context.ReportDiagnostic(DiagnosticDescriptors.SimplifyConditionalExpression2, conditionalExpression);
                        }
                    }
                }
            }
        }

        public static Task<Document> RefactorAsync(
            Document document,
            ConditionalExpressionSyntax conditionalExpression,
            CancellationToken cancellationToken)
        {
            ExpressionSyntax whenTrue = conditionalExpression.WhenTrue;
            ExpressionSyntax whenFalse = conditionalExpression.WhenFalse;

            ConditionalExpressionInfo conditionalExpressionInfo = ConditionalExpressionInfo.Create(conditionalExpression);

            ArgumentListInfo whenTrueInfo = ArgumentListInfo.Create(whenTrue);
            ArgumentListInfo whenFalseInfo = ArgumentListInfo.Create(whenFalse);

            ExpressionSyntax expression = whenTrueInfo.Arguments.First().Expression;

            ConditionalExpressionSyntax newExpression = conditionalExpression
                .WithCondition(conditionalExpression.Condition.TrimTrailingTrivia())
                .WithWhenTrue(expression)
                .WithWhenFalse(whenFalseInfo.Arguments.First().Expression);

            BaseArgumentListSyntax argumentList = whenTrueInfo.ArgumentList;

            ExpressionSyntax newNode = whenTrue
                .ReplaceNode(argumentList, argumentList.ReplaceNode(expression, newExpression))
                .WithTriviaFrom(conditionalExpression)
                .WithFormatterAnnotation();

            return document.ReplaceNodeAsync(conditionalExpression, newNode, cancellationToken);
        }

        private struct ArgumentListInfo
        {
            private ArgumentListInfo(ExpressionSyntax symbolExpression, ExpressionSyntax expression, BaseArgumentListSyntax argumentList)
            {
                SymbolExpression = symbolExpression;
                Expression = expression;
                ArgumentList = argumentList;
                Arguments = argumentList.Arguments;
            }

            public ExpressionSyntax SymbolExpression { get; }

            public ExpressionSyntax Expression { get; }

            public BaseArgumentListSyntax ArgumentList { get; }

            public SeparatedSyntaxList<ArgumentSyntax> Arguments { get; }

            public static ArgumentListInfo Create(ExpressionSyntax expression)
            {
                ArgumentListInfo info;
                if (TryCreate(expression, expression.Kind(), out info))
                    return info;

                return default(ArgumentListInfo);
            }

            public static bool TryCreate(ExpressionSyntax expression, SyntaxKind kind, out ArgumentListInfo info)
            {
                switch (kind)
                {
                    case SyntaxKind.InvocationExpression:
                        {
                            var invocationExpression = (InvocationExpressionSyntax)expression;
                            info = new ArgumentListInfo(invocationExpression, invocationExpression.Expression, invocationExpression.ArgumentList);
                            return true;
                        }
                    case SyntaxKind.ElementAccessExpression:
                        {
                            var elementAccessExpression = (ElementAccessExpressionSyntax)expression;
                            info = new ArgumentListInfo(elementAccessExpression, elementAccessExpression.Expression, elementAccessExpression.ArgumentList);
                            return true;
                        }
                    case SyntaxKind.ConditionalAccessExpression:
                        {
                            var conditionalAccessExpression = (ConditionalAccessExpressionSyntax)expression;

                            ExpressionSyntax whenNotNull = conditionalAccessExpression.WhenNotNull;

                            switch (whenNotNull.Kind())
                            {
                                case SyntaxKind.InvocationExpression:
                                    {
                                        var invocationExpression2 = (InvocationExpressionSyntax)whenNotNull;

                                        info = new ArgumentListInfo(invocationExpression2, invocationExpression2.Expression, invocationExpression2.ArgumentList);
                                        return true;
                                    }
                                case SyntaxKind.ElementBindingExpression:
                                    {
                                        var elementBindingExpression = (ElementBindingExpressionSyntax)whenNotNull;

                                        info = new ArgumentListInfo(elementBindingExpression, conditionalAccessExpression.Expression, elementBindingExpression.ArgumentList);
                                        return true;
                                    }
                            }

                            break;
                        }
                }

                info = default(ArgumentListInfo);
                return false;
            }
        }
    }
}
