﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Refactorings.UseMethodChaining
{
    internal abstract class UseMethodChainingRefactoring
    {
        public static MethodChainingWithoutAssignmentRefactoring WithoutAssignment { get; } = new MethodChainingWithoutAssignmentRefactoring();

        public static MethodChainingWithAssignmentRefactoring WithAssignment { get; } = new MethodChainingWithAssignmentRefactoring();

        public static bool IsFixable(
            MemberInvocationExpressionInfo memberInvocation,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            InvocationExpressionSyntax invocationExpression = memberInvocation.InvocationExpression;

            SyntaxNode parent = invocationExpression.Parent;

            switch (parent?.Kind())
            {
                case SyntaxKind.ExpressionStatement:
                    {
                        var expressionStatement = (ExpressionStatementSyntax)parent;

                        if (!(WalkDownMethodChain(memberInvocation).Expression is IdentifierNameSyntax identifierName))
                            break;

                        string name = identifierName.Identifier.ValueText;

                        return WithoutAssignment.Analyze(memberInvocation, expressionStatement, name, semanticModel, cancellationToken);
                    }
                case SyntaxKind.SimpleAssignmentExpression:
                    {
                        var assinmentExpression = (AssignmentExpressionSyntax)parent;

                        if (!(assinmentExpression.Left is IdentifierNameSyntax identifierName))
                            break;

                        if (assinmentExpression.Right != invocationExpression)
                            break;

                        if (!(assinmentExpression.Parent is ExpressionStatementSyntax expressionStatement))
                            break;

                        string name = identifierName.Identifier.ValueText;

                        if (name != (WalkDownMethodChain(memberInvocation).Expression as IdentifierNameSyntax)?.Identifier.ValueText)
                            break;

                        return WithAssignment.Analyze(memberInvocation, expressionStatement, name, semanticModel, cancellationToken);
                    }
            }

            return false;
        }

        public bool Analyze(
            MemberInvocationExpressionInfo memberInvocation,
            StatementSyntax statement,
            string name,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            if (statement.SpanOrTrailingTriviaContainsDirectives())
                return false;

            StatementsInfo statementsInfo = SyntaxInfo.StatementsInfo(statement);

            if (!statementsInfo.Success)
                return false;

            SyntaxList<StatementSyntax> statements = statementsInfo.Statements;

            if (statements.Count == 1)
                return false;

            if (!semanticModel.TryGetMethodInfo(memberInvocation.InvocationExpression, out MethodInfo methodInfo, cancellationToken))
                return false;

            ITypeSymbol typeSymbol = methodInfo.ReturnType;

            int i = statements.IndexOf(statement);

            if (i != 0
                && IsFixableStatement(statements[i - 1], name, typeSymbol, semanticModel, cancellationToken))
            {
                return false;
            }

            int j = i;
            while (j < statements.Count - 1)
            {
                if (!IsFixableStatement(statements[j + 1], name, typeSymbol, semanticModel, cancellationToken))
                    break;

                j++;
            }

            return j > i;
        }

        protected abstract bool IsFixableStatement(
            StatementSyntax statement,
            string name,
            ITypeSymbol typeSymbol,
            SemanticModel semanticModel,
            CancellationToken cancellationToken);

        protected abstract InvocationExpressionSyntax GetInvocationExpression(ExpressionStatementSyntax expressionStatement);

        protected static MemberInvocationExpressionInfo WalkDownMethodChain(MemberInvocationExpressionInfo memberInvocation)
        {
            while (true)
            {
                MemberInvocationExpressionInfo memberInvocation2 = SyntaxInfo.MemberInvocationExpressionInfo(memberInvocation.Expression);

                if (memberInvocation2.Success)
                {
                    memberInvocation = memberInvocation2;
                }
                else
                {
                    break;
                }
            }

            return memberInvocation;
        }

        public async Task<Document> RefactorAsync(
            Document document,
            ExpressionStatementSyntax expressionStatement,
            CancellationToken cancellationToken)
        {
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            InvocationExpressionSyntax invocationExpression = GetInvocationExpression(expressionStatement);

            semanticModel.TryGetMethodInfo(invocationExpression, out MethodInfo methodInfo, cancellationToken);

            MemberInvocationExpressionInfo memberInvocation = SyntaxInfo.MemberInvocationExpressionInfo(invocationExpression);

            ITypeSymbol typeSymbol = methodInfo.ReturnType;

            string name = ((IdentifierNameSyntax)WalkDownMethodChain(memberInvocation).Expression).Identifier.ValueText;

            StatementsInfo statementsInfo = StatementsInfo.Create(expressionStatement);

            SyntaxList<StatementSyntax> statements = statementsInfo.Statements;

            int index = statements.IndexOf(expressionStatement);

            string indentation = CSharpFormatter.GetIncreasedIndentation(expressionStatement, cancellationToken).ToString();

            var sb = new StringBuilder(invocationExpression.ToString());

            int j = index;
            while (j < statements.Count - 1)
            {
                StatementSyntax statement = statements[j + 1];

                if (!IsFixableStatement(statement, name, typeSymbol, semanticModel, cancellationToken))
                    break;

                sb.AppendLine();
                sb.Append(indentation);
                sb.Append(GetTextToAppend((ExpressionStatementSyntax)statement));

                j++;
            }

            StatementSyntax lastStatement = statements[j];

            SyntaxList<StatementSyntax> newStatements = statements;

            while (j > index)
            {
                newStatements = newStatements.RemoveAt(j);
                j--;
            }

            ExpressionSyntax newInvocationExpression = SyntaxFactory.ParseExpression(sb.ToString());

            SyntaxTriviaList trailingTrivia = statementsInfo
                .Node
                .DescendantTrivia(TextSpan.FromBounds(invocationExpression.Span.End, lastStatement.Span.End))
                .ToSyntaxTriviaList()
                .EmptyIfWhitespace()
                .AddRange(lastStatement.GetTrailingTrivia());

            ExpressionStatementSyntax newExpressionStatement = expressionStatement
                .ReplaceNode(invocationExpression, newInvocationExpression)
                .WithLeadingTrivia(expressionStatement.GetLeadingTrivia())
                .WithTrailingTrivia(trailingTrivia)
                .WithFormatterAndSimplifierAnnotations();

            newStatements = newStatements.ReplaceAt(index, newExpressionStatement);

            return await document.ReplaceStatementsAsync(statementsInfo, newStatements, cancellationToken).ConfigureAwait(false);
        }

        private string GetTextToAppend(ExpressionStatementSyntax expressionStatement)
        {
            MemberInvocationExpressionInfo memberInvocation = SyntaxInfo.MemberInvocationExpressionInfo(GetInvocationExpression(expressionStatement));

            MemberInvocationExpressionInfo firstMemberInvocation = WalkDownMethodChain(memberInvocation);

            InvocationExpressionSyntax invocationExpression = memberInvocation.InvocationExpression;

            return invocationExpression
                .ToString()
                .Substring(firstMemberInvocation.OperatorToken.SpanStart - invocationExpression.SpanStart);
        }
    }
}