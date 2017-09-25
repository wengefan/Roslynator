// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;

namespace Roslynator
{
    internal static class AccessibilityExtensions
    {
        public static bool IsMoreRestrictiveThan(this Accessibility accessibility, Accessibility value)
        {
            switch (value)
            {
                case Accessibility.Public:
                    {
                        return accessibility == Accessibility.Internal
                            || accessibility == Accessibility.ProtectedOrInternal
                            || accessibility == Accessibility.Protected
                            || accessibility == Accessibility.Private;
                    }
                case Accessibility.Internal:
                    {
                        return accessibility == Accessibility.Private;
                    }
                case Accessibility.ProtectedOrInternal:
                    {
                        return accessibility == Accessibility.Internal
                            || accessibility == Accessibility.Protected
                            || accessibility == Accessibility.Private;
                    }
                case Accessibility.Protected:
                    {
                        return accessibility == Accessibility.Private;
                    }
                case Accessibility.Private:
                    {
                        return false;
                    }
            }

            return false;
        }

        public static string GetTitle(this Accessibility accessibility)
        {
            switch (accessibility)
            {
                case Accessibility.Private:
                    return "private";
                case Accessibility.Protected:
                    return "protected";
                case Accessibility.Internal:
                    return "internal";
                case Accessibility.ProtectedOrInternal:
                    return "protected internal";
                case Accessibility.Public:
                    return "public";
            }

            throw new ArgumentException("", nameof(accessibility));
        }
    }
}
