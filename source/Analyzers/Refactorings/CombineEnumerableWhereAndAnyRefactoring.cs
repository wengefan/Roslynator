// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Refactorings
{
    internal static class CombineEnumerableWhereAndAnyRefactoring
    {
        internal static void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext context)
        {
            var invocationExpression = (InvocationExpressionSyntax)context.Node;

            if (!invocationExpression.ContainsDiagnostics
                && !invocationExpression.SpanContainsDirectives())
            {
                MemberInvocationExpressionInfo invocation;
                if (MemberInvocationExpressionInfo.TryCreate(invocationExpression, out invocation)
                    && invocation.NameText == "Any")
                {
                    ArgumentSyntax argument1 = invocation.Arguments.SingleOrDefault(throwException: false);

                    if (argument1 != null)
                    {
                        MemberInvocationExpressionInfo invocation2;
                        if (MemberInvocationExpressionInfo.TryCreate(invocation.Expression, out invocation2)
                            && invocation2.NameText == "Where")
                        {
                            ArgumentSyntax argument2 = invocation2.Arguments.SingleOrDefault(throwException: false);

                            if (argument2 != null)
                            {
                                SemanticModel semanticModel = context.SemanticModel;
                                CancellationToken cancellationToken = context.CancellationToken;

                                MethodInfo methodInfo;
                                if (semanticModel.TryGetExtensionMethodInfo(invocationExpression, out methodInfo, ExtensionMethodKind.None, cancellationToken)
                                    && methodInfo.IsLinqExtensionOfIEnumerableOfTWithPredicate("Any"))
                                {
                                    MethodInfo methodInfo2;
                                    if (semanticModel.TryGetExtensionMethodInfo(invocation2.InvocationExpression, out methodInfo2, ExtensionMethodKind.None, cancellationToken)
                                        && methodInfo2.IsLinqWhere(allowImmutableArrayExtension: true))
                                    {
                                        SingleParameterLambdaExpressionInfo lambda = SyntaxInfo.SingleParameterLambdaExpressionInfo(argument1.Expression);
                                        if (lambda.Success
                                            && lambda.Body is ExpressionSyntax)
                                        {
                                            SingleParameterLambdaExpressionInfo lambda2 = SyntaxInfo.SingleParameterLambdaExpressionInfo(argument2.Expression);
                                            if (lambda2.Success
                                                && lambda2.Body is ExpressionSyntax
                                                && lambda.ParameterName.Equals(lambda2.ParameterName, StringComparison.Ordinal))
                                            {
                                                context.ReportDiagnostic(
                                                    DiagnosticDescriptors.SimplifyLinqMethodChain,
                                                    Location.Create(context.SyntaxTree(), TextSpan.FromBounds(invocation2.Name.SpanStart, invocationExpression.Span.End)));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static Task<Document> RefactorAsync(
            Document document,
            InvocationExpressionSyntax invocationExpression,
            CancellationToken cancellationToken)
        {
            MemberInvocationExpressionInfo invocation = MemberInvocationExpressionInfo.Create(invocationExpression);
            MemberInvocationExpressionInfo invocation2 = MemberInvocationExpressionInfo.Create((InvocationExpressionSyntax)invocation.Expression);

            SingleParameterLambdaExpressionInfo lambda = SingleParameterLambdaExpressionInfo.Create((LambdaExpressionSyntax)invocation.Arguments.First().Expression);
            SingleParameterLambdaExpressionInfo lambda2 = SingleParameterLambdaExpressionInfo.Create((LambdaExpressionSyntax)invocation2.Arguments.First().Expression);

            BinaryExpressionSyntax logicalAnd = CSharpFactory.LogicalAndExpression(
                ((ExpressionSyntax)lambda2.Body).Parenthesize(),
                ((ExpressionSyntax)lambda.Body).Parenthesize());

            InvocationExpressionSyntax newNode = invocation2.InvocationExpression
                .ReplaceNode(invocation2.Name, invocation.Name.WithTriviaFrom(invocation2.Name))
                .WithArgumentList(invocation2.ArgumentList.ReplaceNode((ExpressionSyntax)lambda2.Body, logicalAnd));

            return document.ReplaceNodeAsync(invocationExpression, newNode, cancellationToken);
        }
    }
}
