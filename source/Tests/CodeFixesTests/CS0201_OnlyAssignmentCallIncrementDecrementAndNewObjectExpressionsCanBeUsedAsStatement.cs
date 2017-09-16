﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class CS0201_OnlyAssignmentCallIncrementDecrementAndNewObjectExpressionsCanBeUsedAsStatement
    {
        private class Foo
        {
            private void Method()
            {
                DateTime _dateTime = DateTime.Now;

                DateTime.Now;
            }

            private static void StaticMethod()
            {
                DateTime _dateTime = DateTime.Now;

                DateTime.Now;
            }
        }
    }
}
