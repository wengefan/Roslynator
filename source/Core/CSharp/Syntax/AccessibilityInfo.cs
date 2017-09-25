// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Roslynator.CSharp.Helpers;

namespace Roslynator.CSharp.Syntax
{
    public struct AccessibilityInfo
    {
        private static AccessibilityInfo Default { get; } = new AccessibilityInfo();

        private AccessibilityInfo(SyntaxNode node, SyntaxTokenList modifiers, int index, int additionalIndex = -1)
        {
            Node = node;
            Modifiers = modifiers;
            Index = index;
            AdditionalIndex = additionalIndex;
        }

        public SyntaxNode Node { get; }

        public SyntaxToken Token
        {
            get { return GetTokenOrDefault(Index); }
        }

        public SyntaxToken AdditionalToken
        {
            get { return GetTokenOrDefault(AdditionalIndex); }
        }

        public SyntaxTokenList Modifiers { get; }

        public int Index { get; }

        public int AdditionalIndex { get; }

        public bool Success
        {
            get { return Node != null; }
        }

        public Accessibility Accessibility
        {
            get
            {
                if (Index == -1)
                    return Accessibility.NotApplicable;

                if (AdditionalIndex == -1)
                {
                    switch (Token.Kind())
                    {
                        case SyntaxKind.PublicKeyword:
                            return Accessibility.Public;
                        case SyntaxKind.PrivateKeyword:
                            return Accessibility.Private;
                        case SyntaxKind.InternalKeyword:
                            return Accessibility.Internal;
                        case SyntaxKind.ProtectedKeyword:
                            return Accessibility.Protected;
                    }
                }

                return Accessibility.ProtectedOrInternal;
            }
        }

        public static AccessibilityInfo Create(SyntaxNode node)
        {
            if (node == null)
                return Default;

            return Create(node, node.GetModifiers());
        }

        internal static AccessibilityInfo Create(SyntaxNode node, SyntaxTokenList modifiers)
        {
            if (node == null)
                return Default;

            int count = modifiers.Count;

            for (int i = 0; i < count; i++)
            {
                switch (modifiers[i].Kind())
                {
                    case SyntaxKind.PublicKeyword:
                    case SyntaxKind.PrivateKeyword:
                        return new AccessibilityInfo(node, modifiers, i);
                    case SyntaxKind.InternalKeyword:
                        return Create(node, modifiers, count, i, SyntaxKind.ProtectedKeyword);
                    case SyntaxKind.ProtectedKeyword:
                        return Create(node, modifiers, count, i, SyntaxKind.InternalKeyword);
                }
            }

            return new AccessibilityInfo(node, modifiers, -1);
        }

        private static AccessibilityInfo Create(SyntaxNode node, SyntaxTokenList modifiers, int count, int i, SyntaxKind kind)
        {
            for (int j = i + 1; j < count; j++)
            {
                if (modifiers[j].Kind() == kind)
                    return new AccessibilityInfo(node, modifiers, i, j);
            }

            return new AccessibilityInfo(node, modifiers, i);
        }

        public AccessibilityInfo WithAccessibility(Accessibility newAccessibility, IModifierComparer comparer = null)
        {
            return ChangeAccessibilityHelper.ChangeAccessibility(this, newAccessibility, comparer);
        }

        private SyntaxToken GetTokenOrDefault(int index)
        {
            return (index == -1) ? default(SyntaxToken) : Modifiers[index];
        }

        public override string ToString()
        {
            return Node?.ToString() ?? base.ToString();
        }
    }
}
