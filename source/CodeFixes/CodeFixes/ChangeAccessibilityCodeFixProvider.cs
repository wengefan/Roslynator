// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ChangeAccessibilityCodeFixProvider))]
    [Shared]
    public class ChangeAccessibilityCodeFixProvider : ModifiersCodeFixProvider
    {
        private static readonly Accessibility[] _publicOrInternalOrProtected = new Accessibility[]
        {
            Accessibility.Public,
            Accessibility.Internal,
            Accessibility.Protected
        };

        private static readonly Accessibility[] _publicOrInternalOrPrivate = new Accessibility[]
        {
            Accessibility.Public,
            Accessibility.Internal,
            Accessibility.Private
        };

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    CompilerDiagnosticIdentifiers.NewProtectedMemberDeclaredInSealedClass,
                    CompilerDiagnosticIdentifiers.StaticClassesCannotContainProtectedMembers,
                    CompilerDiagnosticIdentifiers.VirtualOrAbstractmembersCannotBePrivate,
                    CompilerDiagnosticIdentifiers.AbstractPropertiesCannotHavePrivateAccessors);
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
                            ChangeAccessibility(context, diagnostic, node, _publicOrInternalOrPrivate);
                            break;
                        }
                    case CompilerDiagnosticIdentifiers.VirtualOrAbstractmembersCannotBePrivate:
                        {
                            ChangeAccessibility(context, diagnostic, node, _publicOrInternalOrProtected);
                            break;
                        }
                    case CompilerDiagnosticIdentifiers.AbstractPropertiesCannotHavePrivateAccessors:
                        {
                            RemoveAccessModifiers(context, diagnostic, node);
                            ChangeAccessibility(context, diagnostic, node, _publicOrInternalOrProtected);
                            break;
                        }
                }
            }
        }
    }
}
