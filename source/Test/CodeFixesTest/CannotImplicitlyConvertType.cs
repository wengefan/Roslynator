// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.CodeFixes.Test
{
    internal static class CannotImplicitlyConvertType_CS0266
    {
        private static void Foo(object x)
        {
            object[] arr = x;

            string s = x;
        }
    }
}
