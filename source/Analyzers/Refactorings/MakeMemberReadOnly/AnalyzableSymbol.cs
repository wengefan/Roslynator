// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;

namespace Roslynator.CSharp.Refactorings.MakeMemberReadOnly
{
    internal struct AnalyzableSymbol : IEquatable<AnalyzableSymbol>
    {
        public AnalyzableSymbol(ISymbol symbol, bool isAutoProperty = false)
        {
            Symbol = symbol;
            IsAutoProperty = isAutoProperty;
        }

        public ISymbol Symbol { get; }
        public bool IsAutoProperty { get; }

        public override string ToString()
        {
            return Symbol?.ToString() ?? base.ToString();
        }

        public override bool Equals(object obj)
        {
            return obj is AnalyzableSymbol
                && Equals((AnalyzableSymbol)obj);
        }

        public bool Equals(AnalyzableSymbol other)
        {
            return Symbol == other.Symbol;
        }

        public override int GetHashCode()
        {
            return Symbol?.GetHashCode() ?? 0;
        }
    }
}
