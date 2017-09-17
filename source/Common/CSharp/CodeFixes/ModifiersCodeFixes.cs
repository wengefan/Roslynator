// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Roslynator.CSharp.Helpers.ModifierHelpers;

namespace Roslynator.CSharp.CodeFixes
{
    internal static class ModifiersCodeFixes
    {
        public static void AddModifier(
            CodeFixContext context,
            Diagnostic diagnostic,
            SyntaxNode node,
            SyntaxKind kind,
            string title = null,
            string additionalKey = null,
            IModifierComparer comparer = null)
        {
            Document document = context.Document;

            CodeAction codeAction = CodeAction.Create(
                title ?? GetAddModifierTitle(kind),
                cancellationToken => AddModifierAsync(document, node, kind, comparer, cancellationToken),
                GetEquivalenceKey(diagnostic, kind, additionalKey));

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        private static Task<Document> AddModifierAsync<TNode>(
            Document document,
            TNode node,
            SyntaxKind kind,
            IModifierComparer comparer = null,
            CancellationToken cancellationToken = default(CancellationToken)) where TNode : SyntaxNode
        {
            TNode newNode = AddModifier(node, kind, comparer);

            return document.ReplaceNodeAsync(node, newNode, cancellationToken);
        }

        private static TNode AddModifier<TNode>(
            TNode node,
            SyntaxKind kind,
            IModifierComparer comparer = null) where TNode : SyntaxNode
        {
            switch (kind)
            {
                case SyntaxKind.AbstractKeyword:
                    {
                        node = node
                            .RemoveModifier(SyntaxKind.VirtualKeyword)
                            .RemoveModifier(SyntaxKind.OverrideKeyword);

                        break;
                    }
                case SyntaxKind.VirtualKeyword:
                    {
                        node = node
                            .RemoveModifier(SyntaxKind.AbstractKeyword)
                            .RemoveModifier(SyntaxKind.OverrideKeyword);

                        break;
                    }
                case SyntaxKind.OverrideKeyword:
                    {
                        node = node
                            .RemoveModifier(SyntaxKind.AbstractKeyword)
                            .RemoveModifier(SyntaxKind.VirtualKeyword);

                        break;
                    }
                case SyntaxKind.StaticKeyword:
                    {
                        if (node.Kind() == SyntaxKind.ConstructorDeclaration)
                            node = ModifierHelper.RemoveAccessModifiers(node);

                        node = node.RemoveModifier(SyntaxKind.SealedKeyword);

                        break;
                    }
            }

            return node.InsertModifier(kind, comparer);
        }

        public static void AddModifier<TNode>(
            CodeFixContext context,
            Diagnostic diagnostic,
            IEnumerable<TNode> nodes,
            SyntaxKind kind,
            string title = null,
            string additionalKey = null,
            IModifierComparer comparer = null) where TNode : SyntaxNode
        {
            if (nodes is IList<TNode> list
                && list.Count == 1)
            {
                AddModifier(context, diagnostic, list[0], kind, title, additionalKey, comparer);
                return;
            }

            CodeAction codeAction = CodeAction.Create(
                title ?? GetAddModifierTitle(kind),
                cancellationToken =>
                {
                    return context.Solution().ReplaceNodesAsync(
                        nodes,
                        (f, g) => AddModifier(f, kind, comparer),
                        cancellationToken);
                },
                GetEquivalenceKey(diagnostic, kind, additionalKey));

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        public static void RemoveModifier(
            CodeFixContext context,
            Diagnostic diagnostic,
            SyntaxNode node,
            SyntaxKind kind,
            string title = null,
            string additionalKey = null)
        {
            Document document = context.Document;

            CodeAction codeAction = CodeAction.Create(
                title ?? GetRemoveModifierTitle(kind),
                cancellationToken => RemoveModifierAsync(document, node, kind, cancellationToken),
                GetEquivalenceKey(diagnostic, kind, additionalKey));

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        public static void RemoveModifier(
            CodeFixContext context,
            Diagnostic diagnostic,
            SyntaxNode node,
            SyntaxToken modifier,
            string title = null,
            string additionalKey = null)
        {
            SyntaxKind kind = modifier.Kind();

            Document document = context.Document;

            CodeAction codeAction = CodeAction.Create(
                title ?? GetRemoveModifierTitle(kind),
                cancellationToken => RemoveModifierAsync(document, node, modifier, cancellationToken),
                GetEquivalenceKey(diagnostic, kind, additionalKey));

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        private static Task<Document> RemoveModifierAsync<TNode>(
            Document document,
            TNode node,
            SyntaxKind kind,
            CancellationToken cancellationToken = default(CancellationToken)) where TNode : SyntaxNode
        {
            SyntaxNode newNode = ModifierHelper.RemoveModifier(node, kind);

            return document.ReplaceNodeAsync(node, newNode, cancellationToken);
        }

        private static Task<Document> RemoveModifierAsync<TNode>(
            Document document,
            TNode node,
            SyntaxToken modifier,
            CancellationToken cancellationToken = default(CancellationToken)) where TNode : SyntaxNode
        {
            SyntaxNode newNode = ModifierHelper.RemoveModifier(node, modifier);

            return document.ReplaceNodeAsync(node, newNode, cancellationToken);
        }

        public static void RemoveModifier<TNode>(
            CodeFixContext context,
            Diagnostic diagnostic,
            IEnumerable<TNode> nodes,
            SyntaxKind kind,
            string title = null,
            string additionalKey = null) where TNode : SyntaxNode
        {
            if (nodes is IList<TNode> list
                && list.Count == 1)
            {
                RemoveModifier(context, diagnostic, list[0], kind, title, additionalKey);
                return;
            }

            CodeAction codeAction = CodeAction.Create(
                title ?? GetRemoveModifierTitle(kind),
                cancellationToken =>
                {
                    return context.Solution().ReplaceNodesAsync(
                        nodes,
                        (f, g) => ModifierHelper.RemoveModifier(f, kind),
                        cancellationToken);
                },
                GetEquivalenceKey(diagnostic, kind, additionalKey));

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        public static void RemoveModifiers(
            CodeFixContext context,
            Diagnostic diagnostic,
            SyntaxNode node,
            Func<SyntaxToken, bool> predicate,
            string additionalKey = null)
        {
            RemoveModifiers(context, diagnostic, node, node.GetModifiers(), predicate, additionalKey);
        }

        public static void RemoveModifiers(
            CodeFixContext context,
            Diagnostic diagnostic,
            SyntaxNode node,
            SyntaxTokenList modifiers,
            Func<SyntaxToken, bool> predicate,
            string additionalKey = null)
        {
            List<int> indexes = null;

            for (int i = 0; i < modifiers.Count; i++)
            {
                if (predicate(modifiers[i]))
                    (indexes ?? (indexes = new List<int>())).Add(i);
            }

            if (indexes != null)
            {
                if (indexes.Count == 1)
                {
                    RemoveModifier(context, diagnostic, node, modifiers[indexes[0]], additionalKey: additionalKey);
                }
                else
                {
                    CodeAction codeAction = CodeAction.Create(
                        "Remove modifiers",
                        cancellationToken =>
                        {
                            SyntaxNode newNode = node;

                            for (int i = indexes.Count - 1; i >= 0; i--)
                                newNode = ModifierHelper.RemoveModifierAt(newNode, indexes[i]);

                            return context.Document.ReplaceNodeAsync(node, newNode, cancellationToken);
                        },
                        GetEquivalenceKey(diagnostic, additionalKey));

                    context.RegisterCodeFix(codeAction, diagnostic);
                }
            }
        }

        public static void RemoveModifiers(
            CodeFixContext context,
            Diagnostic diagnostic,
            SyntaxNode node,
            string additionalKey = null)
        {
            SyntaxTokenList modifiers = node.GetModifiers();

            if (modifiers.Count == 1)
            {
                RemoveModifier(context, diagnostic, node, modifiers[0], additionalKey);
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
                    GetEquivalenceKey(diagnostic, additionalKey));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
        }

        public static void RemoveAccessModifiers(
            CodeFixContext context,
            Diagnostic diagnostic,
            SyntaxNode node,
            string additionalKey = null)
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
                RemoveModifier(context, diagnostic, node, accessModifier, additionalKey: additionalKey);
            }
            else
            {
                CodeAction codeAction = CodeAction.Create(
                    "Remove access modifiers",
                    cancellationToken =>
                    {
                        SyntaxNode newNode = ModifierHelper.RemoveAccessModifiers(node);

                        return context.Document.ReplaceNodeAsync(node, newNode, cancellationToken);
                    },
                    GetEquivalenceKey(diagnostic, additionalKey));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
        }

        public static void MoveModifier(
            CodeFixContext context,
            Diagnostic diagnostic,
            SyntaxNode node,
            SyntaxToken modifier,
            string title = null,
            string additionalKey = null,
            IModifierComparer comparer = null)
        {
            Document document = context.Document;

            SyntaxKind kind = modifier.Kind();

            CodeAction codeAction = CodeAction.Create(
                title ?? GetRemoveModifierTitle(kind),
                cancellationToken =>
                {
                    SyntaxNode newNode = node
                        .RemoveModifier(modifier)
                        .InsertModifier(kind, comparer);

                    return document.ReplaceNodeAsync(node, newNode, cancellationToken);
                },
                GetEquivalenceKey(diagnostic, kind, additionalKey));

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        private static string GetEquivalenceKey(Diagnostic diagnostic, string additionalKey)
        {
            return AbstractCodeFixProvider.GetEquivalenceKey(diagnostic, additionalKey);
        }

        private static string GetEquivalenceKey(Diagnostic diagnostic, SyntaxKind kind, string additionalKey)
        {
            return AbstractCodeFixProvider.GetEquivalenceKey(diagnostic, additionalKey ?? kind.ToString());
        }

        private static string GetAddModifierTitle(SyntaxKind kind)
        {
            return $"Add '{GetModifierName(kind)}' modifier";
        }

        private static string GetRemoveModifierTitle(SyntaxKind kind)
        {
            return $"Remove '{GetModifierName(kind)}' modifier";
        }

        private static string MoveModifierTitle(SyntaxKind kind)
        {
            return $"Move '{GetModifierName(kind)}' modifier";
        }

        private static string GetModifierName(SyntaxKind modifierKind)
        {
            switch (modifierKind)
            {
                case SyntaxKind.NewKeyword:
                    return "new";
                case SyntaxKind.PublicKeyword:
                    return "public";
                case SyntaxKind.ProtectedKeyword:
                    return "protected";
                case SyntaxKind.InternalKeyword:
                    return "internal";
                case SyntaxKind.PrivateKeyword:
                    return "private";
                case SyntaxKind.ConstKeyword:
                    return "const";
                case SyntaxKind.StaticKeyword:
                    return "static";
                case SyntaxKind.VirtualKeyword:
                    return "virtual";
                case SyntaxKind.SealedKeyword:
                    return "sealed";
                case SyntaxKind.OverrideKeyword:
                    return "override";
                case SyntaxKind.AbstractKeyword:
                    return "abstract";
                case SyntaxKind.ReadOnlyKeyword:
                    return "readonly";
                case SyntaxKind.ExternKeyword:
                    return "extern";
                case SyntaxKind.UnsafeKeyword:
                    return "unsafe";
                case SyntaxKind.VolatileKeyword:
                    return "volatile";
                case SyntaxKind.AsyncKeyword:
                    return "async";
                case SyntaxKind.PartialKeyword:
                    return "partial";
                case SyntaxKind.ThisKeyword:
                    return "this";
                case SyntaxKind.ParamsKeyword:
                    return "params";
                case SyntaxKind.InKeyword:
                    return "in";
                case SyntaxKind.OutKeyword:
                    return "out";
                case SyntaxKind.RefKeyword:
                    return "ref";
                default:
                    {
                        Debug.Fail(modifierKind.ToString());
                        return null;
                    }
            }
        }
    }
}