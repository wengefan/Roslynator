// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.ReduceIfNesting
{
    internal static class ReduceIfNestingOptionsExtensions
    {
        public static bool AllowsNestedFix(this ReduceIfNestingOptions options)
        {
            return (options & ReduceIfNestingOptions.AllowNestedFix) != 0;
        }

        public static bool AllowsLoop(this ReduceIfNestingOptions options)
        {
            return (options & ReduceIfNestingOptions.AllowLoop) != 0;
        }

        public static bool AllowsSwitchSection(this ReduceIfNestingOptions options)
        {
            return (options & ReduceIfNestingOptions.AllowSwitchSection) != 0;
        }
    }
}
