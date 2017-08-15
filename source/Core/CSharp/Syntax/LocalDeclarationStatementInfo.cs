// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Syntax
{
    public struct LocalDeclarationStatementInfo
    {
        private static LocalDeclarationStatementInfo Default { get; } = new LocalDeclarationStatementInfo();

        private LocalDeclarationStatementInfo(
            LocalDeclarationStatementSyntax statement,
            VariableDeclarationSyntax declaration,
            TypeSyntax type)
        {
            Statement = statement;
            Declaration = declaration;
            Type = type;
        }

        public LocalDeclarationStatementSyntax Statement { get; }

        public VariableDeclarationSyntax Declaration { get; }

        public TypeSyntax Type { get; }

        public SyntaxTokenList Modifiers
        {
            get { return Statement?.Modifiers ?? default(SyntaxTokenList); }
        }

        public SyntaxToken SemicolonToken
        {
            get { return Statement?.SemicolonToken ?? default(SyntaxToken); }
        }

        public bool Success
        {
            get { return Statement != null; }
        }

        internal static LocalDeclarationStatementInfo Create(
            LocalDeclarationStatementSyntax localDeclarationStatement,
            SyntaxInfoOptions options = null)
        {
            options = options ?? SyntaxInfoOptions.Default;

            VariableDeclarationSyntax variableDeclaration = localDeclarationStatement?.Declaration;

            if (!options.Check(variableDeclaration))
                return Default;

            TypeSyntax type = variableDeclaration.Type;

            if (!options.Check(type))
                return Default;

            return new LocalDeclarationStatementInfo(localDeclarationStatement, variableDeclaration, type);
        }

        internal static LocalDeclarationStatementInfo Create(
            ExpressionSyntax value,
            SyntaxInfoOptions options = null)
        {
            options = options ?? SyntaxInfoOptions.Default;

            SyntaxNode node = value?.WalkUpParentheses().Parent;

            if (node?.Kind() != SyntaxKind.EqualsValueClause)
                return Default;

            node = node.Parent;

            if (node?.Kind() != SyntaxKind.VariableDeclarator)
                return Default;

            if (!(node?.Parent is VariableDeclarationSyntax declaration))
                return Default;

            TypeSyntax type = declaration.Type;

            if (!options.Check(type))
                return Default;

            if (!(declaration.Parent is LocalDeclarationStatementSyntax localDeclarationStatement))
                return Default;

            return new LocalDeclarationStatementInfo(localDeclarationStatement, declaration, type);
        }

        public override string ToString()
        {
            return Statement?.ToString() ?? base.ToString();
        }
    }
}
