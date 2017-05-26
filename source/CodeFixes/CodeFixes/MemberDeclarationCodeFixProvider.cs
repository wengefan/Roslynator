// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
                    CSharpErrorCodes.CannotChangeAccessModifiersWhenOverridingInheritedMember,
                    CSharpErrorCodes.MissingXmlCommentForPubliclyVisibleTypeOrMember,
                    CSharpErrorCodes.MemberReturnTypeMustMatchOverriddenMemberReturnType,
                    CSharpErrorCodes.MemberTypeMustMatchOverriddenMemberType);
            }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            if (!Settings.IsAnyCodeFixEnabled(
                CodeFixIdentifiers.OverridingMemberCannotChangeAccessModifiers,
                CodeFixIdentifiers.AddDocumentationComment,
                CodeFixIdentifiers.MemberReturnTypeMustMatchOverriddenMemberReturnType,
                CodeFixIdentifiers.MemberTypeMustMatchOverriddenMemberType))
            {
                return;
            }

            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            MemberDeclarationSyntax memberDeclaration = root
                .FindNode(context.Span, getInnermostNodeForTie: true)?
                .FirstAncestorOrSelf<MemberDeclarationSyntax>();

            Debug.Assert(memberDeclaration != null, $"{nameof(memberDeclaration)} is null");

            if (memberDeclaration == null)
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case CSharpErrorCodes.CannotChangeAccessModifiersWhenOverridingInheritedMember:
                        {
                            if (!Settings.IsCodeFixEnabled(CodeFixIdentifiers.OverridingMemberCannotChangeAccessModifiers))
                                break;

                            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                            OverrideInfo overrideInfo = OverridingMemberCannotChangeAccessModifiersRefactoring.GetOverrideInfo(memberDeclaration, semanticModel, context.CancellationToken);

                            string title = $"Change accessibility to '{overrideInfo.DeclaredAccessibilityText}'";

                            CodeAction codeAction = CodeAction.Create(
                                title,
                                cancellationToken => OverridingMemberCannotChangeAccessModifiersRefactoring.RefactorAsync(context.Document, memberDeclaration, overrideInfo, cancellationToken),
                                diagnostic.Id + EquivalenceKeySuffix);

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case CSharpErrorCodes.MissingXmlCommentForPubliclyVisibleTypeOrMember:
                        {
                            if (!Settings.IsCodeFixEnabled(CodeFixIdentifiers.AddDocumentationComment))
                                break;

                            CodeAction codeAction = CodeAction.Create(
                           "Add documentation comment",
                           cancellationToken => AddDocumentationCommentRefactoring.RefactorAsync(context.Document, memberDeclaration, false, cancellationToken),
                           diagnostic.Id + EquivalenceKeySuffix);

                            context.RegisterCodeFix(codeAction, diagnostic);

                            CodeAction codeAction2 = CodeAction.Create(
                                "Add documentation comment (copy from base if available)",
                                cancellationToken => AddDocumentationCommentRefactoring.RefactorAsync(context.Document, memberDeclaration, true, cancellationToken),
                                diagnostic.Id + "CopyFromBaseIfAvailable" + EquivalenceKeySuffix);

                            context.RegisterCodeFix(codeAction2, diagnostic);
                            break;
                        }
                    case CSharpErrorCodes.MemberReturnTypeMustMatchOverriddenMemberReturnType:
                        {
                            if (!Settings.IsCodeFixEnabled(CodeFixIdentifiers.MemberReturnTypeMustMatchOverriddenMemberReturnType))
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
                                    diagnostic.Id + EquivalenceKeySuffix);

                                context.RegisterCodeFix(codeAction, diagnostic);
                            }

                            break;
                        }
                    case CSharpErrorCodes.MemberTypeMustMatchOverriddenMemberType:
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
                                    diagnostic.Id + EquivalenceKeySuffix);

                                context.RegisterCodeFix(codeAction, diagnostic);
                            }

                            break;
                        }
                }
            }
        }
    }
}
