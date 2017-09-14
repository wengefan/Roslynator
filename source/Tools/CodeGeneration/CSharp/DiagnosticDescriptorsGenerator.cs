// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp;
using Roslynator.Metadata;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CodeGeneration.CSharp
{
    public static class DiagnosticDescriptorsGenerator
    {
        public static CompilationUnitSyntax Generate(IEnumerable<AnalyzerDescriptor> analyzers, IComparer<string> comparer)
        {
            return CompilationUnit(
                UsingDirectives("System", "Microsoft.CodeAnalysis"),
                NamespaceDeclaration("Roslynator.CSharp",
                    ClassDeclaration(
                        Modifiers.PublicStatic(),
                        "DiagnosticDescriptors",
                        List(CreateMembers(analyzers.OrderBy(f => f.Id, comparer)).Select(f => f.WithTrailingTrivia(NewLine()))))));
        }

        private static IEnumerable<MemberDeclarationSyntax> CreateMembers(IEnumerable<AnalyzerDescriptor> analyzers)
        {
            yield return FieldDeclaration(
                Modifiers.PrivateConst(),
                StringType(),
                "HelpLinkUriRoot",
                StringLiteralExpression("http://pihrt.net/roslynator/analyzer?id="));

            foreach (AnalyzerDescriptor analyzer in analyzers)
            {
                yield return FieldDeclaration(
                    Modifiers.PublicStaticReadOnly(),
                    IdentifierName("DiagnosticDescriptor"),
                    analyzer.Identifier,
                    ObjectCreationExpression(
                        IdentifierName("DiagnosticDescriptor"),
                        ArgumentList(
                            Argument(
                                NameColon("id"),
                                SimpleMemberAccessExpression(IdentifierName("DiagnosticIdentifiers"), IdentifierName(analyzer.Identifier))),
                            Argument(
                                NameColon("title"),
                                StringLiteralExpression(analyzer.Title)),
                            Argument(
                                NameColon("messageFormat"),
                                StringLiteralExpression(analyzer.MessageFormat)),
                            Argument(
                                NameColon("category"),
                                SimpleMemberAccessExpression(IdentifierName("DiagnosticCategories"), IdentifierName(analyzer.Category))),
                            Argument(
                                NameColon("defaultSeverity"),
                                SimpleMemberAccessExpression(IdentifierName("DiagnosticSeverity"), IdentifierName(analyzer.DefaultSeverity))),
                            Argument(
                                NameColon("isEnabledByDefault"),
                                BooleanLiteralExpression(analyzer.IsEnabledByDefault)),
                            Argument(
                                NameColon("description"),
                                NullLiteralExpression()),
                            Argument(
                                NameColon("helpLinkUri"),
                                ParseExpression($"$\"{{HelpLinkUriRoot}}{{DiagnosticIdentifiers.{analyzer.Identifier}}}\"")),
                            Argument(
                                NameColon("customTags"),
                                (analyzer.SupportsFadeOut)
                                    ? SimpleMemberAccessExpression(IdentifierName("WellKnownDiagnosticTags"), IdentifierName("Unnecessary"))
                                    : ParseExpression("Array.Empty<string>()"))
                            )));

                if (analyzer.SupportsFadeOutAnalyzer)
                {
                    yield return FieldDeclaration(
                        Modifiers.PublicStaticReadOnly(),
                        IdentifierName("DiagnosticDescriptor"),
                        analyzer.Identifier + "FadeOut",
                        SimpleMemberInvocationExpression(
                            IdentifierName(analyzer.Identifier),
                            IdentifierName("CreateFadeOut")));
                }
            }
        }
    }
}
