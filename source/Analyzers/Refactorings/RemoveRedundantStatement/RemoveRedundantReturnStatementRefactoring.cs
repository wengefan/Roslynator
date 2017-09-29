﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Refactorings.RemoveRedundantStatement
{
    internal class RemoveRedundantReturnStatementRefactoring : RemoveRedundantStatementRefactoring<ReturnStatementSyntax>
    {
        protected override bool IsFixable(ReturnStatementSyntax statement)
        {
            return statement.Expression == null
                && base.IsFixable(statement);
        }

        protected override bool IsFixable(StatementSyntax statement, BlockSyntax block, SyntaxKind parentKind)
        {
            if (parentKind != SyntaxKind.ConstructorDeclaration
                && parentKind != SyntaxKind.DestructorDeclaration
                && parentKind != SyntaxKind.MethodDeclaration
                && parentKind != SyntaxKind.SetAccessorDeclaration
                && parentKind != SyntaxKind.LocalFunctionStatement)
            {
                return false;
            }

            if (!base.IsFixable(statement, block, parentKind))
                return false;

            if (parentKind == SyntaxKind.MethodDeclaration)
                return ((MethodDeclarationSyntax)block.Parent).ReturnType?.IsVoid() == true;

            if (parentKind == SyntaxKind.LocalFunctionStatement)
                return ((LocalFunctionStatementSyntax)block.Parent).ReturnType?.IsVoid() == true;

            return true;
        }
    }
}
