// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp
{
    public class StatementsSelection : SyntaxListSelection<StatementSyntax>
    {
        private StatementsSelection(StatementsInfo info, TextSpan span)
             : base(info.Statements, span)
        {
            Info = info;
        }

        private StatementsSelection(StatementsInfo info, TextSpan span, int startIndex, int endIndex)
             : base(info.Statements, span, startIndex, endIndex)
        {
            Info = info;
        }

        public static StatementsSelection Create(BlockSyntax block, TextSpan span)
        {
            if (block == null)
                throw new ArgumentNullException(nameof(block));

            var info = new StatementsInfo(block);

            return new StatementsSelection(info, span);
        }

        public static StatementsSelection Create(SwitchSectionSyntax switchSection, TextSpan span)
        {
            if (switchSection == null)
                throw new ArgumentNullException(nameof(switchSection));

            var info = new StatementsInfo(switchSection);

            return new StatementsSelection(info, span);
        }

        public static bool TryCreate(BlockSyntax block, TextSpan span, out StatementsSelection selectedStatements)
        {
            StatementsInfo info = SyntaxInfo.StatementsInfo(block);

            if (!info.Success)
            {
                selectedStatements = null;
                return false;
            }

            return TryCreate(info, span, out selectedStatements);
        }

        public static bool TryCreate(SwitchSectionSyntax switchSection, TextSpan span, out StatementsSelection selectedStatements)
        {
            StatementsInfo info = SyntaxInfo.StatementsInfo(switchSection);

            if (!info.Success)
            {
                selectedStatements = null;
                return false;
            }

            return TryCreate(info, span, out selectedStatements);
        }

        public static bool TryCreate(StatementsInfo statementsInfo, TextSpan span, out StatementsSelection selectedStatements)
        {
            if (statementsInfo.Statements.Any())
            {
                IndexPair indexes = GetIndexes(statementsInfo.Statements, span);

                if (indexes.StartIndex != -1)
                {
                    selectedStatements = new StatementsSelection(statementsInfo, span, indexes.StartIndex, indexes.EndIndex);
                    return true;
                }
            }

            selectedStatements = null;
            return false;
        }

        //TODO: 
        public StatementsInfo Info { get; }
    }
}
