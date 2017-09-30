// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Refactorings.ReduceIfNesting
{
    internal static partial class ReduceIfNestingRefactoring
    {
        private static readonly ReduceIfNestingOptions _noneOptions = new ReduceIfNestingOptions(allowNestedFix: false, allowLoop: false, allowSwitchSection: false);

        private static ReduceIfNestingAnalysis Fail { get; } = new ReduceIfNestingAnalysis();

        private static ReduceIfNestingAnalysis Success(SyntaxNode topNode, SyntaxKind jumpKind)
        {
            return new ReduceIfNestingAnalysis(topNode, jumpKind);
        }

        public static ReduceIfNestingAnalysis Analyze(
            IfStatementSyntax ifStatement,
            SemanticModel semanticModel,
            ReduceIfNestingOptions options,
            INamedTypeSymbol taskSymbol = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!IsFixable(ifStatement))
                return Fail;

            return AnalyzeCore(ifStatement, semanticModel, SyntaxKind.None, options, taskSymbol, cancellationToken);
        }

        private static ReduceIfNestingAnalysis AnalyzeCore(
            IfStatementSyntax ifStatement,
            SemanticModel semanticModel,
            SyntaxKind jumpKind,
            ReduceIfNestingOptions options,
            INamedTypeSymbol taskSymbol = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!StatementContainer.TryCreate(ifStatement, out StatementContainer container))
                return Fail;

            SyntaxNode parent = container.Node.Parent;
            SyntaxKind parentKind = parent.Kind();

            SyntaxList<StatementSyntax> statements = container.Statements;

            if (container.IsSwitchSection
                || parentKind == SyntaxKind.SwitchSection)
            {
                if (!options.AllowSwitchSection)
                    return Fail;

                if (ifStatement != statements.LastButOneOrDefault())
                    return Fail;

                if (!IsFixableJumpStatement(statements.Last(), ref jumpKind))
                    return Fail;

                SyntaxNode node = (container.IsSwitchSection) ? container.Node : parent;

                if (!options.AllowNestedFix
                    && IsNestedFix(node.Parent, semanticModel, taskSymbol, cancellationToken))
                {
                    return Fail;
                }

                return Success(node, jumpKind);
            }

            if (parentKind.IsKind(
                SyntaxKind.ForStatement,
                SyntaxKind.ForEachStatement,
                SyntaxKind.DoStatement,
                SyntaxKind.WhileStatement))
            {
                if (!options.AllowLoop)
                    return Fail;

                StatementSyntax lastStatement = statements.Last();

                if (ifStatement == lastStatement)
                {
                    jumpKind = SyntaxKind.ContinueStatement;
                }
                else
                {
                    if (ifStatement != statements.LastButOneOrDefault())
                        return Fail;

                    if (!IsFixableJumpStatement(lastStatement, ref jumpKind))
                        return Fail;
                }

                if (!options.AllowNestedFix
                    && IsNestedFix(parent.Parent, semanticModel, taskSymbol, cancellationToken))
                {
                    return Fail;
                }

                return Success(parent, jumpKind);
            }

            if (!IsFixable(ifStatement, statements, ref jumpKind))
                return Fail;

            switch (parentKind)
            {
                case SyntaxKind.ConstructorDeclaration:
                case SyntaxKind.DestructorDeclaration:
                case SyntaxKind.SetAccessorDeclaration:
                case SyntaxKind.AddAccessorDeclaration:
                case SyntaxKind.RemoveAccessorDeclaration:
                    {
                        if (jumpKind == SyntaxKind.None)
                        {
                            jumpKind = SyntaxKind.ReturnStatement;
                        }
                        else if (jumpKind != SyntaxKind.ReturnStatement)
                        {
                            return Fail;
                        }

                        return Success(parent, jumpKind);
                    }
                case SyntaxKind.OperatorDeclaration:
                case SyntaxKind.ConversionOperatorDeclaration:
                case SyntaxKind.GetAccessorDeclaration:
                    {
                        if (jumpKind == SyntaxKind.None)
                            return Fail;

                        return Success(parent, jumpKind);
                    }
            }

            switch (parent)
            {
                case MethodDeclarationSyntax methodDeclaration:
                    {
                        if (jumpKind != SyntaxKind.None)
                            return Success(parent, jumpKind);

                        if (methodDeclaration.ReturnsVoid())
                            return Success(parent, SyntaxKind.ReturnStatement);

                        if (methodDeclaration.Modifiers.Contains(SyntaxKind.AsyncKeyword)
                            && taskSymbol != null
                            && semanticModel
                                .GetDeclaredSymbol(methodDeclaration, cancellationToken)?
                                .ReturnType
                                .Equals(taskSymbol) == true)
                        {
                            return Success(parent, SyntaxKind.ReturnStatement);
                        }

                        if (semanticModel
                                .GetDeclaredSymbol(methodDeclaration, cancellationToken)?
                                .ReturnType
                                .IsIEnumerableOrConstructedFromIEnumerableOfT() == true
                            && methodDeclaration.ContainsYield())
                        {
                            return Success(parent, SyntaxKind.YieldBreakStatement);
                        }

                        break;
                    }
                case LocalFunctionStatementSyntax localFunction:
                    {
                        if (jumpKind != SyntaxKind.None)
                            return Success(parent, jumpKind);

                        if (localFunction.ReturnsVoid())
                            return Success(parent, SyntaxKind.ReturnStatement);

                        if (localFunction.Modifiers.Contains(SyntaxKind.AsyncKeyword)
                            && taskSymbol != null
                            && ((IMethodSymbol)semanticModel.GetDeclaredSymbol(localFunction, cancellationToken))?
                                .ReturnType
                                .Equals(taskSymbol) == true)
                        {
                            return Success(parent, SyntaxKind.ReturnStatement);
                        }

                        if (((IMethodSymbol)semanticModel.GetDeclaredSymbol(localFunction, cancellationToken))?
                                .ReturnType
                                .IsIEnumerableOrConstructedFromIEnumerableOfT() == true
                            && localFunction.ContainsYield())
                        {
                            return Success(parent, SyntaxKind.YieldBreakStatement);
                        }

                        break;
                    }
                case AnonymousFunctionExpressionSyntax anonymousFunction:
                    {
                        if (jumpKind != SyntaxKind.None)
                            return Success(parent, jumpKind);

                        var methodSymbol = semanticModel.GetSymbol(anonymousFunction, cancellationToken) as IMethodSymbol;

                        if (methodSymbol == null)
                            return Fail;

                        if (methodSymbol.ReturnsVoid)
                            return Success(parent, SyntaxKind.ReturnStatement);

                        if (anonymousFunction.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword)
                            && methodSymbol.ReturnType.Equals(taskSymbol))
                        {
                            return Success(parent, SyntaxKind.ReturnStatement);
                        }

                        break;
                    }
                case IfStatementSyntax ifStatement2:
                    {
                        if (!options.AllowNestedFix)
                            return Fail;

                        if (ifStatement2.Parent is ElseClauseSyntax elseClause)
                        {
                            if (ifStatement2.Else != null)
                                return Fail;

                            return AnalyzeCore(ifStatement2.GetTopmostIf(), semanticModel, jumpKind, options, taskSymbol, cancellationToken);
                        }
                        else
                        {
                            if (!IsFixable(ifStatement2))
                                return Fail;

                            return AnalyzeCore(ifStatement2, semanticModel, jumpKind, options, taskSymbol, cancellationToken);
                        }
                    }
                case ElseClauseSyntax elseClause:
                    {
                        return AnalyzeCore(elseClause.GetTopmostIf(), semanticModel, jumpKind, options, taskSymbol, cancellationToken);
                    }
            }

            return Fail;
        }

        private static bool IsNestedFix(SyntaxNode node, SemanticModel semanticModel, INamedTypeSymbol taskSymbol, CancellationToken cancellationToken)
        {
            while (node != null)
            {
                if (node is IfStatementSyntax ifStatement)
                {
                    ReduceIfNestingAnalysis analysis = Analyze(ifStatement, semanticModel, _noneOptions, taskSymbol, cancellationToken);

                    return analysis.Success;
                }
                else if (node is MemberDeclarationSyntax)
                {
                    return false;
                }

                node = node.Parent;
            }

            Debug.Fail("");

            return false;
        }

        //TODO: rename
        private static bool IsFixable(
            IfStatementSyntax ifStatement,
            SyntaxList<StatementSyntax> statements,
            ref SyntaxKind jumpKind)
        {
            int i = statements.Count - 1;

            while (i >= 0
                && statements[i].Kind() == SyntaxKind.LocalFunctionStatement)
            {
                i--;
            }

            if (statements[i] == ifStatement)
            {
                return true;
            }
            else if (IsFixableJumpStatement(statements[i], ref jumpKind))
            {
                i--;

                while (i >= 0
                    && statements[i].Kind() == SyntaxKind.LocalFunctionStatement)
                {
                    i--;
                }

                return statements[i] == ifStatement;
            }

            return false;
        }

        private static bool IsFixableJumpStatement(StatementSyntax statement, ref SyntaxKind kind)
        {
            SyntaxKind kind2 = GetJumpKind(statement);

            if (kind2 == SyntaxKind.None)
            {
                kind = SyntaxKind.None;
                return false;
            }
            else if (kind == SyntaxKind.None)
            {
                kind = kind2;
                return true;
            }

            return kind == kind2;
        }

        private static SyntaxKind GetJumpKind(StatementSyntax statement)
        {
            switch (statement)
            {
                case BreakStatementSyntax breakStatement:
                    {
                        return SyntaxKind.BreakStatement;
                    }
                case ContinueStatementSyntax continueStatement:
                    {
                        return SyntaxKind.ContinueStatement;
                    }
                case ReturnStatementSyntax returnStatement:
                    {
                        ExpressionSyntax expression = returnStatement.Expression;

                        if (expression == null)
                            return SyntaxKind.ReturnStatement;

                        SyntaxKind kind = expression.Kind();

                        if (kind.IsKind(
                            SyntaxKind.NullLiteralExpression,
                            SyntaxKind.TrueLiteralExpression,
                            SyntaxKind.FalseLiteralExpression))
                        {
                            return kind;
                        }

                        return SyntaxKind.None;
                    }
                case ThrowStatementSyntax throwStatement:
                    {
                        ExpressionSyntax expression = throwStatement.Expression;

                        if (expression == null)
                            return SyntaxKind.ThrowStatement;

                        return SyntaxKind.None;
                    }
                default:
                    {
                        return SyntaxKind.None;
                    }
            }
        }

        internal static bool IsFixableRecursively(IfStatementSyntax ifStatement, SyntaxKind jumpKind)
        {
            if (!(ifStatement.Statement is BlockSyntax block))
                return false;

            SyntaxList<StatementSyntax> statements = block.Statements;

            if (!statements.Any())
                return false;

            StatementSyntax statement = statements.Last();

            if (statement is IfStatementSyntax ifStatement2)
                return IsFixable(ifStatement2);

            return jumpKind == GetJumpKind(statement)
                && (statements.LastButOneOrDefault() is IfStatementSyntax ifStatement3)
                && IsFixable(ifStatement3);
        }

        private static bool IsFixable(IfStatementSyntax ifStatement)
        {
            if (ifStatement == null)
                return false;

            if (!ifStatement.IsSimpleIf())
                return false;

            if (ifStatement.Condition?.IsMissing != false)
                return false;

            if (!(ifStatement.Statement is BlockSyntax block))
                return false;

            SyntaxList<StatementSyntax> statements = block.Statements;

            if (!statements.Any())
                return false;

            return statements.Count > 1
                || !statements.First().Kind().IsJumpStatementOrYieldBreakStatement();
        }

        public static Task<Document> RefactorAsync(
            Document document,
            IfStatementSyntax ifStatement,
            SyntaxKind jumpKind,
            bool recursive,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            StatementContainer container = StatementContainer.Create(ifStatement);

            CSharpSyntaxNode node = container.Node;

            var rewriter = new ReduceIfStatementRewriter(jumpKind, recursive);

            SyntaxNode newNode = rewriter.Visit(node);

            return document.ReplaceNodeAsync(node, newNode, cancellationToken);
        }
    }
}
