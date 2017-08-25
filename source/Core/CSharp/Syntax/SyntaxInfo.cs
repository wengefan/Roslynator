// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Syntax
{
    public static class SyntaxInfo
    {
        public static AsExpressionInfo AsExpressionInfo(
            SyntaxNode node,
            SyntaxInfoOptions options = null)
        {
            return Syntax.AsExpressionInfo.Create(
                node,
                options);
        }

        public static AsExpressionInfo AsExpressionInfo(
            BinaryExpressionSyntax binaryExpression,
            SyntaxInfoOptions options = null)
        {
            return Syntax.AsExpressionInfo.Create(
                binaryExpression,
                options);
        }

        public static BinaryExpressionChainInfo BinaryExpressionChainInfo(
            SyntaxNode node,
            SyntaxKind kind,
            SyntaxInfoOptions options = null)
        {
            return Syntax.BinaryExpressionChainInfo.Create(
                node,
                kind,
                options);
        }

        public static BinaryExpressionChainInfo BinaryExpressionChainInfo(BinaryExpressionSyntax binaryExpression)
        {
            return Syntax.BinaryExpressionChainInfo.Create(binaryExpression);
        }

        public static BinaryExpressionInfo BinaryExpressionInfo(
            SyntaxNode node,
            SyntaxInfoOptions options = null)
        {
            return Syntax.BinaryExpressionInfo.Create(
                node,
                options);
        }

        public static BinaryExpressionInfo BinaryExpressionInfo(
            BinaryExpressionSyntax binaryExpression,
            SyntaxInfoOptions options = null)
        {
            return Syntax.BinaryExpressionInfo.Create(
                binaryExpression,
                options);
        }

        public static ConditionalExpressionInfo ConditionalExpressionInfo(
            SyntaxNode node,
            SyntaxInfoOptions options = null)
        {
            return Syntax.ConditionalExpressionInfo.Create(
                node,
                options);
        }

        public static ConditionalExpressionInfo ConditionalExpressionInfo(
            ConditionalExpressionSyntax conditionalExpression,
            SyntaxInfoOptions options = null)
        {
            return Syntax.ConditionalExpressionInfo.Create(
                conditionalExpression,
                options);
        }

        public static IfStatementInfo IfStatementInfo(IfStatementSyntax ifStatement)
        {
            return Syntax.IfStatementInfo.Create(ifStatement);
        }

        public static MemberInvocationExpressionInfo MemberInvocationExpressionInfo(
            SyntaxNode node,
            SyntaxInfoOptions options = null)
        {
            return Syntax.MemberInvocationExpressionInfo.Create(
                node,
                options);
        }

        public static MemberInvocationExpressionInfo MemberInvocationExpressionInfo(
            InvocationExpressionSyntax invocationExpression,
            SyntaxInfoOptions options = null)
        {
            return Syntax.MemberInvocationExpressionInfo.Create(
                invocationExpression,
                options);
        }

        public static MemberInvocationStatementInfo MemberInvocationStatementInfo(
            SyntaxNode node,
            SyntaxInfoOptions options = null)
        {
            return Syntax.MemberInvocationStatementInfo.Create(
                node,
                options);
        }

        public static MemberInvocationStatementInfo MemberInvocationStatementInfo(
            ExpressionStatementSyntax expressionStatement,
            SyntaxInfoOptions options = null)
        {
            return Syntax.MemberInvocationStatementInfo.Create(
                expressionStatement,
                options);
        }

        public static NullCheckExpressionInfo NullCheckExpressionInfo(
            SyntaxNode node,
            SyntaxInfoOptions options = null,
            NullCheckKind allowedKinds = NullCheckKind.All,
            SemanticModel semanticModel = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return Syntax.NullCheckExpressionInfo.Create(
                node,
                options,
                allowedKinds,
                semanticModel,
                cancellationToken);
        }

        public static SimpleAssignmentExpressionInfo SimpleAssignmentExpressionInfo(
            SyntaxNode node,
            SyntaxInfoOptions options = null)
        {
            return Syntax.SimpleAssignmentExpressionInfo.Create(node, options);
        }

        public static SimpleAssignmentExpressionInfo SimpleAssignmentExpressionInfo(
            AssignmentExpressionSyntax assignmentExpression,
            SyntaxInfoOptions options = null)
        {
            return Syntax.SimpleAssignmentExpressionInfo.Create(assignmentExpression, options);
        }

        public static SimpleAssignmentStatementInfo SimpleAssignmentStatementInfo(
            SyntaxNode node,
            SyntaxInfoOptions options = null)
        {
            return Syntax.SimpleAssignmentStatementInfo.Create(node, options);
        }

        public static SimpleAssignmentStatementInfo SimpleAssignmentStatementInfo(
            ExpressionStatementSyntax expressionStatement,
            SyntaxInfoOptions options = null)
        {
            return Syntax.SimpleAssignmentStatementInfo.Create(expressionStatement, options);
        }

        public static SimpleIfElseInfo SimpleIfElseInfo(
            IfStatementSyntax ifStatement,
            SyntaxInfoOptions options = null)
        {
            return Syntax.SimpleIfElseInfo.Create(ifStatement, options);
        }

        public static SimpleIfStatementInfo SimpleIfStatementInfo(
            SyntaxNode node,
            SyntaxInfoOptions options = null)
        {
            return Syntax.SimpleIfStatementInfo.Create(node, options);
        }

        public static SimpleIfStatementInfo SimpleIfStatementInfo(
            IfStatementSyntax ifStatement,
            SyntaxInfoOptions options = null)
        {
            return Syntax.SimpleIfStatementInfo.Create(ifStatement, options);
        }

        public static SingleLocalDeclarationStatementInfo SingleLocalDeclarationStatementInfo(
            LocalDeclarationStatementSyntax localDeclarationStatement,
            SyntaxInfoOptions options = null)
        {
            return Syntax.SingleLocalDeclarationStatementInfo.Create(localDeclarationStatement, options);
        }

        public static SingleLocalDeclarationStatementInfo SingleLocalDeclarationStatementInfo(
            ExpressionSyntax expression,
            SyntaxInfoOptions options = null)
        {
            return Syntax.SingleLocalDeclarationStatementInfo.Create(expression, options);
        }

        public static SingleParameterLambdaExpressionInfo SingleParameterLambdaExpressionInfo(
            SyntaxNode node,
            SyntaxInfoOptions options = null)
        {
            return Syntax.SingleParameterLambdaExpressionInfo.Create(node, options);
        }

        public static SingleParameterLambdaExpressionInfo SingleParameterLambdaExpressionInfo(
            LambdaExpressionSyntax lambdaExpression,
            SyntaxInfoOptions options = null)
        {
            return Syntax.SingleParameterLambdaExpressionInfo.Create(lambdaExpression, options);
        }

        public static StringConcatenationExpressionInfo StringConcatenationExpressionInfo(
            BinaryExpressionSyntax binaryExpression,
            SemanticModel semanticModel,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return Syntax.StringConcatenationExpressionInfo.Create(binaryExpression, semanticModel, cancellationToken);
        }

        internal static StringConcatenationExpressionInfo StringConcatenationExpressionInfo(
            BinaryExpressionSelection binaryExpressionSelection,
            SemanticModel semanticModel,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return Syntax.StringConcatenationExpressionInfo.Create(binaryExpressionSelection, semanticModel, cancellationToken);
        }

        public static TypeParameterInfo TypeParameterInfo(TypeParameterSyntax typeParameter)
        {
            return Syntax.TypeParameterInfo.Create(typeParameter);
        }

        public static XmlElementInfo XmlElementInfo(XmlNodeSyntax xmlNode)
        {
            return Syntax.XmlElementInfo.Create(xmlNode);
        }
    }
}