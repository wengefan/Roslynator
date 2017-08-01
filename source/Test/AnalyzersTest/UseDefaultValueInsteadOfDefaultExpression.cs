// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Analyzers.Test
{
    internal static class UseDefaultValueInsteadOfDefaultExpression
    {
        private static void Foo()
        {
            var f = default(bool);
            var ch = default(char);
            var sb = default(sbyte);
            var b = default(byte);
            var us = default(ushort);
            var s = default(short);
            var ui = default(uint);
            var i = default(int);
            var l = default(long);
            var ul = default(ulong);
            var ft = default(float);
            var de = default(double);
            var dl = default(decimal);
        }

        private static void Foo2()
        {
            bool f = default(bool);
            char ch = default(char);
            sbyte sb = default(sbyte);
            byte b = default(byte);
            ushort us = default(ushort);
            short s = default(short);
            uint ui = default(uint);
            int i = default(int);
            long l = default(long);
            ulong ul = default(ulong);
            float ft = default(float);
            double de = default(double);
            decimal dl = default(decimal);
        }
    }
}
