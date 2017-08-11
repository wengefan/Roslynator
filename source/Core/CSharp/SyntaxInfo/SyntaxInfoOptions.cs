// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;

namespace Roslynator.CSharp.Syntax
{
    public class SyntaxInfoOptions
    {
        public static SyntaxInfoOptions Default { get; } = new SyntaxInfoOptions();

        public SyntaxInfoOptions(bool allowMissing = false, bool walkDownParentheses = true)
        {
            AllowMissing = allowMissing;
            WalkDownParentheses = walkDownParentheses;
        }

        public bool AllowMissing { get; }

        public bool WalkDownParentheses { get; }

        internal bool CheckNode(SyntaxNode node)
        {
            return node != null
                && (AllowMissing || !node.IsMissing);
        }
    }
}