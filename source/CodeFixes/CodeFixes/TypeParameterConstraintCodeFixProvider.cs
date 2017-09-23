// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TypeParameterConstraintCodeFixProvider))]
    [Shared]
    public class TypeParameterConstraintCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    CompilerDiagnosticIdentifiers.NewConstraintMustBeLastConstraintSpecified,
                    CompilerDiagnosticIdentifiers.DuplicateConstraintForTypeParameter,
                    CompilerDiagnosticIdentifiers.ClassOrStructConstraintMustComeBeforeAnyOtherConstraints,
                    CompilerDiagnosticIdentifiers.CannotSpecifyBothConstraintClassAndClassOrStructConstraint,
                    CompilerDiagnosticIdentifiers.NewConstraintCannotBeUsedWithStructConstraint);
            }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            if (!Settings.IsAnyCodeFixEnabled(
                CodeFixIdentifiers.RemoveConstraint,
                CodeFixIdentifiers.MoveConstraint))
            {
                return;
            }

            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out TypeParameterConstraintSyntax constraint))
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case CompilerDiagnosticIdentifiers.NewConstraintMustBeLastConstraintSpecified:
                        {
                            if (!Settings.IsCodeFixEnabled(CodeFixIdentifiers.MoveConstraint))
                                break;

                            TypeParameterConstraintInfo info = SyntaxInfo.TypeParameterConstraintInfo(constraint);

                            if (!info.Success)
                                break;

                            MoveConstraint(context, diagnostic, info, info.Constraints.Count - 1);

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.DuplicateConstraintForTypeParameter:
                        {
                            if (Settings.IsCodeFixEnabled(CodeFixIdentifiers.RemoveConstraint))
                                RemoveConstraint(context, diagnostic, constraint);

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.ClassOrStructConstraintMustComeBeforeAnyOtherConstraints:
                        {
                            if (!Settings.IsCodeFixEnabled(CodeFixIdentifiers.MoveConstraint))
                                break;

                            TypeParameterConstraintInfo info = SyntaxInfo.TypeParameterConstraintInfo(constraint);

                            if (!info.Success)
                                break;

                            if (info.IsDuplicateConstraint)
                            {
                                RemoveConstraint(context, diagnostic, constraint);
                            }
                            else
                            {
                                MoveConstraint(context, diagnostic, info, 0);
                            }

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.CannotSpecifyBothConstraintClassAndClassOrStructConstraint:
                        {
                            if (!Settings.IsCodeFixEnabled(CodeFixIdentifiers.RemoveConstraint))
                                break;

                            TypeParameterConstraintInfo info = SyntaxInfo.TypeParameterConstraintInfo(constraint);

                            if (!info.Success)
                                break;

                            RemoveConstraint(context, diagnostic, constraint);

                            TypeParameterConstraintSyntax classConstraint = info.Constraints.Find(SyntaxKind.ClassConstraint);

                            if (classConstraint != null)
                                RemoveConstraint(context, diagnostic, classConstraint);

                            TypeParameterConstraintSyntax structConstraint = info.Constraints.Find(SyntaxKind.StructConstraint);

                            if (structConstraint != null)
                                RemoveConstraint(context, diagnostic, structConstraint);

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.NewConstraintCannotBeUsedWithStructConstraint:
                        {
                            if (!Settings.IsCodeFixEnabled(CodeFixIdentifiers.RemoveConstraint))
                                break;

                            RemoveConstraint(context, diagnostic, constraint);

                            TypeParameterConstraintInfo info = SyntaxInfo.TypeParameterConstraintInfo(constraint);

                            if (!info.Success)
                                break;

                            TypeParameterConstraintSyntax structConstraint = info.Constraints.Find(SyntaxKind.StructConstraint);

                            RemoveConstraint(context, diagnostic, structConstraint);
                            break;
                        }
                }
            }
        }

        private void MoveConstraint(
            CodeFixContext context,
            Diagnostic diagnostic,
            TypeParameterConstraintInfo info,
            int index)
        {
            CodeAction codeAction = CodeAction.Create(
                $"Move constraint '{info.Constraint}'",
                cancellationToken =>
                {
                    SeparatedSyntaxList<TypeParameterConstraintSyntax> newConstraints = info.Constraints
                        .Remove(info.Constraint)
                        .Insert(index, info.Constraint);

                    TypeParameterConstraintClauseSyntax newNode = info.ConstraintClause
                        .WithConstraints(newConstraints)
                        .WithFormatterAnnotation();

                    return context.Document.ReplaceNodeAsync(info.ConstraintClause, newNode, cancellationToken);
                },
                GetEquivalenceKey(diagnostic));

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        private void RemoveConstraint(
            CodeFixContext context,
            Diagnostic diagnostic,
            TypeParameterConstraintSyntax constraint)
        {
            CodeAction codeAction = CodeAction.Create(
                $"Remove constraint '{constraint}'",
                cancellationToken => context.Document.RemoveNodeAsync(constraint, RemoveHelper.GetRemoveOptions(constraint), cancellationToken),
                GetEquivalenceKey(diagnostic, constraint.Kind().ToString()));

            context.RegisterCodeFix(codeAction, diagnostic);
        }
    }
}
