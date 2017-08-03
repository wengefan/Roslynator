// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#pragma warning disable RCS1032, RCS1051, RCS1118, RCS1176

using System.Collections.Generic;

namespace Roslynator.CSharp.Analyzers.Test
{
    internal static class SimplifyConditionalExpression2
    {
        private class Foo
        {
            private void Bar()
            {
                bool condition = false;

                string s = null;

                s = (condition) ? s.Substring(1) : s.Substring(2);

                s = (condition)
                    ? s.Substring(1)
                    : s.Substring(2);

                s = (condition) ? s?.Substring(1) : s?.Substring(2);

                s = (condition)
                    ? s?.Substring(1)
                    : s?.Substring(2);

                s = (condition) ? this.Dictionary[1] : this.Dictionary[2];

                s = (condition)
                    ? this.Dictionary[1]
                    : this.Dictionary[2];

                s = (condition) ? this.Dictionary?[1] : this.Dictionary?[2];

                s = (condition)
                    ? this.Dictionary?[1]
                    : this.Dictionary?[2];
            }

            public Dictionary<int, string> Dictionary { get; }
        }
    }
}
