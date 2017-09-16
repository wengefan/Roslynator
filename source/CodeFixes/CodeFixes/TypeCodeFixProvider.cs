﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp.Refactorings;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TypeCodeFixProvider))]
    [Shared]
    public class TypeCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(CompilerDiagnosticIdentifiers.StaticTypesCannotBeUsedAsTypeArguments); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            if (!Settings.IsCodeFixEnabled(CodeFixIdentifiers.MakeClassNonStatic))
                return;

            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out TypeSyntax type))
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case CompilerDiagnosticIdentifiers.StaticTypesCannotBeUsedAsTypeArguments:
                        {
                            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                            ISymbol symbol = semanticModel.GetSymbol(type, context.CancellationToken)?.OriginalDefinition;

                            if (symbol?.IsNamedType() == true
                                && ((INamedTypeSymbol)symbol).IsClass())
                            {
                                ImmutableArray<SyntaxReference> syntaxReferences = symbol.DeclaringSyntaxReferences;

                                if (syntaxReferences.Length == 1)
                                {
                                    ModifiersRefactoring.RemoveModifier(
                                        context,
                                        diagnostic,
                                        syntaxReferences[0].GetSyntax(context.CancellationToken),
                                        SyntaxKind.StaticKeyword,
                                        title: GetTitle(symbol, semanticModel, type.SpanStart));
                                }
                                else if (syntaxReferences.Length > 1)
                                {
                                    CodeAction codeAction = CodeAction.Create(
                                        GetTitle(symbol, semanticModel, type.SpanStart),
                                        cancellationToken =>
                                        {
                                            return context.Document.Solution().ReplaceNodesAsync(
                                                syntaxReferences.Select(f => (ClassDeclarationSyntax)f.GetSyntax(cancellationToken)),
                                                (f, g) => f.RemoveModifier(SyntaxKind.StaticKeyword),
                                                cancellationToken);
                                        },
                                        GetEquivalenceKey(diagnostic));

                                    context.RegisterCodeFix(codeAction, diagnostic);
                                }
                            }

                            break;
                        }
                }
            }
        }

        private static string GetTitle(ISymbol symbol, SemanticModel semanticModel, int position)
        {
            return $"Make '{SymbolDisplay.GetMinimalString(symbol, semanticModel, position)}' non-static";
        }
    }
}
