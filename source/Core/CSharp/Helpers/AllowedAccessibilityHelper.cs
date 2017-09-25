// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Helpers
{
    internal static class AllowedAccessibilityHelper
    {
        public static bool IsAllowedAccessibility(SyntaxNode node, Accessibility accessibility)
        {
            switch (node.Kind())
            {
                case SyntaxKind.ClassDeclaration:
                case SyntaxKind.InterfaceDeclaration:
                case SyntaxKind.StructDeclaration:
                case SyntaxKind.EnumDeclaration:
                    {
                        if (node.IsParentKind(SyntaxKind.NamespaceDeclaration, SyntaxKind.CompilationUnit))
                        {
                            return accessibility == Accessibility.Public
                                || accessibility == Accessibility.Internal;
                        }

                        return true;
                    }
                case SyntaxKind.EventDeclaration:
                    {
                        var eventDeclaration = (EventDeclarationSyntax)node;

                        SyntaxTokenList modifiers = eventDeclaration.Modifiers;

                        return !modifiers.Contains(SyntaxKind.OverrideKeyword)
                            && CheckPrivateForAbstractOrVirtualMember(accessibility, modifiers)
                            && CheckProtectedOrProtectedInternalInStaticOrSealedClass(node, accessibility)
                            && CheckAccessorAccessibility(eventDeclaration.AccessorList, accessibility);
                    }
                case SyntaxKind.IndexerDeclaration:
                    {
                        var indexerDeclaration = (IndexerDeclarationSyntax)node;

                        SyntaxTokenList modifiers = indexerDeclaration.Modifiers;

                        return !modifiers.Contains(SyntaxKind.OverrideKeyword)
                            && CheckPrivateForAbstractOrVirtualMember(accessibility, modifiers)
                            && CheckProtectedOrProtectedInternalInStaticOrSealedClass(node, accessibility)
                            && CheckAccessorAccessibility(indexerDeclaration.AccessorList, accessibility);
                    }
                case SyntaxKind.PropertyDeclaration:
                    {
                        var propertyDeclaration = (PropertyDeclarationSyntax)node;

                        SyntaxTokenList modifiers = propertyDeclaration.Modifiers;

                        return !modifiers.Contains(SyntaxKind.OverrideKeyword)
                            && CheckPrivateForAbstractOrVirtualMember(accessibility, modifiers)
                            && CheckProtectedOrProtectedInternalInStaticOrSealedClass(node, accessibility)
                            && CheckAccessorAccessibility(propertyDeclaration.AccessorList, accessibility);
                    }
                case SyntaxKind.MethodDeclaration:
                    {
                        var methodDeclaration = (MethodDeclarationSyntax)node;

                        SyntaxTokenList modifiers = methodDeclaration.Modifiers;

                        return !modifiers.Contains(SyntaxKind.OverrideKeyword)
                            && CheckPrivateForAbstractOrVirtualMember(accessibility, modifiers)
                            && CheckProtectedOrProtectedInternalInStaticOrSealedClass(node, accessibility);
                    }
                case SyntaxKind.EventFieldDeclaration:
                    {
                        var eventFieldDeclaration = (EventFieldDeclarationSyntax)node;

                        SyntaxTokenList modifiers = eventFieldDeclaration.Modifiers;

                        return !modifiers.Contains(SyntaxKind.OverrideKeyword)
                            && CheckPrivateForAbstractOrVirtualMember(accessibility, modifiers)
                            && CheckProtectedOrProtectedInternalInStaticOrSealedClass(node, accessibility);
                    }
                case SyntaxKind.ConstructorDeclaration:
                case SyntaxKind.DelegateDeclaration:
                case SyntaxKind.FieldDeclaration:
                case SyntaxKind.IncompleteMember:
                    {
                        return CheckProtectedOrProtectedInternalInStaticOrSealedClass(node, accessibility);
                    }
                case SyntaxKind.OperatorDeclaration:
                case SyntaxKind.ConversionOperatorDeclaration:
                    {
                        return accessibility == Accessibility.Public;
                    }
                case SyntaxKind.GetAccessorDeclaration:
                case SyntaxKind.SetAccessorDeclaration:
                case SyntaxKind.AddAccessorDeclaration:
                case SyntaxKind.RemoveAccessorDeclaration:
                case SyntaxKind.UnknownAccessorDeclaration:
                    {
                        var memberDeclaration = node.Parent.Parent as MemberDeclarationSyntax;

                        Debug.Assert(memberDeclaration != null, node.ToString());

                        if (memberDeclaration != null)
                        {
                            if (accessibility == Accessibility.Protected
                                || accessibility == Accessibility.ProtectedOrInternal)
                            {
                                if ((memberDeclaration.Parent as ClassDeclarationSyntax)?.Modifiers.ContainsAny(SyntaxKind.StaticKeyword, SyntaxKind.SealedKeyword) == true)
                                    return false;
                            }

                            Accessibility declarationAccessibility = memberDeclaration.GetModifiers().GetAccessibility();

                            if (declarationAccessibility == Accessibility.NotApplicable)
                                declarationAccessibility = memberDeclaration.GetDefaultExplicitAccessibility();

                            return accessibility.IsMoreRestrictiveThan(declarationAccessibility);
                        }

                        return false;
                    }
                case SyntaxKind.LocalFunctionStatement:
                    {
                        return false;
                    }
                default:
                    {
                        Debug.Fail(node.Kind().ToString());
                        return false;
                    }
            }
        }

        private static bool CheckPrivateForAbstractOrVirtualMember(Accessibility accessibility, SyntaxTokenList modifiers)
        {
            return accessibility != Accessibility.Private
                || !modifiers.ContainsAny(SyntaxKind.AbstractKeyword, SyntaxKind.VirtualKeyword);
        }

        private static bool CheckProtectedOrProtectedInternalInStaticOrSealedClass(SyntaxNode node, Accessibility accessibility)
        {
            if (accessibility == Accessibility.Protected
                || accessibility == Accessibility.ProtectedOrInternal)
            {
                if ((node.Parent as ClassDeclarationSyntax)?.Modifiers.ContainsAny(SyntaxKind.StaticKeyword, SyntaxKind.SealedKeyword) == true)
                    return false;
            }

            return true;
        }

        private static bool CheckAccessorAccessibility(AccessorListSyntax accessorList, Accessibility accessibility)
        {
            if (accessorList != null)
            {
                foreach (AccessorDeclarationSyntax accessor in accessorList.Accessors)
                {
                    Accessibility accessorAccessibility = accessor.Modifiers.GetAccessibility();

                    if (accessorAccessibility != Accessibility.NotApplicable)
                        return accessorAccessibility.IsMoreRestrictiveThan(accessibility);
                }
            }

            return true;
        }
    }
}
