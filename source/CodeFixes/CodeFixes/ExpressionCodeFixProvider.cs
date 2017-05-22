// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ExpressionCodeFixProvider))]
    [Shared]
    public class ExpressionCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(CodeFixIdentifiers.CannotImplicitlyConvertType); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            ExpressionSyntax expression = root
                .FindNode(context.Span, getInnermostNodeForTie: true)?
                .FirstAncestorOrSelf<ExpressionSyntax>();

            Debug.Assert(expression != null, $"{nameof(expression)} is null");

            if (expression == null)
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                if (IsCodeFixEnabled(diagnostic.Id))
                {
                    switch (diagnostic.Id)
                    {
                        case CodeFixIdentifiers.CannotImplicitlyConvertType:
                            {
                                SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                                TypeInfo typeInfo = semanticModel.GetTypeInfo(expression, context.CancellationToken);

                                ITypeSymbol type = typeInfo.Type;
                                ITypeSymbol convertedType = typeInfo.ConvertedType;

                                if (type?.IsErrorType() == false
                                    && !type.Equals(convertedType))
                                {
                                    if (convertedType.IsArrayType())
                                    {
                                        var arrayType = (IArrayTypeSymbol)convertedType;

                                        if (semanticModel.IsImplicitConversion(expression, arrayType.ElementType))
                                        {
                                            CodeAction codeAction = CodeAction.Create(
                                                "Create singleton array",
                                                cancellationToken => CreateSingletonArrayRefactoring.RefactorAsync(context.Document, expression, arrayType.ElementType, semanticModel, cancellationToken),
                                                diagnostic.Id + EquivalenceKeySuffix);

                                            context.RegisterCodeFix(codeAction, diagnostic);
                                        }
                                    }

                                    if (semanticModel.IsExplicitConversion(expression, convertedType))
                                    {
                                        CodeAction codeAction = CodeAction.Create(
                                            $"Cast to '{SymbolDisplay.GetMinimalString(convertedType, semanticModel, expression.SpanStart)}' ",
                                            cancellationToken => AddCastExpressionRefactoring.RefactorAsync(context.Document, expression, convertedType, semanticModel, cancellationToken),
                                            diagnostic.Id + EquivalenceKeySuffix);

                                        context.RegisterCodeFix(codeAction, diagnostic);
                                    }
                                }

                                break;
                            }
                    }
                }
            }
        }
    }
}
