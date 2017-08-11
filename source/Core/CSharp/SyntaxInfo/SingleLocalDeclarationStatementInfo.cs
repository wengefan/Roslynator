// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Syntax
{
    public struct SingleLocalDeclarationStatementInfo
    {
        private static SingleLocalDeclarationStatementInfo Default { get; } = new SingleLocalDeclarationStatementInfo();

        public SingleLocalDeclarationStatementInfo(
            VariableDeclarationSyntax declaration,
            VariableDeclaratorSyntax declarator)
        {
            Declaration = declaration;
            Declarator = declarator;
        }

        public LocalDeclarationStatementSyntax Statement
        {
            get { return (LocalDeclarationStatementSyntax)Declaration?.Parent; }
        }

        public VariableDeclarationSyntax Declaration { get; }

        public VariableDeclaratorSyntax Declarator { get; }

        public EqualsValueClauseSyntax Initializer
        {
            get { return Declarator?.Initializer; }
        }

        public SyntaxTokenList Modifiers
        {
            get { return Statement?.Modifiers ?? default(SyntaxTokenList); }
        }

        public TypeSyntax Type
        {
            get { return Declaration?.Type; }
        }

        public SyntaxToken Identifier
        {
            get { return Declarator?.Identifier ?? default(SyntaxToken); }
        }

        public string IdentifierText
        {
            get { return Declarator?.Identifier.ValueText; }
        }

        public SyntaxToken EqualsToken
        {
            get { return Initializer?.EqualsToken ?? default(SyntaxToken); }
        }

        public SyntaxToken SemicolonToken
        {
            get { return Statement?.SemicolonToken ?? default(SyntaxToken); }
        }

        public bool Success
        {
            get { return Declaration != null; }
        }

        internal static SingleLocalDeclarationStatementInfo Create(
            LocalDeclarationStatementSyntax localDeclarationStatement,
            SyntaxInfoOptions options = null)
        {
            options = options ?? SyntaxInfoOptions.Default;

            VariableDeclarationSyntax variableDeclaration = localDeclarationStatement?.Declaration;

            if (variableDeclaration == null)
                return Default;

            VariableDeclaratorSyntax variable = variableDeclaration.Variables.SingleOrDefault(throwException: false);

            if (variable == null)
                return Default;

            return new SingleLocalDeclarationStatementInfo(variableDeclaration, variable);
        }

        internal static SingleLocalDeclarationStatementInfo Create(
            ExpressionSyntax expression,
            SyntaxInfoOptions options = null)
        {
            options = options ?? SyntaxInfoOptions.Default;

            SyntaxNode node = expression?.WalkUpParentheses().Parent;

            if (node?.Kind() != SyntaxKind.EqualsValueClause)
                return Default;

            if (!(node.Parent is VariableDeclaratorSyntax declarator))
                return Default;

            if (!(declarator.Parent is VariableDeclarationSyntax declaration))
                return Default;

            if (!declaration.IsParentKind(SyntaxKind.LocalDeclarationStatement))
                return Default;

            if (declaration.Variables.Count != 1)
                return Default;

            return new SingleLocalDeclarationStatementInfo(declaration, declarator);
        }

        public override string ToString()
        {
            return Statement?.ToString() ?? base.ToString();
        }
    }
}
