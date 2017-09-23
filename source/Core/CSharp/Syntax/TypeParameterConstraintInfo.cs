// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Syntax
{
    public struct TypeParameterConstraintInfo
    {
        private static TypeParameterConstraintInfo Default { get; } = new TypeParameterConstraintInfo();

        private TypeParameterConstraintInfo(
            TypeParameterConstraintSyntax constraint,
            TypeParameterConstraintClauseSyntax constraintClause,
            IdentifierNameSyntax name,
            SyntaxNode declaration,
            TypeParameterListSyntax typeParameterList,
            SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses)
        {
            Constraint = constraint;
            ConstraintClause = constraintClause;
            Name = name;
            Declaration = declaration;
            TypeParameterList = typeParameterList;
            ConstraintClauses = constraintClauses;
        }

        public TypeParameterConstraintSyntax Constraint { get; }

        public TypeParameterConstraintClauseSyntax ConstraintClause { get; }

        public SeparatedSyntaxList<TypeParameterConstraintSyntax> Constraints
        {
            get { return ConstraintClause?.Constraints ?? default(SeparatedSyntaxList<TypeParameterConstraintSyntax>); }
        }

        public IdentifierNameSyntax Name { get; }

        public string NameText
        {
            get { return Name?.Identifier.ValueText; }
        }

        public SyntaxNode Declaration { get; }

        public TypeParameterListSyntax TypeParameterList { get; }

        public SeparatedSyntaxList<TypeParameterSyntax> TypeParameters
        {
            get { return TypeParameterList?.Parameters ?? default(SeparatedSyntaxList<TypeParameterSyntax>); }
        }

        public SyntaxList<TypeParameterConstraintClauseSyntax> ConstraintClauses { get; }

        public TypeParameterSyntax TypeParameter
        {
            get
            {
                foreach (TypeParameterSyntax typeParameter in TypeParameters)
                {
                    if (string.Equals(NameText, typeParameter.Identifier.ValueText, StringComparison.Ordinal))
                        return typeParameter;
                }

                return null;
            }
        }

        public GenericInfo GenericInfo()
        {
            return SyntaxInfo.GenericInfo(Declaration);
        }

        public bool Success
        {
            get { return Constraint != null; }
        }

        internal bool IsDuplicateConstraint
        {
            get
            {
                return Constraint != null
                    && IsDuplicateConstraintHelper(Constraint, Constraints);
            }
        }

        public static TypeParameterConstraintInfo Create(
            TypeParameterConstraintSyntax constraint,
            SyntaxInfoOptions options = null)
        {
            if (!(constraint?.Parent is TypeParameterConstraintClauseSyntax constraintClause))
                return Default;

            IdentifierNameSyntax name = constraintClause.Name;

            if (!options.Check(name))
                return Default;

            SyntaxNode parent = constraintClause.Parent;

            switch (parent?.Kind())
            {
                case SyntaxKind.ClassDeclaration:
                    {
                        var classDeclaration = (ClassDeclarationSyntax)parent;

                        TypeParameterListSyntax typeParameterList = classDeclaration.TypeParameterList;

                        if (!options.Check(typeParameterList))
                            return Default;

                        return new TypeParameterConstraintInfo(constraint, constraintClause, name, classDeclaration, typeParameterList, classDeclaration.ConstraintClauses);
                    }
                case SyntaxKind.DelegateDeclaration:
                    {
                        var delegateDeclaration = (DelegateDeclarationSyntax)parent;

                        TypeParameterListSyntax typeParameterList = delegateDeclaration.TypeParameterList;

                        if (!options.Check(typeParameterList))
                            return Default;

                        return new TypeParameterConstraintInfo(constraint, constraintClause, name, delegateDeclaration, typeParameterList, delegateDeclaration.ConstraintClauses);
                    }
                case SyntaxKind.InterfaceDeclaration:
                    {
                        var interfaceDeclaration = (InterfaceDeclarationSyntax)parent;

                        TypeParameterListSyntax typeParameterList = interfaceDeclaration.TypeParameterList;

                        if (!options.Check(typeParameterList))
                            return Default;

                        return new TypeParameterConstraintInfo(constraint, constraintClause, name, interfaceDeclaration, interfaceDeclaration.TypeParameterList, interfaceDeclaration.ConstraintClauses);
                    }
                case SyntaxKind.LocalFunctionStatement:
                    {
                        var localFunctionStatement = (LocalFunctionStatementSyntax)parent;

                        TypeParameterListSyntax typeParameterList = localFunctionStatement.TypeParameterList;

                        if (!options.Check(typeParameterList))
                            return Default;

                        return new TypeParameterConstraintInfo(constraint, constraintClause, name, localFunctionStatement, typeParameterList, localFunctionStatement.ConstraintClauses);
                    }
                case SyntaxKind.MethodDeclaration:
                    {
                        var methodDeclaration = (MethodDeclarationSyntax)parent;

                        TypeParameterListSyntax typeParameterList = methodDeclaration.TypeParameterList;

                        if (!options.Check(typeParameterList))
                            return Default;

                        return new TypeParameterConstraintInfo(constraint, constraintClause, name, methodDeclaration, typeParameterList, methodDeclaration.ConstraintClauses);
                    }
                case SyntaxKind.StructDeclaration:
                    {
                        var structDeclaration = (StructDeclarationSyntax)parent;

                        TypeParameterListSyntax typeParameterList = structDeclaration.TypeParameterList;

                        if (!options.Check(typeParameterList))
                            return Default;

                        return new TypeParameterConstraintInfo(constraint, constraintClause, name, structDeclaration, typeParameterList, structDeclaration.ConstraintClauses);
                    }
            }

            return Default;
        }

        private static bool IsDuplicateConstraintHelper(
            TypeParameterConstraintSyntax constraint,
            SeparatedSyntaxList<TypeParameterConstraintSyntax> constraints)
        {
            int index = constraints.IndexOf(constraint);

            SyntaxKind kind = constraint.Kind();

            switch (kind)
            {
                case SyntaxKind.ClassConstraint:
                case SyntaxKind.StructConstraint:
                    {
                        for (int i = 0; i < index; i++)
                        {
                            if (constraints[i].Kind() == kind)
                                return true;
                        }

                        break;
                    }
            }

            return false;
        }

        public override string ToString()
        {
            return Constraint?.ToString() ?? base.ToString();
        }
    }
}