// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Roslynator.CSharp.Comparers;
using Roslynator.CSharp.Helpers.ModifierHelpers;
using Roslynator.CSharp.Syntax;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.Helpers
{
    internal static class ChangeAccessibilityHelper
    {
        public static TNode ChangeAccessibility<TNode>(
            TNode node,
            Accessibility newAccessibility,
            IModifierComparer comparer = null) where TNode : SyntaxNode
        {
            AccessibilityInfo info = SyntaxInfo.AccessibilityInfo(node);

            AccessibilityInfo newInfo = ChangeAccessibility(info, newAccessibility, comparer);

            return (TNode)newInfo.Node;
        }

        public static AccessibilityInfo ChangeAccessibility(
            AccessibilityInfo info,
            Accessibility newAccessibility,
            IModifierComparer comparer = null)
        {
            if (!info.Success)
                return info;

            Accessibility accessibility = info.Accessibility;

            if (accessibility == newAccessibility)
                return info;

            comparer = comparer ?? ModifierComparer.Instance;

            SyntaxNode node = info.Node;

            if (IsSingleTokenAccessibility(accessibility)
                && IsSingleTokenAccessibility(newAccessibility))
            {
                int insertIndex = comparer.GetInsertIndex(info.Modifiers, GetKind(newAccessibility));

                if (info.Index == insertIndex
                    || info.Index == insertIndex - 1)
                {
                    SyntaxToken newToken = CreateToken(newAccessibility).WithTriviaFrom(info.Token);

                    SyntaxTokenList newModifiers = info.Modifiers.Replace(info.Token, newToken);

                    return AccessibilityInfo.Create(node.WithModifiers(newModifiers), newModifiers);
                }
            }

            if (accessibility != Accessibility.NotApplicable)
            {
                node = ModifierHelper.RemoveModifierAt(node, Math.Max(info.Index, info.AdditionalIndex));

                if (accessibility == Accessibility.ProtectedOrInternal)
                    node = ModifierHelper.RemoveModifierAt(node, Math.Min(info.Index, info.AdditionalIndex));
            }

            if (newAccessibility != Accessibility.NotApplicable)
            {
                node = InsertModifier(node, newAccessibility, comparer);
            }

            return AccessibilityInfo.Create(node);
        }

        private static SyntaxNode InsertModifier(SyntaxNode node, Accessibility accessibility, IModifierComparer comparer)
        {
            switch (accessibility)
            {
                case Accessibility.Private:
                    {
                        return node.InsertModifier(SyntaxKind.PrivateKeyword, comparer);
                    }
                case Accessibility.Protected:
                    {
                        return node.InsertModifier(SyntaxKind.ProtectedKeyword, comparer);
                    }
                case Accessibility.Internal:
                    {
                        return node.InsertModifier(SyntaxKind.InternalKeyword, comparer);
                    }
                case Accessibility.Public:
                    {
                        return node.InsertModifier(SyntaxKind.PublicKeyword, comparer);
                    }
                case Accessibility.ProtectedOrInternal:
                    {
                        node = node.InsertModifier(SyntaxKind.ProtectedKeyword, comparer);
                        node = node.InsertModifier(SyntaxKind.InternalKeyword, comparer);

                        return node;
                    }
            }

            return node;
        }

        private static SyntaxToken CreateToken(Accessibility accessibility)
        {
            switch (accessibility)
            {
                case Accessibility.Private:
                    return PrivateKeyword();
                case Accessibility.Protected:
                    return ProtectedKeyword();
                case Accessibility.Internal:
                    return InternalKeyword();
                case Accessibility.Public:
                    return PublicKeyword();
                case Accessibility.NotApplicable:
                    return default(SyntaxToken);
                default:
                    throw new ArgumentException("", nameof(accessibility));
            }
        }

        private static SyntaxKind GetKind(Accessibility accessibility)
        {
            switch (accessibility)
            {
                case Accessibility.Private:
                    return SyntaxKind.PrivateKeyword;
                case Accessibility.Protected:
                    return SyntaxKind.ProtectedKeyword;
                case Accessibility.Internal:
                    return SyntaxKind.InternalKeyword;
                case Accessibility.Public:
                    return SyntaxKind.PublicKeyword;
                case Accessibility.NotApplicable:
                    return SyntaxKind.None;
                default:
                    throw new ArgumentException("", nameof(accessibility));
            }
        }

        private static bool IsSingleTokenAccessibility(Accessibility accessibility)
        {
            switch (accessibility)
            {
                case Accessibility.Private:
                case Accessibility.Protected:
                case Accessibility.Internal:
                case Accessibility.Public:
                    return true;
                default:
                    return false;
            }
        }
    }
}
