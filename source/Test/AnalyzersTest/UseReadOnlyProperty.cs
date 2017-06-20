// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

#pragma warning disable RCS1016, RCS1048, RCS1060, RCS1081, RCS1085, RCS1163, RCS1176

namespace Roslynator.CSharp.Analyzers.Test
{
    public partial class UseReadOnlyProperty
    {
        private string _expandedProperty;

        public static string StaticProperty { get; private set; }
        public string Property { get; private set; }
        public int IntProperty { get; private set; }
        public StringSplitOptions EnumProperty { get; private set; }

        public string ExpandedProperty
        {
            get { return _expandedProperty; }
            private set { _expandedProperty = value; }
        }

        //n

        public string PropertyWithPublicSet { get; set; }
        public static string StaticAssignedInInstanceConstructor { get; private set; }
        public string Assigned { get; private set; }
        public string InSimpleLambda { get; private set; }
        public string InParenthesizedLambda { get; private set; }
        public string InAnonymousMethod { get; private set; }
        public string InLocalFunction { get; private set; }

        public string ExpandedPropertyAssignedInConstructor
        {
            get { return _expandedProperty; }
            private set { _expandedProperty = value; }
        }

        public string ExpandedPropertyAssignedInMethod
        {
            get { return _expandedProperty; }
            private set { _expandedProperty = value; }
        }

        public string ExpandedPropertyWithPublicSetter
        {
            get { return _expandedProperty; }
            set { _expandedProperty = value; }
        }

        [DataMember]
        public string PropertyWithDataMemberAttribute { get; private set; }

        public string PrivateSetHasAttribute { get; [DebuggerStepThrough]private set; }

        static UseReadOnlyProperty()
        {
            StaticProperty = null;
        }

        public UseReadOnlyProperty()
        {
            Property = null;
            IntProperty = 0;
            EnumProperty = StringSplitOptions.None;
            StaticAssignedInInstanceConstructor = null;
            _expandedProperty = null;
            ExpandedPropertyAssignedInConstructor = null;
        }

        private void Bar()
        {
            Assigned = null;
            _expandedProperty = null;
            ExpandedPropertyAssignedInMethod = null;
        }

        private class BaseClassName
        {
        }

        private class ClassName<T> : BaseClassName
        {
            public BaseClassName Property { get; private set; }

            public ClassName<TResult> MethodName<TResult>()
            {
                return new ClassName<TResult>() { Property = this };
            }
        }
    }

    public partial class UseReadOnlyProperty
    {
        public UseReadOnlyProperty(object parameter)
        {
            var items = new List<string>();

            IEnumerable<string> q = items.Select(f =>
            {
                InSimpleLambda = null;
                return f;
            });

            IEnumerable<string> q2 = items.Select((f) =>
            {
                InParenthesizedLambda = null;
                return f;
            });

            IEnumerable<string> q3 = items.Select(delegate (string f)
            {
                InAnonymousMethod = null;
                return f;
            });

            LocalFunction();

            void LocalFunction()
            {
                InLocalFunction = null;
            }
        }
    }
}

namespace System.Runtime.Serialization
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    internal class DataMemberAttribute : Attribute
    {
    }
}
