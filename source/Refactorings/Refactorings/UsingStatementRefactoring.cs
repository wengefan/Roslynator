// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Refactorings
{
    internal static class UsingStatementRefactoring
    {
        public static async Task ComputeRefactoringsAsync(RefactoringContext context, UsingStatementSyntax usingStatement)
        {
            if (context.IsRefactoringEnabled(RefactoringIdentifiers.ExtractDeclarationFromUsingStatement))
            {
                VariableDeclarationSyntax declaration = usingStatement.Declaration;

                if (declaration != null
                    && context.Span.IsContainedInSpanOrBetweenSpans(declaration))
                {
                    SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                    if (StatementContainer.CanCreate(usingStatement)
                        && semanticModel.ContainsCompilerDiagnostic(
                            CompilerDiagnosticIdentifiers.TypeUsedInUsingStatementMustBeImplicitlyConvertibleToIDisposable,
                            declaration.Span,
                            context.CancellationToken))
                    {
                        context.RegisterRefactoring(
                            "Extract local declaration",
                            cancellationToken => ExtractDeclarationFromUsingStatementRefactoring.RefactorAsync(context.Document, usingStatement, cancellationToken));
                    }
                }
            }

            if (context.IsRefactoringEnabled(RefactoringIdentifiers.IntroduceLocalVariable))
                IntroduceLocalVariableRefactoring.ComputeRefactoring(context, usingStatement);
        }
    }
}