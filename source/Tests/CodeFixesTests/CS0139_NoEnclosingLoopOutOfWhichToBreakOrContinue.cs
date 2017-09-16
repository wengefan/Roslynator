﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class CS0139_NoEnclosingLoopOutOfWhichToBreakOrContinue
    {
        private static void Foo()
        {
            break;

            string s = null;

            continue;
        }
    }
}
