// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Roslynator.Metadata
{
    public class CodeFixDescriptor
    {
        public CodeFixDescriptor(
            string id,
            string identifier,
            string title,
            bool isEnabledByDefault)
        {
            Id = id;
            Identifier = identifier;
            Title = title;
            IsEnabledByDefault = isEnabledByDefault;
        }

        public static IEnumerable<CodeFixDescriptor> LoadFromFile(string filePath)
        {
            XDocument doc = XDocument.Load(filePath);

            foreach (XElement element in doc.Root.Elements())
            {
                yield return new CodeFixDescriptor(
                    element.Attribute("Id")?.Value,
                    element.Attribute("Identifier").Value,
                    element.Attribute("Title").Value,
                    (element.Attribute("IsEnabledByDefault") != null)
                        ? bool.Parse(element.Attribute("IsEnabledByDefault").Value)
                        : true);
            }
        }

        public string Id { get; }

        public string Identifier { get; }

        public string Title { get; }

        public bool IsEnabledByDefault { get; }

        public string GetGitHubHref()
        {
            string s = Title.TrimEnd('.').ToLowerInvariant();

            s = Regex.Replace(s, @"[^a-zA-Z0-9\ \-]", "");
            s = Regex.Replace(s, @"\ ", "-");

            return s;
        }
    }
}
