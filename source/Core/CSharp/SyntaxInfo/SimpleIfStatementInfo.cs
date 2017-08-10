// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Roslynator.CSharp.SyntaxInfo.SyntaxInfoHelper;

namespace Roslynator.CSharp.SyntaxInfo
{
    public struct SimpleIfStatementInfo
    {
        public SimpleIfStatementInfo(
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

        public static SimpleIfStatementInfo Create(
            IfStatementSyntax ifStatement,
            bool allowNullOrMissing = false,
            bool walkDownParentheses = true)
        {
            if (ifStatement == null)
                throw new ArgumentNullException(nameof(ifStatement));

            if (!ifStatement.IsSimpleIf())
                throw new ArgumentException("", nameof(ifStatement));

            ExpressionSyntax condition = ifStatement.Condition?.WalkDownParenthesesIf(walkDownParentheses);

            if (!CheckNode(condition, allowNullOrMissing))
                throw new ArgumentException("", nameof(ifStatement));

            StatementSyntax statement = ifStatement.Statement;

            if (CheckNode(statement, allowNullOrMissing))
                throw new ArgumentException("", nameof(ifStatement));

            return new SimpleIfStatementInfo(ifStatement, ifStatement.Condition, statement);
        }

        public static bool TryCreate(
            SyntaxNode node,
            out SimpleIfStatementInfo info,
            bool allowNullOrMissing = false,
            bool walkDownParentheses = true)
        {
            return TryCreate(
                node as IfStatementSyntax,
                out info,
                allowNullOrMissing: allowNullOrMissing,
                walkDownParentheses: walkDownParentheses);
        }

        public static bool TryCreate(
            IfStatementSyntax ifStatement,
            out SimpleIfStatementInfo info,
            bool allowNullOrMissing = false,
            bool walkDownParentheses = true)
        {
            if (ifStatement?.IsSimpleIf() == true)
            {
                ExpressionSyntax condition = ifStatement.Condition?.WalkDownParenthesesIf(walkDownParentheses);

                if (CheckNode(condition, allowNullOrMissing))
                {
                    StatementSyntax statement = ifStatement.Statement;

                    if (CheckNode(statement, allowNullOrMissing))
                    {
                        info = new SimpleIfStatementInfo(ifStatement, condition, statement);
                        return true;
                    }
                }
            }

            info = default(SimpleIfStatementInfo);
            return false;
        }

        public override string ToString()
        {
            return IfStatement?.ToString() ?? base.ToString();
        }
    }
}
