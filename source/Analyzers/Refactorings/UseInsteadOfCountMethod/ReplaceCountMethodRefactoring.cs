﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Refactorings.UseInsteadOfCountMethod
{
    internal static class UseInsteadOfCountMethodRefactoring
    {
        public static void Analyze(SyntaxNodeAnalysisContext context, MemberInvocationExpression invocation)
        {
            InvocationExpressionSyntax invocationExpression = invocation.InvocationExpression;

            SemanticModel semanticModel = context.SemanticModel;
            CancellationToken cancellationToken = context.CancellationToken;

            if (!semanticModel.TryGetExtensionMethodInfo(invocationExpression, out MethodInfo methodInfo, ExtensionMethodKind.Reduced, cancellationToken))
                return;

            if (!methodInfo.IsLinqExtensionOfIEnumerableOfTWithoutParameters("Count"))
                return;

            string propertyName = CSharpUtility.GetCountOrLengthPropertyName(invocation.Expression, semanticModel, cancellationToken);

            if (propertyName != null)
            {
                context.ReportDiagnostic(
                    DiagnosticDescriptors.UseCountOrLengthPropertyInsteadOfCountMethod,
                    Location.Create(context.Node.SyntaxTree, TextSpan.FromBounds(invocation.Name.Span.Start, invocationExpression.Span.End)),
                    ImmutableDictionary.CreateRange(new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("PropertyName", propertyName) }),
                    propertyName);
            }
            else
            {
                bool isFixable = false;

                SyntaxNode parent = invocationExpression.Parent;

                SyntaxKind kind = parent.Kind();

                if (kind.IsKind(
                    SyntaxKind.EqualsExpression,
                    SyntaxKind.NotEqualsExpression))
                {
                    var equalsExpression = (BinaryExpressionSyntax)parent;

                    if (equalsExpression.Left == invocationExpression)
                    {
                        if (equalsExpression.Right.IsNumericLiteralExpression("0"))
                            isFixable = true;
                    }
                    else if (equalsExpression.Left.IsNumericLiteralExpression("0"))
                    {
                        isFixable = true;
                    }
                }
                else if (kind.IsKind(
                    SyntaxKind.GreaterThanExpression,
                    SyntaxKind.GreaterThanOrEqualExpression,
                    SyntaxKind.LessThanExpression,
                    SyntaxKind.LessThanOrEqualExpression))
                {
                    isFixable = true;
                }

                if (isFixable)
                    context.ReportDiagnostic(DiagnosticDescriptors.UseAnyMethodInsteadOfCountMethod, parent);
            }
        }
    }
}
