// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Xml.Linq;

namespace Roslynator.Metadata
{
    public class CompilerDiagnosticDescriptor
    {
        public CompilerDiagnosticDescriptor(
            string id,
            string helpUrl)
        {
            Id = id;
            HelpUrl = helpUrl;
        }

        public static IEnumerable<CompilerDiagnosticDescriptor> LoadFromFile(string filePath)
        {
            XDocument doc = XDocument.Load(filePath);

            foreach (XElement element in doc.Root.Elements("Diagnostic"))
            {
                yield return new CompilerDiagnosticDescriptor(
                    element.Attribute("Id").Value,
                    element.Attribute("HelpUrl").Value);
            }
        }

        public string Id { get; }

        public string HelpUrl { get; }
    }
}
