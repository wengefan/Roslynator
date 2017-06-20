// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SimpleNameCodeFixProvider))]
    [Shared]
    public class SimpleNameCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(CompilerDiagnosticIdentifiers.TypeDoesNotContainDefinitionAndNoExtensionMethodCouldBeFound); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            if (!Settings.IsCodeFixEnabled(CodeFixIdentifiers.FixMemberAccessName))
                return;

            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            SimpleNameSyntax simpleName = root
                .FindNode(context.Span, getInnermostNodeForTie: true)?
                .FirstAncestorOrSelf<SimpleNameSyntax>();

            Debug.Assert(simpleName != null, $"{nameof(simpleName)} is null");

            if (simpleName == null)
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case CompilerDiagnosticIdentifiers.TypeDoesNotContainDefinitionAndNoExtensionMethodCouldBeFound:
                        {
                            if (!simpleName.IsParentKind(SyntaxKind.SimpleMemberAccessExpression))
                                break;

                            var memberAccess = (MemberAccessExpressionSyntax)simpleName.Parent;

                            if (memberAccess.IsParentKind(SyntaxKind.InvocationExpression))
                                break;

                            switch (simpleName.Identifier.ValueText)
                            {
                                case "Count":
                                    {
                                        SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                                        ComputeCodeFix(context, diagnostic, memberAccess, semanticModel, "Count", "Length");
                                        break;
                                    }
                                case "Length":
                                    {
                                        SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                                        ComputeCodeFix(context, diagnostic, memberAccess, semanticModel, "Length", "Count");
                                        break;
                                    }
                            }

                            break;
                        }
                }
            }
        }

        private static void ComputeCodeFix(
            CodeFixContext context,
            Diagnostic diagnostic,
            MemberAccessExpressionSyntax memberAccess,
            SemanticModel semanticModel,
            string name,
            string newName)
        {
            if (IsFixable(memberAccess, newName, semanticModel, context.CancellationToken))
            {
                CodeAction codeAction = CodeAction.Create(
                    $"Use '{newName}' instead of '{name}'",
                    cancellationToken => RefactorAsync(context.Document, memberAccess, newName, cancellationToken),
                    diagnostic.Id + EquivalenceKeySuffix);

                context.RegisterCodeFix(codeAction, diagnostic);
            }
        }

        private static bool IsFixable(
            MemberAccessExpressionSyntax memberAccess,
            string newName,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(memberAccess.Expression, cancellationToken);

            if (typeSymbol != null)
            {
                if (typeSymbol.IsArrayType())
                    typeSymbol = ((IArrayTypeSymbol)typeSymbol).ElementType;

                foreach (ISymbol symbol in typeSymbol.GetMembers(newName))
                {
                    if (!symbol.IsStatic
                        && symbol.IsProperty())
                    {
                        var propertySymbol = (IPropertySymbol)symbol;

                        if (!propertySymbol.IsIndexer
                            && propertySymbol.IsReadOnly
                            && semanticModel.IsAccessible(memberAccess.SpanStart, symbol))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static Task<Document> RefactorAsync(
            Document document,
            MemberAccessExpressionSyntax memberAccess,
            string newName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            MemberAccessExpressionSyntax newNode = memberAccess
                .WithName(SyntaxFactory.IdentifierName(newName))
                .WithTriviaFrom(memberAccess.Name);

            return document.ReplaceNodeAsync(memberAccess, newNode, cancellationToken);
        }
    }
}
