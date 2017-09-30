﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.ReduceIfNesting
{
    internal class ReduceIfNestingOptions
    {
        public ReduceIfNestingOptions(
            bool allowNestedFix,
            bool allowLoop,
            bool allowSwitchSection)
        {
            AllowNestedFix = allowNestedFix;
            AllowLoop = allowLoop;
            AllowSwitchSection = allowSwitchSection;
        }

        public bool AllowNestedFix { get; }
        public bool AllowLoop { get; }
        public bool AllowSwitchSection { get; }
    }
}
