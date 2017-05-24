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
            get { return ImmutableArray.Create(CSharpErrorCodes.CannotImplicitlyConvertTypeExplicitConversionExists); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            if (!Settings.IsAnyCodeFixEnabled(
                CodeFixIdentifiers.AddCastExpression,
                CodeFixIdentifiers.CreateSingletonArray))
            {
                return;
            }

            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            ExpressionSyntax expression = root
                .FindNode(context.Span, getInnermostNodeForTie: true)?
                .FirstAncestorOrSelf<ExpressionSyntax>();

            Debug.Assert(expression != null, $"{nameof(expression)} is null");

            if (expression == null)
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case CSharpErrorCodes.CannotImplicitlyConvertTypeExplicitConversionExists:
                        {
                            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                            TypeInfo typeInfo = semanticModel.GetTypeInfo(expression, context.CancellationToken);

                            ITypeSymbol type = typeInfo.Type;
                            ITypeSymbol convertedType = typeInfo.ConvertedType;

                            if (type?.IsErrorType() == false
                                && !type.Equals(convertedType))
                            {
                                if (Settings.IsCodeFixEnabled(CodeFixIdentifiers.CreateSingletonArray)
                                    && convertedType.IsArrayType())
                                {
                                    var arrayType = (IArrayTypeSymbol)convertedType;

                                    if (semanticModel.IsImplicitConversion(expression, arrayType.ElementType))
                                    {
                                        CodeAction codeAction = CodeAction.Create(
                                            "Create singleton array",
                                            cancellationToken => CreateSingletonArrayRefactoring.RefactorAsync(context.Document, expression, arrayType.ElementType, semanticModel, cancellationToken),
                                            CodeFixIdentifiers.CreateSingletonArray + EquivalenceKeySuffix);

                                        context.RegisterCodeFix(codeAction, diagnostic);
                                    }
                                }

                                if (Settings.IsCodeFixEnabled(CodeFixIdentifiers.AddCastExpression)
                                    && semanticModel.IsExplicitConversion(expression, convertedType))
                                {
                                    CodeAction codeAction = CodeAction.Create(
                                        $"Cast to '{SymbolDisplay.GetMinimalString(convertedType, semanticModel, expression.SpanStart)}' ",
                                        cancellationToken => AddCastExpressionRefactoring.RefactorAsync(context.Document, expression, convertedType, semanticModel, cancellationToken),
                                        CodeFixIdentifiers.AddCastExpression + EquivalenceKeySuffix);

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
