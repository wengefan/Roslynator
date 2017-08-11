// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;

namespace Roslynator.CSharp.Syntax
{
    internal static class SyntaxHelper
    {
        public static bool CheckNode(SyntaxNode node, bool allowMissing)
        {
            return node != null
                && (allowMissing || !node.IsMissing);
        }

        public static bool CheckNode(SyntaxNode node, SyntaxInfoOptions options)
        {
            return node != null
                && (options.AllowMissing || !node.IsMissing);
        }
    }
}