// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp
{
    internal static class CSharpErrorCodes
    {
        public const string Prefix = "CS";

        public const string OperatorCannotBeAppliedToOperands = Prefix + "0019";
        public const string CannotImplicitlyConvertType = Prefix + "0029";
        public const string NotAllCodePathsReturnValue = Prefix + "0161";
        public const string UnreachableCodeDetected = Prefix + "0162";
        public const string VariableIsDeclaredButNeverUsed = Prefix + "0168";
        public const string VariableIsAssignedButItsValueIsNeverUsed = Prefix + "0219";
        public const string CannotImplicitlyConvertTypeExplicitConversionExists = Prefix + "0266";
        public const string CannotChangeAccessModifiersWhenOverridingInheritedMember = Prefix + "0507";
        public const string MemberTypeMustMatchOverridenMemberType = Prefix + "0508";
        public const string MissingXmlComment = Prefix + "1591";
        public const string ArgumentMustBePassedWitOutKeyword = Prefix + "1620";
        public const string CannotReturnValueFromIterator = Prefix + "1622";
        public const string TypeUsedInUsingStatementMustBeImplicitlyConvertibleToIDisposable = Prefix + "1674";
        public const string BaseClassMustComeBeforeAnyInterface = Prefix + "1722";
    }
}
