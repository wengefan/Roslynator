// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.CodeFixes.Test
{
    internal static class ChangeAccessibility
    {
        private sealed class SealedFoo
        {
            protected void Bar()
            {
            }

            public string Property { get; protected set; }
        }

        private static class StaticFoo
        {
            protected static void Bar()
            {
            }

            protected static void Bar2()
            {
            }
        }

        private abstract class Foo
        {
            private abstract void FooMethod();

            private abstract object FooProperty { get; private set; }

            private abstract object this[int index] { get; private set; }
        }

        private class Foo2
        {
            private virtual void FooMethod()
            {
            }

            private virtual object FooProperty { get; set; }

            private virtual object this[int index]
            {
                get { return null; }
                set { }
            }
        }
    }
}
