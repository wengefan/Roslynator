// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Syntax
{
    public struct SimpleIfElseInfo
    {
        private static SimpleIfElseInfo Default { get; } = new SimpleIfElseInfo();

        public SimpleIfElseInfo(
            IfStatementSyntax ifStatement,
            ExpressionSyntax condition,
            StatementSyntax whenTrue,
            StatementSyntax whenFalse)
        {
            IfStatement = ifStatement;
            Condition = condition;
            WhenTrue = whenTrue;
            WhenFalse = whenFalse;
        }

        public IfStatementSyntax IfStatement { get; }

        public ExpressionSyntax Condition { get; }

        public StatementSyntax WhenTrue { get; }

        public StatementSyntax WhenFalse { get; }

        internal static SimpleIfElseInfo Create(
            IfStatementSyntax ifStatement,
            SyntaxInfoOptions options = null)
        {
            if (ifStatement?.IsParentKind(SyntaxKind.ElseClause) != false)
                return Default;

            StatementSyntax whenFalse = ifStatement.Else?.Statement;

            if (!options.CheckNode(whenFalse))
                return Default;

            if (whenFalse.IsKind(SyntaxKind.IfStatement))
                return Default;

            StatementSyntax whenTrue = ifStatement.Statement;

            if (!options.CheckNode(whenTrue))
                return Default;

            ExpressionSyntax condition = ifStatement.Condition?.WalkDownParenthesesIf(options.WalkDownParentheses);

            if (!options.CheckNode(condition))
                return Default;

            return new SimpleIfElseInfo(ifStatement, condition, whenTrue, whenFalse);
        }

        public override string ToString()
        {
            return IfStatement?.ToString() ?? base.ToString();
        }
    }
}
