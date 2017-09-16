﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.Refactorings
{
    internal static class ReplaceIfWithSwitchRefactoring
    {
        public static async Task ComputeRefactoringAsync(RefactoringContext context, IfStatementSyntax ifStatement)
        {
            if (!ifStatement.IsTopmostIf())
                return;

            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

            foreach (IfStatementOrElseClause ifOrElse in ifStatement.GetChain())
            {
                if (ifOrElse.IsIf)
                {
                    if (!IsFixable(ifOrElse.AsIf(), semanticModel, context.CancellationToken))
                        return;
                }
                else if (ContainsBreakStatementThatBelongsToParentLoop(ifOrElse.AsElse().Statement))
                {
                    return;
                }
            }

            string title = (ifStatement.IsSimpleIf())
                ? "Replace if with switch"
                : "Replace if-else with switch";

            context.RegisterRefactoring(
                title,
                cancellationToken => RefactorAsync(context.Document, ifStatement, cancellationToken));
        }

        private static bool IsFixable(
            IfStatementSyntax ifStatement,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            ExpressionSyntax condition = ifStatement.Condition?.WalkDownParentheses();

            return condition?.IsKind(SyntaxKind.EqualsExpression, SyntaxKind.LogicalOrExpression) == true
                && IsFixableCondition((BinaryExpressionSyntax)condition, null, semanticModel, cancellationToken)
                && !ContainsBreakStatementThatBelongsToParentLoop(ifStatement.Statement);
        }

        private static bool IsFixableCondition(
            BinaryExpressionSyntax binaryExpression,
            ExpressionSyntax switchExpression,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            bool success = true;

            while (success)
            {
                success = false;

                SyntaxKind kind = binaryExpression.Kind();

                if (kind == SyntaxKind.LogicalOrExpression)
                {
                    ExpressionSyntax right = binaryExpression.Right.WalkDownParentheses();

                    if (right?.IsKind(SyntaxKind.EqualsExpression) == true)
                    {
                        var equalsExpression = (BinaryExpressionSyntax)right;

                        if (IsFixableEqualsExpression(equalsExpression, switchExpression, semanticModel, cancellationToken))
                        {
                            if (switchExpression == null)
                            {
                                switchExpression = equalsExpression.Left?.WalkDownParentheses();

                                if (!IsFixableSwitchExpression(switchExpression, semanticModel, cancellationToken))
                                    return false;
                            }

                            ExpressionSyntax left = binaryExpression.Left?.WalkDownParentheses();

                            if (left?.IsKind(SyntaxKind.LogicalOrExpression, SyntaxKind.EqualsExpression) == true)
                            {
                                binaryExpression = (BinaryExpressionSyntax)left;
                                success = true;
                            }
                        }
                    }
                }
                else if (kind == SyntaxKind.EqualsExpression)
                {
                    return IsFixableEqualsExpression(binaryExpression, switchExpression, semanticModel, cancellationToken);
                }
            }

            return false;
        }

        private static bool IsFixableEqualsExpression(
            BinaryExpressionSyntax equalsExpression,
            ExpressionSyntax switchExpression,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            ExpressionSyntax left = equalsExpression.Left?.WalkDownParentheses();

            if (IsFixableSwitchExpression(left, semanticModel, cancellationToken))
            {
                ExpressionSyntax right = equalsExpression.Right?.WalkDownParentheses();

                if (IsFixableSwitchExpression(right, semanticModel, cancellationToken)
                    && semanticModel.GetConstantValue(right).HasValue)
                {
                    return switchExpression == null
                        || SyntaxComparer.AreEquivalent(left, switchExpression);
                }
            }

            return false;
        }

        private static bool IsFixableSwitchExpression(
            ExpressionSyntax expression,
            SemanticModel semanticModel,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (expression == null)
                return false;

            ITypeSymbol typeSymbol = semanticModel.GetTypeInfo(expression, cancellationToken).ConvertedType;

            if (typeSymbol.IsEnum())
                return true;

            switch (typeSymbol.SpecialType)
            {
                case SpecialType.System_Boolean:
                case SpecialType.System_Char:
                case SpecialType.System_SByte:
                case SpecialType.System_Byte:
                case SpecialType.System_Int16:
                case SpecialType.System_UInt16:
                case SpecialType.System_Int32:
                case SpecialType.System_UInt32:
                case SpecialType.System_Int64:
                case SpecialType.System_UInt64:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                case SpecialType.System_String:
                    return true;
            }

            if ((typeSymbol is INamedTypeSymbol namedTypeSymbol)
                && namedTypeSymbol.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
            {
                switch (namedTypeSymbol.TypeArguments[0].SpecialType)
                {
                    case SpecialType.System_Boolean:
                    case SpecialType.System_Char:
                    case SpecialType.System_SByte:
                    case SpecialType.System_Byte:
                    case SpecialType.System_Int16:
                    case SpecialType.System_UInt16:
                    case SpecialType.System_Int32:
                    case SpecialType.System_UInt32:
                    case SpecialType.System_Int64:
                    case SpecialType.System_UInt64:
                    case SpecialType.System_Single:
                    case SpecialType.System_Double:
                        return true;
                }
            }

            return false;
        }

        private static Task<Document> RefactorAsync(
            Document document,
            IfStatementSyntax ifStatement,
            CancellationToken cancellationToken)
        {
            SwitchStatementSyntax switchStatement = SwitchStatement(
                GetSwitchExpression(ifStatement),
                List(CreateSwitchSections(ifStatement)));

            switchStatement = switchStatement
                .WithTriviaFrom(ifStatement)
                .WithFormatterAnnotation();

            return document.ReplaceNodeAsync(ifStatement, switchStatement, cancellationToken);
        }

        private static ExpressionSyntax GetSwitchExpression(IfStatementSyntax ifStatement)
        {
            var condition = (BinaryExpressionSyntax)ifStatement.Condition.WalkDownParentheses();

            if (condition.IsKind(SyntaxKind.LogicalOrExpression))
            {
                var right = (BinaryExpressionSyntax)condition.Right.WalkDownParentheses();

                return right.Left.WalkDownParentheses();
            }

            return condition.Left.WalkDownParentheses();
        }

        private static IEnumerable<SwitchSectionSyntax> CreateSwitchSections(IfStatementSyntax ifStatement)
        {
            foreach (IfStatementOrElseClause ifOrElse in ifStatement.GetChain())
            {
                if (ifOrElse.IsIf)
                {
                    ifStatement = ifOrElse.AsIf();

                    var condition = ifStatement.Condition.WalkDownParentheses() as BinaryExpressionSyntax;

                    List<SwitchLabelSyntax> labels = CreateSwitchLabels(condition, new List<SwitchLabelSyntax>());
                    labels.Reverse();

                    SwitchSectionSyntax section = SwitchSection(
                        List(labels),
                        AddBreakStatementIfNecessary(ifStatement.Statement));

                    yield return section;
                }
                else
                {
                    yield return DefaultSwitchSection(AddBreakStatementIfNecessary(ifOrElse.Statement));
                }
            }
        }

        private static SyntaxList<StatementSyntax> AddBreakStatementIfNecessary(StatementSyntax statement)
        {
            if (statement.IsKind(SyntaxKind.Block))
            {
                var block = (BlockSyntax)statement;

                SyntaxList<StatementSyntax> statements = block.Statements;

                if (statements.Any()
                    && IsJumpStatement(statements.Last()))
                {
                    return SingletonList<StatementSyntax>(block);
                }
                else
                {
                    return SingletonList<StatementSyntax>(block.AddStatements(BreakStatement()));
                }
            }
            else
            {
                if (IsJumpStatement(statement))
                {
                    return SingletonList(statement);
                }
                else
                {
                    return SingletonList<StatementSyntax>(Block(statement, BreakStatement()));
                }
            }
        }

        private static bool IsJumpStatement(StatementSyntax statement)
        {
            return statement.IsKind(
                SyntaxKind.BreakStatement,
                SyntaxKind.GotoCaseStatement,
                SyntaxKind.ReturnStatement,
                SyntaxKind.ThrowStatement);
        }

        private static List<SwitchLabelSyntax> CreateSwitchLabels(BinaryExpressionSyntax binaryExpression, List<SwitchLabelSyntax> labels)
        {
            if (binaryExpression.IsKind(SyntaxKind.EqualsExpression))
            {
                labels.Add(CaseSwitchLabel(binaryExpression.Right.WalkDownParentheses()));
            }
            else
            {
                var equalsExpression = (BinaryExpressionSyntax)binaryExpression.Right.WalkDownParentheses();

                labels.Add(CaseSwitchLabel(equalsExpression.Right.WalkDownParentheses()));

                if (binaryExpression.IsKind(SyntaxKind.LogicalOrExpression))
                    return CreateSwitchLabels((BinaryExpressionSyntax)binaryExpression.Left.WalkDownParentheses(), labels);
            }

            return labels;
        }

        private static bool ContainsBreakStatementThatBelongsToParentLoop(StatementSyntax statement)
        {
            if (ShouldCheckBreakStatement())
            {
                foreach (SyntaxNode descendant in statement.DescendantNodes(statement.Span, f => !IsLoopOrNestedMethod(f.Kind())))
                {
                    if (descendant.IsKind(SyntaxKind.BreakStatement))
                        return true;
                }
            }

            return false;

            bool IsLoopOrNestedMethod(SyntaxKind kind)
            {
                return kind.IsLoop() || kind.IsNestedMethod();
            }

            bool ShouldCheckBreakStatement()
            {
                for (SyntaxNode node = statement.Parent; node != null; node = node.Parent)
                {
                    if (node is MemberDeclarationSyntax)
                        break;

                    SyntaxKind kind = node.Kind();

                    if (kind.IsNestedMethod())
                        break;

                    if (kind.IsLoop())
                        return true;
                }

                return false;
            }
        }
    }
}