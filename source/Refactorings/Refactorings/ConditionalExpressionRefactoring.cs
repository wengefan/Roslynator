﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Refactorings
{
    internal static class ConditionalExpressionRefactoring
    {
        public static async Task ComputeRefactoringsAsync(RefactoringContext context, ConditionalExpressionSyntax conditionalExpression)
        {
            if (context.Span.IsEmptyAndContainedInSpanOrBetweenSpans(conditionalExpression))
            {
                if (context.IsRefactoringEnabled(RefactoringIdentifiers.FormatConditionalExpression))
                {
                    if (conditionalExpression.IsSingleLine())
                    {
                        context.RegisterRefactoring(
                            "Format ?: on separate lines",
                            cancellationToken =>
                            {
                                return CSharpFormatter.ToMultiLineAsync(
                                    context.Document,
                                    conditionalExpression,
                                    cancellationToken);
                            });
                    }
                    else
                    {
                        context.RegisterRefactoring(
                            "Format ?: on a single line",
                            cancellationToken =>
                            {
                                return CSharpFormatter.ToSingleLineAsync(
                                    context.Document,
                                    conditionalExpression,
                                    cancellationToken);
                            });
                    }
                }

                if (context.IsRefactoringEnabled(RefactoringIdentifiers.UseIfElseInsteadOfConditionalExpression))
                    await UseIfElseInsteadOfConditionalExpressionRefactoring.ComputeRefactoringAsync(context, conditionalExpression).ConfigureAwait(false);
            }

            if (context.IsRefactoringEnabled(RefactoringIdentifiers.SwapExpressionsInConditionalExpression)
                && (context.Span.IsBetweenSpans(conditionalExpression)
                    || context.Span.IsEmptyAndContainedInSpan(conditionalExpression.QuestionToken)
                    || context.Span.IsEmptyAndContainedInSpan(conditionalExpression.ColonToken))
                && SwapExpressionsInConditionalExpressionRefactoring.CanRefactor(conditionalExpression))
            {
                context.RegisterRefactoring(
                    "Swap expressions in ?:",
                    cancellationToken =>
                    {
                        return SwapExpressionsInConditionalExpressionRefactoring.RefactorAsync(
                            context.Document,
                            conditionalExpression,
                            cancellationToken);
                    });
            }
        }
    }
}