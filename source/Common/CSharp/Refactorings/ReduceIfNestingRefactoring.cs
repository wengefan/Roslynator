// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.Refactorings
{
    internal static class ReduceIfNestingRefactoring
    {
        public static bool IsFixable(
            IfStatementSyntax ifStatement,
            SemanticModel semanticModel,
            INamedTypeSymbol taskType = null,
            CancellationToken cancellationToken = default(CancellationToken),
            bool topLevelOnly = false)
        {
            return IsFixable(ifStatement)
                && IsFixableCore(ifStatement, semanticModel, taskType, cancellationToken, topLevelOnly);
        }

        private static bool IsFixableCore(
            IfStatementSyntax ifStatement,
            SemanticModel semanticModel,
            INamedTypeSymbol taskType = null,
            CancellationToken cancellationToken = default(CancellationToken),
            bool topLevelOnly = false)
        {
            if (!StatementContainer.TryCreate(ifStatement, out StatementContainer container))
                return false;

            SyntaxNode parent = container.Node.Parent;
            SyntaxKind parentKind = parent.Kind();

            SyntaxList<StatementSyntax> statements = container.Statements;

            if (container.IsSwitchSection
                || parentKind == SyntaxKind.SwitchSection)
            {
                if (topLevelOnly)
                    return false;

                if (statements.Count == 1)
                    return false;

                if (statements.Last().Kind() != SyntaxKind.BreakStatement)
                    return false;

                if (!object.ReferenceEquals(ifStatement, statements[statements.Count - 2]))
                    return false;

                return true;
            }

            if (parentKind.IsKind(
                SyntaxKind.ForStatement,
                SyntaxKind.ForEachStatement,
                SyntaxKind.DoStatement,
                SyntaxKind.WhileStatement))
            {
                return !topLevelOnly
                    && statements.IsLast(ifStatement);
            }

            if (!statements.IsLastStatement(ifStatement, skipLocalFunction: true))
                return false;

            switch (parent)
            {
                case MethodDeclarationSyntax methodDeclaration:
                    {
                        if (methodDeclaration.ReturnsVoid())
                            return true;

                        if (methodDeclaration.Modifiers.Contains(SyntaxKind.AsyncKeyword))
                        {
                            return taskType != null
                                && semanticModel
                                    .GetDeclaredSymbol(methodDeclaration, cancellationToken)?
                                    .ReturnType
                                    .Equals(taskType) == true;
                        }

                        return semanticModel
                                .GetDeclaredSymbol(methodDeclaration, cancellationToken)?
                                .ReturnType
                                .IsIEnumerableOrConstructedFromIEnumerableOfT() == true
                            && methodDeclaration.ContainsYield();
                    }
                case LocalFunctionStatementSyntax localFunction:
                    {
                        if (localFunction.ReturnsVoid())
                            return true;

                        if (localFunction.Modifiers.Contains(SyntaxKind.AsyncKeyword))
                        {
                            return taskType != null
                                && ((IMethodSymbol)semanticModel.GetDeclaredSymbol(localFunction, cancellationToken))?
                                    .ReturnType
                                    .Equals(taskType) == true;
                        }

                        return ((IMethodSymbol)semanticModel.GetDeclaredSymbol(localFunction, cancellationToken))?
                                .ReturnType
                                .IsIEnumerableOrConstructedFromIEnumerableOfT() == true
                            && localFunction.ContainsYield();
                    }
                case AnonymousFunctionExpressionSyntax anonymousFunction:
                    {
                        var methodSymbol = semanticModel.GetSymbol(anonymousFunction, cancellationToken) as IMethodSymbol;

                        if (methodSymbol == null)
                            return false;

                        if (methodSymbol.ReturnsVoid)
                            return true;

                        return anonymousFunction.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword)
                            && methodSymbol.ReturnType.Equals(taskType);
                    }
                case IfStatementSyntax ifStatement2:
                    {
                        if (topLevelOnly)
                            return false;

                        if (ifStatement2.Parent is ElseClauseSyntax elseClause)
                        {
                            return ifStatement2.Else == null
                                && IsFixableCore(ifStatement2.GetTopmostIf(), semanticModel, taskType, cancellationToken, topLevelOnly);
                        }
                        else
                        {
                            return IsFixable(ifStatement2, semanticModel, taskType, cancellationToken, topLevelOnly);
                        }
                    }
                case ElseClauseSyntax elseClause:
                    {
                        return !topLevelOnly
                            && IsFixableCore(elseClause.GetTopmostIf(), semanticModel, taskType, cancellationToken, topLevelOnly);
                    }
            }

            return false;
        }

        internal static bool IsFixableRecursively(IfStatementSyntax ifStatement)
        {
            StatementSyntax statement = ifStatement.Statement;

            if (statement == null)
                return false;

            SyntaxKind kind = statement.Kind();

            if (kind == SyntaxKind.Block)
            {
                statement = ((BlockSyntax)statement).Statements.LastOrDefault();

                if (statement == null)
                    return false;

                kind = statement.Kind();
            }

            return kind == SyntaxKind.IfStatement
                && IsFixable((IfStatementSyntax)statement);
        }

        private static bool IsFixable(IfStatementSyntax ifStatement)
        {
            return ifStatement.IsSimpleIf()
                && ifStatement.Condition?.IsMissing == false
                && (ifStatement.Statement is BlockSyntax block)
                && block.Statements.Any();
        }

        public static Task<Document> RefactorAsync(
            Document document,
            IfStatementSyntax ifStatement,
            bool recursive,
            CancellationToken cancellationToken)
        {
            StatementContainer container = StatementContainer.Create(ifStatement);

            CSharpSyntaxNode node = container.Node;

            StatementSyntax jumpStatement = GetJumpStatement(node);

            var rewriter = new IfStatementRewriter(jumpStatement, recursive);

            SyntaxNode newNode = rewriter.Visit(node);

            return document.ReplaceNodeAsync(node, newNode, cancellationToken);
        }

        private static StatementSyntax GetJumpStatement(SyntaxNode node)
        {
            while (node != null)
            {
                switch (node.Kind())
                {
                    case SyntaxKind.MethodDeclaration:
                        {
                            if (((MethodDeclarationSyntax)node).ContainsYield())
                            {
                                return YieldBreakStatement();
                            }
                            else
                            {
                                return ReturnStatement();
                            }
                        }
                    case SyntaxKind.LocalFunctionStatement:
                        {
                            if (((LocalFunctionStatementSyntax)node).ContainsYield())
                            {
                                return YieldBreakStatement();
                            }
                            else
                            {
                                return ReturnStatement();
                            }
                        }
                    case SyntaxKind.SimpleLambdaExpression:
                    case SyntaxKind.ParenthesizedLambdaExpression:
                    case SyntaxKind.AnonymousMethodExpression:
                        {
                            return ReturnStatement();
                        }
                    case SyntaxKind.ForStatement:
                    case SyntaxKind.ForEachStatement:
                    case SyntaxKind.DoStatement:
                    case SyntaxKind.WhileStatement:
                        {
                            return ContinueStatement();
                        }
                    case SyntaxKind.SwitchSection:
                        {
                            return BreakStatement();
                        }
                }

                node = node.Parent;
            }

            throw new InvalidOperationException("");
        }

        private class IfStatementRewriter : CSharpSyntaxRewriter
        {
            private readonly StatementSyntax _jumpStatement;
            private readonly bool _recursive;
            private StatementContainer _container;

            public IfStatementRewriter(StatementSyntax jumpStatement, bool recursive)
            {
                _jumpStatement = jumpStatement;
                _recursive = recursive;
            }

            public override SyntaxNode VisitIfStatement(IfStatementSyntax node)
            {
                if (node.Parent == _container.Node)
                {
                    return base.VisitIfStatement(node);
                }
                else
                {
                    return node;
                }
            }

            public override SyntaxNode VisitSwitchSection(SwitchSectionSyntax node)
            {
                if (_container.Node == null)
                {
                    return Rewrite(new StatementContainer(node));
                }

                return node;
            }

            public override SyntaxNode VisitBlock(BlockSyntax node)
            {
                if (_container.Node == null
                    && node.IsParentKind(SyntaxKind.SwitchSection))
                {
                    return Rewrite(new StatementContainer(node));
                }

                _container = new StatementContainer(node);

                if (node.LastStatementOrDefault(skipLocalFunction: true) is IfStatementSyntax ifStatement
                    && IsFixable(ifStatement))
                {
                    return Rewrite(_container, ifStatement);
                }

                return node;
            }

            private SyntaxNode Rewrite(StatementContainer container)
            {
                _container = container;

                SyntaxList<StatementSyntax> statements = _container.Statements;

                var ifStatement = (IfStatementSyntax)statements[statements.Count - 2];

                return Rewrite(_container, ifStatement);
            }

            private SyntaxNode Rewrite(StatementContainer container, IfStatementSyntax ifStatement)
            {
                SyntaxList<StatementSyntax> statements = container.Statements;

                int index = statements.IndexOf(ifStatement);

                if (_recursive)
                    ifStatement = (IfStatementSyntax)VisitIfStatement(ifStatement);

                var block = (BlockSyntax)ifStatement.Statement;

                ExpressionSyntax newCondition = Negator.LogicallyNegate(ifStatement.Condition);

                BlockSyntax newBlock = block.WithStatements(SingletonList(_jumpStatement));

                if (!block
                    .Statements
                    .First()
                    .GetLeadingTrivia()
                    .Any(f => f.IsEndOfLineTrivia()))
                {
                    newBlock = newBlock.WithCloseBraceToken(newBlock.CloseBraceToken.AppendToTrailingTrivia(NewLine()));
                }

                IfStatementSyntax newIfStatement = ifStatement
                    .WithCondition(newCondition)
                    .WithStatement(newBlock)
                    .WithFormatterAnnotation();

                SyntaxList<StatementSyntax> newStatements = statements
                    .ReplaceAt(index, newIfStatement)
                    .InsertRange(index + 1, block.Statements.Select(f => f.WithFormatterAnnotation()));

                return container.NodeWithStatements(newStatements);
            }
        }
    }
}
