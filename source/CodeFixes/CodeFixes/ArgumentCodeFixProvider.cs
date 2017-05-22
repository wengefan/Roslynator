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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ArgumentCodeFixProvider))]
    [Shared]
    public class ArgumentCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(CSharpErrorCodes.ArgumentMustBePassedWitOutKeyword); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            if (!Settings.IsCodeFixEnabled(CodeFixIdentifiers.AddOutModifierToArgument))
                return;

            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            ArgumentSyntax argument = root
                .FindNode(context.Span, getInnermostNodeForTie: true)?
                .FirstAncestorOrSelf<ArgumentSyntax>();

            Debug.Assert(argument != null, $"{nameof(argument)} is null");

            if (argument == null)
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case CSharpErrorCodes.ArgumentMustBePassedWitOutKeyword:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Add 'out' modifier",
                                cancellationToken =>
                                {
                                    ArgumentSyntax newArgument = argument
                                        .WithRefOrOutKeyword(CSharpFactory.OutKeyword())
                                        .WithFormatterAnnotation();

                                    return context.Document.ReplaceNodeAsync(argument, newArgument, context.CancellationToken);
                                },
                                diagnostic.Id + EquivalenceKeySuffix);

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                }
            }
        }
    }
}
