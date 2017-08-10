// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.SyntaxInfo
{
    public struct SingleLocalDeclarationStatementInfo
    {
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

        public static SingleLocalDeclarationStatementInfo Create(LocalDeclarationStatementSyntax localDeclarationStatement)
        {
            if (localDeclarationStatement == null)
                throw new ArgumentNullException(nameof(localDeclarationStatement));

            VariableDeclarationSyntax variableDeclaration = localDeclarationStatement.Declaration;

            if (variableDeclaration == null)
                throw new ArgumentNullException(nameof(localDeclarationStatement));

            SeparatedSyntaxList<VariableDeclaratorSyntax> variables = variableDeclaration.Variables;

            if (variables.Count != 1)
                throw new ArgumentNullException(nameof(localDeclarationStatement));

            return new SingleLocalDeclarationStatementInfo(variableDeclaration, variables[0]);
        }

        public static bool TryCreate(
            SyntaxNode node,
            out SingleLocalDeclarationStatementInfo info)
        {
            return TryCreate(node as LocalDeclarationStatementSyntax, out info);
        }

        public static bool TryCreate(
            LocalDeclarationStatementSyntax localDeclarationStatement,
            out SingleLocalDeclarationStatementInfo info)
        {
            VariableDeclarationSyntax variableDeclaration = localDeclarationStatement.Declaration;

            if (variableDeclaration != null)
            {
                SeparatedSyntaxList<VariableDeclaratorSyntax> variables = variableDeclaration.Variables;

                if (variables.Count == 1)
                {
                    info = new SingleLocalDeclarationStatementInfo(variableDeclaration, variables[0]);
                    return true;
                }
            }

            info = default(SingleLocalDeclarationStatementInfo);
            return false;
        }

        internal static bool TryCreateFromValue(
            ExpressionSyntax expression,
            out SingleLocalDeclarationStatementInfo info)
        {
            SyntaxNode parent = expression?.WalkUpParentheses().Parent;

            if (parent?.Kind() == SyntaxKind.EqualsValueClause
                && (parent.Parent is VariableDeclaratorSyntax declarator))
            {
                var declaration = declarator.Parent as VariableDeclarationSyntax;

                if (declaration?.IsParentKind(SyntaxKind.LocalDeclarationStatement) == true
                    && declaration.Variables.Count == 1)
                {
                    info = new SingleLocalDeclarationStatementInfo(declaration, declarator);
                    return true;
                }
            }

            info = default(SingleLocalDeclarationStatementInfo);
            return false;
        }

        public override string ToString()
        {
            return Statement?.ToString() ?? base.ToString();
        }
    }
}
