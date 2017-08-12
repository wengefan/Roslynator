// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Syntax
{
    public struct SimpleIfStatementInfo
    {
        private static SimpleIfStatementInfo Default { get; } = new SimpleIfStatementInfo();

        private SimpleIfStatementInfo(
            IfStatementSyntax ifStatement,
            ExpressionSyntax condition,
            StatementSyntax statement)
        {
            IfStatement = ifStatement;
            Condition = condition;
            Statement = statement;
        }

        public IfStatementSyntax IfStatement { get; }

        public ExpressionSyntax Condition { get; }

        public StatementSyntax Statement { get; }

        public bool Success
        {
            get { return IfStatement != null; }
        }

        internal static SimpleIfStatementInfo Create(
            SyntaxNode node,
            SyntaxInfoOptions options = null)
        {
            return Create(node as IfStatementSyntax, options);
        }

        internal static SimpleIfStatementInfo Create(
            IfStatementSyntax ifStatement,
            SyntaxInfoOptions options = null)
        {
            options = options ?? SyntaxInfoOptions.Default;

            if (ifStatement?.IsSimpleIf() != true)
                return Default;

            ExpressionSyntax condition = options.WalkAndCheck(ifStatement.Condition);

            if (condition == null)
                return Default;

            StatementSyntax statement = ifStatement.Statement;

            if (!options.Check(statement))
                return Default;

            return new SimpleIfStatementInfo(ifStatement, condition, statement);
        }

        public override string ToString()
        {
            return IfStatement?.ToString() ?? base.ToString();
        }
    }
}
