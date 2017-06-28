// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp.Refactorings;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ChangeAccessibilityCodeFixProvider))]
    [Shared]
    public class ChangeAccessibilityCodeFixProvider : BaseCodeFixProvider
    {
        private static readonly Accessibility[] _accessibilities = new Accessibility[]
        {
            Accessibility.Public,
            Accessibility.Internal,
            Accessibility.Protected
        };

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    CompilerDiagnosticIdentifiers.NewProtectedMemberDeclaredInSealedClass,
                    CompilerDiagnosticIdentifiers.StaticClassesCannotContainProtectedMembers,
                    CompilerDiagnosticIdentifiers.VirtualOrAbstractmembersCannotBePrivate);
            }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            if (!Settings.IsCodeFixEnabled(CodeFixIdentifiers.ChangeAccessibility))
                return;

            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            SyntaxNode node = root
                .FindNode(context.Span, getInnermostNodeForTie: true)?
                .FirstAncestorOrSelf(f => f is MemberDeclarationSyntax || f is AccessorDeclarationSyntax);

            Debug.Assert(node != null, $"{nameof(node)} is null");

            if (node == null)
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case CompilerDiagnosticIdentifiers.NewProtectedMemberDeclaredInSealedClass:
                    case CompilerDiagnosticIdentifiers.StaticClassesCannotContainProtectedMembers:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Change accessibility to 'private'",
                                cancellationToken =>
                                {
                                    SyntaxTokenList modifiers = node.GetModifiers();

                                    SyntaxToken protectedKeyword = modifiers[modifiers.IndexOf(SyntaxKind.ProtectedKeyword)];

                                    SyntaxTokenList newModifiers = modifiers.Replace(protectedKeyword, CSharpFactory.PrivateKeyword().WithTriviaFrom(protectedKeyword));

                                    SyntaxNode newNode = node.WithModifiers(newModifiers);

                                    return context.Document.ReplaceNodeAsync(node, newNode, context.CancellationToken);
                                },
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case CompilerDiagnosticIdentifiers.VirtualOrAbstractmembersCannotBePrivate:
                        {
                            if (!Settings.IsCodeFixEnabled(CodeFixIdentifiers.ChangeAccessibility))
                                break;

                            foreach (Accessibility accessibility in _accessibilities)
                            {
                                if (AccessibilityHelper.IsAllowedAccessibility(node, accessibility))
                                {
                                    CodeAction codeAction = CodeAction.Create(
                                        $"Change accessibility to '{AccessibilityHelper.GetAccessibilityName(accessibility)}'",
                                        cancellationToken => ChangeAccessibilityRefactoring.RefactorAsync(context.Document, node, accessibility, cancellationToken),
                                        GetEquivalenceKey(CompilerDiagnosticIdentifiers.VirtualOrAbstractmembersCannotBePrivate, accessibility.ToString()));

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
