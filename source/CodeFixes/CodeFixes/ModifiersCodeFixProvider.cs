// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Roslynator.CSharp.Helpers.ModifierHelpers;
using Roslynator.CSharp.Refactorings;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ChangeAccessibilityCodeFixProvider))]
    [Shared]
    public abstract class ModifiersCodeFixProvider : BaseCodeFixProvider
    {
        protected void ChangeAccessibility(CodeFixContext context, Diagnostic diagnostic, SyntaxNode node, Accessibility[] accessibilities)
        {
            foreach (Accessibility accessibility in accessibilities)
            {
                if (AccessibilityHelper.IsAllowedAccessibility(node, accessibility))
                {
                    CodeAction codeAction = CodeAction.Create(
                        $"Change accessibility to '{AccessibilityHelper.GetAccessibilityName(accessibility)}'",
                        cancellationToken => ChangeAccessibilityRefactoring.RefactorAsync(context.Document, node, accessibility, cancellationToken),
                        GetEquivalenceKey(diagnostic.Id, accessibility.ToString()));

                    context.RegisterCodeFix(codeAction, diagnostic);
                }
            }
        }

        protected void RemoveModifier(CodeFixContext context, Diagnostic diagnostic, SyntaxNode node, SyntaxTokenList modifiers, SyntaxKind modifierKind)
        {
            int index = modifiers.IndexOf(modifierKind);

            if (index == -1)
                return;

            SyntaxToken modifier = modifiers[index];

            RemoveModifier(context, diagnostic, node, modifier, modifierKind.ToString());
        }

        protected void RemoveModifier(CodeFixContext context, Diagnostic diagnostic, SyntaxNode node, SyntaxToken token, string additionalKey = null)
        {
            CodeAction codeAction = CodeAction.Create(
                $"Remove '{token.ToString()}' modifier",
                cancellationToken =>
                {
                    SyntaxNode newNode = node.RemoveModifier(token);

                    return context.Document.ReplaceNodeAsync(node, newNode, cancellationToken);
                },
                GetEquivalenceKey(diagnostic, additionalKey));

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        protected void RemoveAccessModifiers(CodeFixContext context, Diagnostic diagnostic, SyntaxNode node)
        {
            SyntaxTokenList modifiers = node.GetModifiers();

            var accessModifier = default(SyntaxToken);

            foreach (SyntaxToken modifier in modifiers)
            {
                if (modifier.IsAccessModifier())
                {
                    if (accessModifier.IsAccessModifier())
                    {
                        accessModifier = default(SyntaxToken);
                        break;
                    }
                    else
                    {
                        accessModifier = modifier;
                    }
                }
            }

            if (accessModifier.IsAccessModifier())
            {
                RemoveModifier(context, diagnostic, node, accessModifier);
            }
            else
            {
                CodeAction codeAction = CodeAction.Create(
                    "Remove accessibility modifiers",
                    cancellationToken =>
                    {
                        SyntaxNode newNode = ModifierHelper.RemoveAccessModifiers(node);

                        return context.Document.ReplaceNodeAsync(node, newNode, cancellationToken);
                    },
                    GetEquivalenceKey(diagnostic));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
        }

        protected void RemoveModifiers(CodeFixContext context, Diagnostic diagnostic, SyntaxNode node)
        {
            SyntaxTokenList modifiers = node.GetModifiers();

            if (modifiers.Count == 1)
            {
                RemoveModifier(context, diagnostic, node, modifiers[0]);
            }
            else
            {
                CodeAction codeAction = CodeAction.Create(
                    "Remove modifiers",
                    cancellationToken =>
                    {
                        SyntaxNode newNode = ModifierHelper.RemoveModifiers(node);

                        return context.Document.ReplaceNodeAsync(node, newNode, cancellationToken);
                    },
                    GetEquivalenceKey(diagnostic));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
        }
    }
}
