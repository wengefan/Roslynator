﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp.Comparers;
using Roslynator.CSharp.Refactorings;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MemberDeclarationCodeFixProvider))]
    [Shared]
    public class MemberDeclarationCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    CompilerDiagnosticIdentifiers.MissingXmlCommentForPubliclyVisibleTypeOrMember,
                    CompilerDiagnosticIdentifiers.MethodReturnTypeMustMatchOverriddenMethodReturnType,
                    CompilerDiagnosticIdentifiers.MemberTypeMustMatchOverriddenMemberType,
                    CompilerDiagnosticIdentifiers.MissingPartialModifier,
                    CompilerDiagnosticIdentifiers.PartialMethodMustBeDeclaredWithinPartialClassOrPartialStruct,
                    CompilerDiagnosticIdentifiers.MemberIsAbstractButItIsContainedInNonAbstractClass,
                    CompilerDiagnosticIdentifiers.ObjectReferenceIsRequiredForNonStaticMember,
                    CompilerDiagnosticIdentifiers.StaticConstructorMustBeParameterless,
                    CompilerDiagnosticIdentifiers.PartialMethodsMustHaveVoidReturnType);
            }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            if (!Settings.IsCodeFixEnabled(CodeFixIdentifiers.AddDocumentationComment)
                && !Settings.IsCodeFixEnabled(CodeFixIdentifiers.ChangeMethodReturnType)
                && !Settings.IsCodeFixEnabled(CodeFixIdentifiers.MemberTypeMustMatchOverriddenMemberType)
                && !Settings.IsCodeFixEnabled(CodeFixIdentifiers.AddPartialModifier)
                && !Settings.IsCodeFixEnabled(CodeFixIdentifiers.MakeContainingClassAbstract)
                && !Settings.IsCodeFixEnabled(CodeFixIdentifiers.MakeMemberNonStatic)
                && !Settings.IsCodeFixEnabled(CodeFixIdentifiers.RemoveParametersFromStaticConstructor))
            {
                return;
            }

            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out MemberDeclarationSyntax memberDeclaration))
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case CompilerDiagnosticIdentifiers.MissingXmlCommentForPubliclyVisibleTypeOrMember:
                        {
                            if (!Settings.IsCodeFixEnabled(CodeFixIdentifiers.AddDocumentationComment))
                                break;

                            CodeAction codeAction = CodeAction.Create(
                           "Add documentation comment",
                           cancellationToken => AddDocumentationCommentRefactoring.RefactorAsync(context.Document, memberDeclaration, false, cancellationToken),
                           GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);

                            CodeAction codeAction2 = CodeAction.Create(
                                "Add documentation comment (copy from base if available)",
                                cancellationToken => AddDocumentationCommentRefactoring.RefactorAsync(context.Document, memberDeclaration, true, cancellationToken),
                                GetEquivalenceKey(diagnostic, "CopyFromBaseIfAvailable"));

                            context.RegisterCodeFix(codeAction2, diagnostic);
                            break;
                        }
                    case CompilerDiagnosticIdentifiers.MethodReturnTypeMustMatchOverriddenMethodReturnType:
                        {
                            if (!Settings.IsCodeFixEnabled(CodeFixIdentifiers.ChangeMethodReturnType))
                                break;

                            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                            var methodSymbol = (IMethodSymbol)semanticModel.GetDeclaredSymbol(memberDeclaration, context.CancellationToken);

                            ITypeSymbol typeSymbol = methodSymbol.OverriddenMethod.ReturnType;

                            if (typeSymbol?.IsErrorType() == false)
                            {
                                TypeSyntax newType = typeSymbol.ToMinimalTypeSyntax(semanticModel, memberDeclaration.SpanStart);

                                CodeAction codeAction = CodeAction.Create(
                                    $"Change return type to '{SymbolDisplay.GetMinimalString(typeSymbol, semanticModel, memberDeclaration.SpanStart)}'",
                                    cancellationToken => MemberTypeMustMatchOverriddenMemberTypeRefactoring.RefactorAsync(context.Document, memberDeclaration, typeSymbol, semanticModel, cancellationToken),
                                    GetEquivalenceKey(diagnostic));

                                context.RegisterCodeFix(codeAction, diagnostic);
                            }

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.PartialMethodsMustHaveVoidReturnType:
                        {
                            if (!Settings.IsCodeFixEnabled(CodeFixIdentifiers.ChangeMethodReturnType))
                                break;

                            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                            var methodDeclaration = (MethodDeclarationSyntax)memberDeclaration;

                            MethodDeclarationSyntax otherPart = semanticModel.GetOtherPart(methodDeclaration, context.CancellationToken);

                            if (otherPart == null)
                                break;

                            CodeAction codeAction = CodeAction.Create(
                                "Change return type to 'void'",
                                cancellationToken =>
                                {
                                    return context.Document.Solution().ReplaceNodesAsync(
                                        new MethodDeclarationSyntax[] { methodDeclaration, otherPart },
                                        (node, rewrittenNode) => node.WithReturnType(CSharpFactory.VoidType().WithTriviaFrom(node.ReturnType)),
                                        cancellationToken);
                                },
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case CompilerDiagnosticIdentifiers.MemberTypeMustMatchOverriddenMemberType:
                        {
                            if (!Settings.IsCodeFixEnabled(CodeFixIdentifiers.MemberTypeMustMatchOverriddenMemberType))
                                break;

                            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                            ITypeSymbol typeSymbol = null;

                            switch (memberDeclaration.Kind())
                            {
                                case SyntaxKind.PropertyDeclaration:
                                case SyntaxKind.IndexerDeclaration:
                                    {
                                        var propertySymbol = (IPropertySymbol)semanticModel.GetDeclaredSymbol(memberDeclaration, context.CancellationToken);

                                        typeSymbol = propertySymbol.OverriddenProperty.Type;
                                        break;
                                    }
                                case SyntaxKind.EventDeclaration:
                                    {
                                        var eventSymbol = (IEventSymbol)semanticModel.GetDeclaredSymbol(memberDeclaration, context.CancellationToken);

                                        typeSymbol = eventSymbol.OverriddenEvent.Type;
                                        break;
                                    }
                                case SyntaxKind.EventFieldDeclaration:
                                    {
                                        VariableDeclaratorSyntax declarator = ((EventFieldDeclarationSyntax)memberDeclaration).Declaration.Variables.First();

                                        var eventSymbol = (IEventSymbol)semanticModel.GetDeclaredSymbol(declarator, context.CancellationToken);

                                        typeSymbol = eventSymbol.OverriddenEvent.Type;
                                        break;
                                    }
                            }

                            if (typeSymbol?.IsErrorType() == false)
                            {
                                string title = $"Change type to '{SymbolDisplay.GetMinimalString(typeSymbol, semanticModel, memberDeclaration.SpanStart)}'";

                                CodeAction codeAction = CodeAction.Create(
                                    title,
                                    cancellationToken => MemberTypeMustMatchOverriddenMemberTypeRefactoring.RefactorAsync(context.Document, memberDeclaration, typeSymbol, semanticModel, cancellationToken),
                                    GetEquivalenceKey(diagnostic));

                                context.RegisterCodeFix(codeAction, diagnostic);
                            }

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.MissingPartialModifier:
                    case CompilerDiagnosticIdentifiers.PartialMethodMustBeDeclaredWithinPartialClassOrPartialStruct:
                        {
                            if (!Settings.IsCodeFixEnabled(CodeFixIdentifiers.AddPartialModifier))
                                break;

                            CodeAction codeAction = CodeAction.Create(
                                "Add 'partial' modifier",
                                cancellationToken =>
                                {
                                    if (memberDeclaration.IsKind(SyntaxKind.MethodDeclaration)
                                        && memberDeclaration.IsParentKind(SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration))
                                    {
                                        return context.Document.InsertModifierAsync(memberDeclaration.Parent, SyntaxKind.PartialKeyword, ModifierComparer.Instance, cancellationToken);
                                    }
                                    else if (memberDeclaration.IsKind(SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration, SyntaxKind.InterfaceDeclaration))
                                    {
                                        return context.Document.InsertModifierAsync(memberDeclaration, SyntaxKind.PartialKeyword, ModifierComparer.Instance, cancellationToken);
                                    }

                                    return Task.FromResult(context.Document);
                                },
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case CompilerDiagnosticIdentifiers.MemberIsAbstractButItIsContainedInNonAbstractClass:
                        {
                            if (!Settings.IsCodeFixEnabled(CodeFixIdentifiers.MakeContainingClassAbstract))
                                break;

                            if (!memberDeclaration.IsParentKind(SyntaxKind.ClassDeclaration))
                                break;

                            CodeAction codeAction = CodeAction.Create(
                                "Make containing class abstract",
                                cancellationToken => context.Document.InsertModifierAsync(memberDeclaration.Parent, SyntaxKind.AbstractKeyword, ModifierComparer.Instance, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case CompilerDiagnosticIdentifiers.ObjectReferenceIsRequiredForNonStaticMember:
                        {
                            if (!Settings.IsCodeFixEnabled(CodeFixIdentifiers.MakeMemberNonStatic))
                                break;

                            SyntaxTokenList modifiers = memberDeclaration.GetModifiers();

                            Debug.Assert(modifiers.Contains(SyntaxKind.StaticKeyword), memberDeclaration.ToString());

                            if (!modifiers.Contains(SyntaxKind.StaticKeyword))
                                break;

                            CodeAction codeAction = CodeAction.Create(
                                $"Make containing {memberDeclaration.GetTitle()} non-static",
                                cancellationToken => context.Document.RemoveModifierAsync(memberDeclaration, SyntaxKind.StaticKeyword, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case CompilerDiagnosticIdentifiers.StaticConstructorMustBeParameterless:
                        {
                            if (!Settings.IsCodeFixEnabled(CodeFixIdentifiers.RemoveParametersFromStaticConstructor))
                                break;

                            var constructorDeclaration = (ConstructorDeclarationSyntax)memberDeclaration;

                            CodeAction codeAction = CodeAction.Create(
                                "Remove parameters",
                                cancellationToken =>
                                {
                                    ParameterListSyntax parameterList = constructorDeclaration.ParameterList;

                                    ParameterListSyntax newParameterList = parameterList
                                        .WithParameters(default(SeparatedSyntaxList<ParameterSyntax>))
                                        .WithOpenParenToken(parameterList.OpenParenToken.WithoutTrailingTrivia())
                                        .WithCloseParenToken(parameterList.CloseParenToken.WithoutLeadingTrivia());

                                    ConstructorDeclarationSyntax newNode = constructorDeclaration.WithParameterList(newParameterList);

                                    return context.Document.ReplaceNodeAsync(constructorDeclaration, newNode, cancellationToken);
                                },
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                }
            }
        }
    }
}
