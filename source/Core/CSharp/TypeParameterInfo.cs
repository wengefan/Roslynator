// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp
{
    internal struct TypeParameterInfo
    {
        private TypeParameterInfo(TypeParameterSyntax typeParameter, SyntaxNode declaration, TypeParameterListSyntax typeParameterList, SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses) : this()
        {
            TypeParameter = typeParameter;
            Name = typeParameter.Identifier.ValueText;
            Declaration = declaration;
            TypeParameterList = typeParameterList;
            ConstraintClauses = constraintClauses;
        }

        public TypeParameterSyntax TypeParameter { get; }

        public string Name { get; }

        public SyntaxNode Declaration { get; }

        public TypeParameterListSyntax TypeParameterList { get; }

        public SyntaxList<TypeParameterConstraintClauseSyntax> ConstraintClauses { get; }

        public TypeParameterConstraintClauseSyntax ConstraintClause
        {
            get
            {
                foreach (TypeParameterConstraintClauseSyntax constraintClause in ConstraintClauses)
                {
                    if (string.Equals(Name, constraintClause.Name.Identifier.ValueText, StringComparison.Ordinal))
                        return constraintClause;
                }

                return null;
            }
        }

        public static TypeParameterInfo Create(TypeParameterSyntax typeParameter)
        {
            TypeParameterInfo info;
            if (TryCreate(typeParameter, out info))
                return info;

            throw new ArgumentException("", nameof(typeParameter));
        }

        public static bool TryCreate(TypeParameterSyntax typeParameter, out TypeParameterInfo info)
        {
            if (typeParameter.IsParentKind(SyntaxKind.TypeParameterList))
            {
                var typeParameterList = (TypeParameterListSyntax)typeParameter.Parent;

                SyntaxNode parent = typeParameterList.Parent;

                switch (parent?.Kind())
                {
                    case SyntaxKind.ClassDeclaration:
                        {
                            var classDeclaration = (ClassDeclarationSyntax)parent;
                            info = new TypeParameterInfo(typeParameter, classDeclaration, typeParameterList, classDeclaration.ConstraintClauses);
                            return true;
                        }
                    case SyntaxKind.DelegateDeclaration:
                        {
                            var delegateDeclaration = (DelegateDeclarationSyntax)parent;
                            info = new TypeParameterInfo(typeParameter, delegateDeclaration, typeParameterList, delegateDeclaration.ConstraintClauses);
                            return true;
                        }
                    case SyntaxKind.InterfaceDeclaration:
                        {
                            var interfaceDeclaration = (InterfaceDeclarationSyntax)parent;
                            info = new TypeParameterInfo(typeParameter, interfaceDeclaration, typeParameterList, interfaceDeclaration.ConstraintClauses);
                            return true;
                        }
                    case SyntaxKind.LocalFunctionStatement:
                        {
                            var localFunctionStatement = (LocalFunctionStatementSyntax)parent;
                            info = new TypeParameterInfo(typeParameter, localFunctionStatement, typeParameterList, localFunctionStatement.ConstraintClauses);
                            return true;
                        }
                    case SyntaxKind.MethodDeclaration:
                        {
                            var methodDeclaration = (MethodDeclarationSyntax)parent;
                            info = new TypeParameterInfo(typeParameter, methodDeclaration, typeParameterList, methodDeclaration.ConstraintClauses);
                            return true;
                        }
                    case SyntaxKind.StructDeclaration:
                        {
                            var structDeclaration = (StructDeclarationSyntax)parent;
                            info = new TypeParameterInfo(typeParameter, structDeclaration, typeParameterList, structDeclaration.ConstraintClauses);
                            return true;
                        }
                }
            }

            info = default(TypeParameterInfo);
            return false;
        }

        public static bool TryCreate(SyntaxNode node, string name, out TypeParameterInfo info)
        {
            switch (node?.Kind())
            {
                case SyntaxKind.ClassDeclaration:
                    {
                        var classDeclaration = (ClassDeclarationSyntax)node;

                        TypeParameterListSyntax typeParameterList = classDeclaration.TypeParameterList;

                        TypeParameterSyntax typeParameter = GetTypeParameterByName(typeParameterList, name);

                        if (typeParameter != null)
                        {
                            info = new TypeParameterInfo(typeParameter, classDeclaration, typeParameterList, classDeclaration.ConstraintClauses);
                            return true;
                        }

                        break;
                    }
                case SyntaxKind.DelegateDeclaration:
                    {
                        var delegateDeclaration = (DelegateDeclarationSyntax)node;

                        TypeParameterListSyntax typeParameterList = delegateDeclaration.TypeParameterList;

                        TypeParameterSyntax typeParameter = GetTypeParameterByName(typeParameterList, name);

                        if (typeParameter != null)
                        {
                            info = new TypeParameterInfo(typeParameter, delegateDeclaration, typeParameterList, delegateDeclaration.ConstraintClauses);
                            return true;
                        }

                        break;
                    }
                case SyntaxKind.InterfaceDeclaration:
                    {
                        var interfaceDeclaration = (InterfaceDeclarationSyntax)node;

                        TypeParameterListSyntax typeParameterList = interfaceDeclaration.TypeParameterList;

                        TypeParameterSyntax typeParameter = GetTypeParameterByName(typeParameterList, name);

                        if (typeParameter != null)
                        {
                            info = new TypeParameterInfo(typeParameter, interfaceDeclaration, typeParameterList, interfaceDeclaration.ConstraintClauses);
                            return true;
                        }

                        break;
                    }
                case SyntaxKind.LocalFunctionStatement:
                    {
                        var localFunctionStatement = (LocalFunctionStatementSyntax)node;

                        TypeParameterListSyntax typeParameterList = localFunctionStatement.TypeParameterList;

                        TypeParameterSyntax typeParameter = GetTypeParameterByName(typeParameterList, name);

                        if (typeParameter != null)
                        {
                            info = new TypeParameterInfo(typeParameter, localFunctionStatement, typeParameterList, localFunctionStatement.ConstraintClauses);
                            return true;
                        }

                        break;
                    }
                case SyntaxKind.MethodDeclaration:
                    {
                        var methodDeclaration = (MethodDeclarationSyntax)node;

                        TypeParameterListSyntax typeParameterList = methodDeclaration.TypeParameterList;

                        TypeParameterSyntax typeParameter = GetTypeParameterByName(typeParameterList, name);

                        if (typeParameter != null)
                        {
                            info = new TypeParameterInfo(typeParameter, methodDeclaration, typeParameterList, methodDeclaration.ConstraintClauses);
                            return true;
                        }

                        break;
                    }
                case SyntaxKind.StructDeclaration:
                    {
                        var structDeclaration = (StructDeclarationSyntax)node;

                        TypeParameterListSyntax typeParameterList = structDeclaration.TypeParameterList;

                        TypeParameterSyntax typeParameter = GetTypeParameterByName(typeParameterList, name);

                        if (typeParameter != null)
                        {
                            info = new TypeParameterInfo(typeParameter, structDeclaration, typeParameterList, structDeclaration.ConstraintClauses);
                            return true;
                        }

                        break;
                    }
            }

            info = default(TypeParameterInfo);
            return false;
        }

        private static TypeParameterSyntax GetTypeParameterByName(TypeParameterListSyntax typeParameterList, string name)
        {
            foreach (TypeParameterSyntax typeParameter in typeParameterList.Parameters)
            {
                if (string.Equals(typeParameter.Identifier.ValueText, name, StringComparison.Ordinal))
                    return typeParameter;
            }

            return null;
        }

        public static SyntaxNode WithConstraintClauses(SyntaxNode node, SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses)
        {
            switch (node.Kind())
            {
                case SyntaxKind.ClassDeclaration:
                    return ((ClassDeclarationSyntax)node).WithConstraintClauses(constraintClauses);
                case SyntaxKind.DelegateDeclaration:
                    return ((DelegateDeclarationSyntax)node).WithConstraintClauses(constraintClauses);
                case SyntaxKind.InterfaceDeclaration:
                    return ((InterfaceDeclarationSyntax)node).WithConstraintClauses(constraintClauses);
                case SyntaxKind.LocalFunctionStatement:
                    return ((LocalFunctionStatementSyntax)node).WithConstraintClauses(constraintClauses);
                case SyntaxKind.MethodDeclaration:
                    return ((MethodDeclarationSyntax)node).WithConstraintClauses(constraintClauses);
                case SyntaxKind.StructDeclaration:
                    return ((StructDeclarationSyntax)node).WithConstraintClauses(constraintClauses);
            }

            Debug.Fail(node.Kind().ToString());

            return node;
        }

        public static SyntaxNode WithTypeParameterList(SyntaxNode node, TypeParameterListSyntax typeParameterList)
        {
            switch (node.Kind())
            {
                case SyntaxKind.ClassDeclaration:
                    return ((ClassDeclarationSyntax)node).WithTypeParameterList(typeParameterList);
                case SyntaxKind.DelegateDeclaration:
                    return ((DelegateDeclarationSyntax)node).WithTypeParameterList(typeParameterList);
                case SyntaxKind.InterfaceDeclaration:
                    return ((InterfaceDeclarationSyntax)node).WithTypeParameterList(typeParameterList);
                case SyntaxKind.LocalFunctionStatement:
                    return ((LocalFunctionStatementSyntax)node).WithTypeParameterList(typeParameterList);
                case SyntaxKind.MethodDeclaration:
                    return ((MethodDeclarationSyntax)node).WithTypeParameterList(typeParameterList);
                case SyntaxKind.StructDeclaration:
                    return ((StructDeclarationSyntax)node).WithTypeParameterList(typeParameterList);
            }

            Debug.Fail(node.Kind().ToString());

            return node;
        }
    }
}