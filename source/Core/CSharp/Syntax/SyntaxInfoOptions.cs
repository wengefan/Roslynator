// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Syntax
{
    public class SyntaxInfoOptions
    {
        public static SyntaxInfoOptions Default { get; } = new SyntaxInfoOptions();

        private SyntaxInfoOptions(bool allowMissing = false, bool walkDownParentheses = true)
        {
            AllowMissing = allowMissing;
            WalkDownParentheses = walkDownParentheses;
        }

        public bool AllowMissing { get; }

        public bool WalkDownParentheses { get; }

        internal ExpressionSyntax Walk(SyntaxNode node)
        {
            return Walk(node as ExpressionSyntax);
        }

        internal ExpressionSyntax Walk(ExpressionSyntax expression)
        {
            return expression?.WalkDownParenthesesIf(WalkDownParentheses);
        }

        internal ExpressionSyntax WalkAndCheck(SyntaxNode node)
        {
            ExpressionSyntax expression = Walk(node);

            if (Check(expression))
                return expression;

            return null;
        }

        internal ExpressionSyntax WalkAndCheck(ExpressionSyntax expression)
        {
            expression = Walk(expression);

            if (Check(expression))
                return expression;

            return null;
        }

        internal bool Check(SyntaxNode node)
        {
            return node != null
                && (AllowMissing || !node.IsMissing);
        }
    }
}