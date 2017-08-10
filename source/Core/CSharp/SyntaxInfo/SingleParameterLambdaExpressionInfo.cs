// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Roslynator.CSharp.SyntaxInfo.SyntaxInfoHelper;

namespace Roslynator.CSharp.SyntaxInfo
{
    public struct SingleParameterLambdaExpressionInfo
    {
        internal SingleParameterLambdaExpressionInfo(LambdaExpressionSyntax lambdaExpression, ParameterSyntax parameter, CSharpSyntaxNode body)
        {
            LambdaExpression = lambdaExpression;
            Parameter = parameter;
            Body = body;
        }

        public LambdaExpressionSyntax LambdaExpression { get; }

        public ParameterSyntax Parameter { get; }

        public CSharpSyntaxNode Body { get; }

        public ParameterListSyntax ParameterList
        {
            get { return (IsParenthesized) ? (ParameterListSyntax)Parameter.Parent : null; }
        }

        public string ParameterName
        {
            get { return Parameter?.Identifier.ValueText; }
        }

        public bool IsSimple
        {
            get { return LambdaExpression?.IsKind(SyntaxKind.SimpleLambdaExpression) == true; }
        }

        public bool IsParenthesized
        {
            get { return LambdaExpression?.IsKind(SyntaxKind.ParenthesizedLambdaExpression) == true; }
        }

        public static SingleParameterLambdaExpressionInfo Create(LambdaExpressionSyntax lambdaExpression)
        {
            if (lambdaExpression == null)
                throw new ArgumentNullException(nameof(lambdaExpression));

            switch (lambdaExpression.Kind())
            {
                case SyntaxKind.SimpleLambdaExpression:
                    {
                        var simpleLambda = (SimpleLambdaExpressionSyntax)lambdaExpression;

                        return new SingleParameterLambdaExpressionInfo(simpleLambda, simpleLambda.Parameter, simpleLambda.Body);
                    }
                case SyntaxKind.ParenthesizedLambdaExpression:
                    {
                        var parenthesizedLambda = (ParenthesizedLambdaExpressionSyntax)lambdaExpression;

                        ParameterListSyntax parameterList = parenthesizedLambda.ParameterList;

                        if (parameterList == null)
                            throw new ArgumentException("", nameof(lambdaExpression));

                        SeparatedSyntaxList<ParameterSyntax> parameters = parameterList.Parameters;

                        if (parameters.Count != 1)
                            throw new ArgumentException("", nameof(lambdaExpression));

                        return new SingleParameterLambdaExpressionInfo(parenthesizedLambda, parameters[0], parenthesizedLambda.Body);
                    }
            }

            throw new ArgumentException("", nameof(lambdaExpression));
        }

        public static bool TryCreate(SyntaxNode lambdaExpression, out SingleParameterLambdaExpressionInfo lambda)
        {
            if (lambdaExpression?.IsKind(SyntaxKind.SimpleLambdaExpression, SyntaxKind.ParenthesizedLambdaExpression) == true)
                return TryCreate((LambdaExpressionSyntax)lambdaExpression, out lambda);

            lambda = default(SingleParameterLambdaExpressionInfo);
            return false;
        }

        public static bool TryCreate(
            LambdaExpressionSyntax lambdaExpression,
            out SingleParameterLambdaExpressionInfo info,
            bool allowNullOrMissing = false)
        {
            if (lambdaExpression != null)
            {
                SyntaxKind kind = lambdaExpression.Kind();

                if (kind == SyntaxKind.SimpleLambdaExpression)
                {
                    var simpleLambda = (SimpleLambdaExpressionSyntax)lambdaExpression;

                    ParameterSyntax parameter = simpleLambda.Parameter;

                    if (CheckNode(parameter, allowNullOrMissing))
                    {
                        CSharpSyntaxNode body = simpleLambda.Body;

                        if (CheckNode(body, allowNullOrMissing))
                        {
                            info = new SingleParameterLambdaExpressionInfo(simpleLambda, parameter, body);
                            return true;
                        }
                    }
                }
                else if (kind == SyntaxKind.ParenthesizedLambdaExpression)
                {
                    var parenthesizedLambda = (ParenthesizedLambdaExpressionSyntax)lambdaExpression;

                    ParameterListSyntax parameterList = parenthesizedLambda.ParameterList;

                    if (parameterList != null)
                    {
                        SeparatedSyntaxList<ParameterSyntax> parameters = parameterList.Parameters;

                        if (parameters.Count == 1)
                        {
                            ParameterSyntax parameter = parameters[0];

                            if (CheckNode(parameter, allowNullOrMissing))
                            {
                                CSharpSyntaxNode body = parenthesizedLambda.Body;

                                if (CheckNode(body, allowNullOrMissing))
                                {
                                    info = new SingleParameterLambdaExpressionInfo(parenthesizedLambda, parameter, body);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            info = default(SingleParameterLambdaExpressionInfo);
            return false;
        }

        public override string ToString()
        {
            return LambdaExpression?.ToString() ?? base.ToString();
        }
    }
}
