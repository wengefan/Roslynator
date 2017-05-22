// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(VariableDeclaratorCodeFixProvider))]
    [Shared]
    public class VariableDeclaratorCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(CSharpErrorCodes.VariableIsAssignedButItsValueIsNeverUsed); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            VariableDeclaratorSyntax variableDeclarator = root
                .FindNode(context.Span, getInnermostNodeForTie: true)?
                .FirstAncestorOrSelf<VariableDeclaratorSyntax>();

            Debug.Assert(variableDeclarator != null, $"{nameof(variableDeclarator)} is null");

            if (variableDeclarator == null)
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                if (CodeFixSettings.Current.IsCodeFixEnabled(diagnostic.Id))
                {
                    switch (diagnostic.Id)
                    {
                        case CSharpErrorCodes.VariableIsAssignedButItsValueIsNeverUsed:
                            {
                                Debug.Assert(variableDeclarator.IsParentKind(SyntaxKind.VariableDeclaration), "");

                                if (variableDeclarator.IsParentKind(SyntaxKind.VariableDeclaration))
                                {
                                    var variableDeclaration = (VariableDeclarationSyntax)variableDeclarator.Parent;

                                    if (variableDeclaration.Variables.Count == 1)
                                    {
                                        var localDeclarationStatement = (LocalDeclarationStatementSyntax)variableDeclaration.Parent;

                                        if (!localDeclarationStatement.SpanContainsDirectives())
                                        {
                                            CodeAction codeAction = CodeAction.Create(
                                                "Remove unused variable",
                                                cancellationToken => context.Document.RemoveNodeAsync(localDeclarationStatement, RemoveHelper.GetRemoveOptions(localDeclarationStatement)),
                                                diagnostic.Id + EquivalenceKeySuffix);

                                            context.RegisterCodeFix(codeAction, diagnostic);
                                        }
                                    }
                                    else if (!variableDeclarator.SpanContainsDirectives())
                                    {
                                        CodeAction codeAction = CodeAction.Create(
                                            "Remove unused variable",
                                            cancellationToken => context.Document.RemoveNodeAsync(variableDeclarator, RemoveHelper.GetRemoveOptions(variableDeclarator)),
                                            diagnostic.Id + EquivalenceKeySuffix);

                                        context.RegisterCodeFix(codeAction, diagnostic);
                                    }
                                }

                                break;
                            }
                    }
                }
            }
        }
    }
}
