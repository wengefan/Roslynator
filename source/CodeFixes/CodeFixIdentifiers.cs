// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.CodeFixes
{
    public static partial class CodeFixIdentifiers
    {
        //private class ClassName
        //{
        //    public void MethodName<T>() where T : ClassName where T : ClassName
        //    {

        //    }

        //    public void MethodName2<T>() where T : class, struct, new(), ClassName
        //    {

        //    }

        //    public void MethodName3<T>() where T : class, ClassName
        //    {

        //    }
        //}

        //private class Foo
        //{

        //}

        //public interface IFoo
        //{

        //}

        public const string Prefix = "RCF";
    }
}