// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp
{
    internal static class CompilerDiagnosticIdentifiers
    {
        public const string Prefix = "CS";
        public const string OperatorCannotBeAppliedToOperands = Prefix + "0019";
        public const string OperatorCannotBeAppliedToOperandOfType = Prefix + "0023";
        public const string CannotImplicitlyConvertType = Prefix + "0029";
        public const string EventInInterfaceCannotHaveAddOrRemoveAccessors = Prefix + "0069";
        public const string ConstraintsAreNotAllowedOnNonGenericDeclarations = Prefix + "0080";
        public const string MemberDoesNotHideAccessibleMember = Prefix + "0109";
        public const string MemberHidesInheritedMember = Prefix + "0114";
        public const string ObjectReferenceIsRequiredForNonStaticMember = Prefix + "0120";
        public const string NoOverloadMatchesDelegate = Prefix + "0123";
        public const string NotAllCodePathsReturnValue = Prefix + "0161";
        public const string UnreachableCodeDetected = Prefix + "0162";
        public const string ControlCannotFallThroughFromOneCaseLabelToAnother = Prefix + "0163";
        public const string LabelHasNotBeenReferenced = Prefix + "0164";
        public const string UseOfUnassignedLocalVariable = Prefix + "0165";
        public const string VariableIsDeclaredButNeverUsed = Prefix + "0168";
        public const string VariableIsAssignedButItsValueIsNeverUsed = Prefix + "0219";
        public const string ConstantValueCannotBeConverted = Prefix + "0221";
        public const string ParamsParameterMustBeSingleDimensionalArray = Prefix + "0225";
        public const string MissingPartialModifier = Prefix + "0260";
        public const string PartialDeclarationsHaveConfictingAccessibilityModifiers = Prefix + "0262";
        public const string CannotImplicitlyConvertTypeExplicitConversionExists = Prefix + "0266";
        public const string UsingGenericTypeRequiresTypeArguments = Prefix + "0305";
        public const string MethodHasWrongReturnType = Prefix + "0407";
        public const string CannotConvertMethodGroupToNonDelegateType = Prefix + "0428";
        public const string MemberCannotDeclareBodyBecauseItIsNotMarkedAbstract = Prefix + "0500";
        public const string CannotChangeAccessModifiersWhenOverridingInheritedMember = Prefix + "0507";
        public const string MemberIsAbstractButItIsContainedInNonAbstractClass = Prefix + "0513";
        public const string MemberReturnTypeMustMatchOverriddenMemberReturnType = Prefix + "0508";
        public const string InterfaceMembersCannotHaveDefinition = Prefix + "0531";
        public const string UserDefinedOperatorMustBeDeclaredStaticAndPublic = Prefix + "0558";
        public const string CannotHaveInstancePropertyOrFieldInitializersInStruct = Prefix + "0573";
        public const string DuplicateAttribute = Prefix + "0579";
        public const string NewProtectedMemberDeclaredInSealedClass = Prefix + "0628";
        public const string CannotDeclareInstanceMembersInStaticClass = Prefix + "0708";
        public const string StaticClassesCannotHaveInstanceConstructors = Prefix + "0710";
        public const string PartialMethodMayNotHaveMultipleDefiningDeclarations = Prefix + "0756";
        public const string CannotAssignMethodGroupToImplicitlyTypedVariable = Prefix + "0815";
        public const string SemicolonExpected = Prefix + "1002";
        public const string DuplicateModifier = Prefix + "1004";
        public const string EmbeddedStatementCannotBeDeclarationOrLabeledStatement = Prefix + "1023";
        public const string StaticClassesCannotContainProtectedMembers = Prefix + "1057";
        public const string TypeDoesNotContainDefinitionAndNoExtensionMethodCouldBeFound = Prefix + "1061";
        public const string CannotConvertArgumentType = Prefix + "1503";
        public const string MissingXmlCommentForPubliclyVisibleTypeOrMember = Prefix + "1591";
        public const string ArgumentMayNotBePassedWithRefKeyword = Prefix + "1615";
        public const string ArgumentMustBePassedWithOutKeyword = Prefix + "1620";
        public const string CannotReturnValueFromIterator = Prefix + "1622";
        public const string TypeUsedInUsingStatementMustBeImplicitlyConvertibleToIDisposable = Prefix + "1674";
        public const string MemberTypeMustMatchOverriddenMemberType = Prefix + "1715";
        public const string AssignmentMadeToSameVariable = Prefix + "1717";
        public const string BaseClassMustComeBeforeAnyInterface = Prefix + "1722";
        public const string NonInvocableMemberCannotBeUsedLikeMethod = Prefix + "1955";
        public const string ControlCannotFallOutOfSwitchFromFinalCaseLabel = Prefix + "8070";
        public const string PartialMethodMustBeDeclaredWithinPartialClassOrPartialStruct = Prefix + "0751";
    }
}
