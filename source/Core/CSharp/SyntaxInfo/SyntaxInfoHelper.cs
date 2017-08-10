// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;

namespace Roslynator.CSharp.SyntaxInfo
{
    internal static class SyntaxInfoHelper
    {
        public static bool CheckNode(SyntaxNode node, bool allowNullOrMissing)
        {
            return allowNullOrMissing || node?.IsMissing == false;
        }
    }
}