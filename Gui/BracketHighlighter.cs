﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.Drawing;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Common;


namespace ICSharpCode.TextEditor
{
    public class Highlight
    {
        public TextLocation OpenBrace { get; set; }

        public TextLocation CloseBrace { get; set; }

        public Highlight(TextLocation openBrace, TextLocation closeBrace)
        {
            this.OpenBrace = openBrace;
            this.CloseBrace = closeBrace;
        }
    }

    public class BracketHighlightingSheme
    {
        public char OpenTag { get; set; }

        public char ClosingTag { get; set; }

        public BracketHighlightingSheme(char opentag, char closingtag)
        {
            this.OpenTag    = opentag;
            this.ClosingTag = closingtag;
        }

        public Highlight GetHighlight(Document.Document document, int offset)
        {
            int searchOffset;
            if (Shared.TEP.BracketMatchingStyle == BracketMatchingStyle.After)
            {
                searchOffset = offset;
            }
            else
            {
                searchOffset = offset + 1;
            }
            char word = document.GetCharAt(Math.Max(0, Math.Min(document.TextLength - 1, searchOffset)));

            TextLocation endP = document.OffsetToPosition(searchOffset);
            if (word == OpenTag)
            {
                if (searchOffset < document.TextLength)
                {
                    int bracketOffset = TextUtilities.SearchBracketForward(document, searchOffset + 1, OpenTag, ClosingTag);
                    if (bracketOffset >= 0)
                    {
                        TextLocation p = document.OffsetToPosition(bracketOffset);
                        return new Highlight(p, endP);
                    }
                }
            }
            else if (word == ClosingTag)
            {
                if (searchOffset > 0)
                {
                    int bracketOffset = TextUtilities.SearchBracketBackward(document, searchOffset - 1, OpenTag, ClosingTag);
                    if (bracketOffset >= 0)
                    {
                        TextLocation p = document.OffsetToPosition(bracketOffset);
                        return new Highlight(p, endP);
                    }
                }
            }
            return null;
        }
    }
}
