using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using Microsoft.CSharp;
using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;
using RegexTester.Parsing;
using RegexTester.Parsing.CSharp;

namespace RegexTester
{
    public class RegexFinder
    {
        private IRegexFormatProvider formatProvider;

        public RegexFinder(IRegexFormatProvider formatProvider)
        {
            this.formatProvider = formatProvider;
        }

        public RegexFinder()
            : this(new CSharpRegexFormatProvider())
        {
        }

        public IRegexFindResults FindRegex(string text)
        {
            Regex matchSyntaxRegex = formatProvider.GetSyntaxRegex();
            Match foundMatch = matchSyntaxRegex.Match(text);

            return formatProvider.GetRegexFindResults(foundMatch);
        }
    }

    public interface IRegexFindResults
    {
        bool FoundMatch { get; }
        bool IsMatchOnlyRegex { get; }
        string ToMatch { get; set; }
        string ToMatchExpression { get; }
        string ReplacePattern { get; set; }
        string Regex { get; set; }
        RegexOptions RegexOptions { get; set; }
        bool IsEdit { get; }
        int Index { get; }
        int Length { get; }
        void ConvertToEdit();
        void ConvertToDisplay();
        string GetRegexString();
    }
}
