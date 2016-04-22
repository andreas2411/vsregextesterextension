using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace RegexTester.Parsing
{
    public interface IRegexFormatProvider
    {
        Regex GetSyntaxRegex();
        IRegexFindResults GetRegexFindResults(Match match);
    }

    public abstract class BaseRegexFormatProvider : IRegexFormatProvider
    {
        public Regex GetSyntaxRegex()
        {
            if (SyntaxRegex == null)
            {
                string regexMatch = string.Format(CultureInfo.InvariantCulture,
                    @"^(?(""|@)({3})|\(\s*((?(""|@)({4})|(?<tomatchnotstring>.*?))\s*,\s*)?({0})\s*(,\s*({1})\s*)?(,\s*{2}\s*)?\))$",
                    GetStringMatchRegex("regex"), GetStringMatchRegex("replace"),
                    GetOptionsMatchRegex("options", "regexnamespace", "option"), GetStringMatchRegex("onlyregex"),
                    GetStringMatchRegex("tomatchstring"));
                SyntaxRegex = new Regex(regexMatch, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }
            return SyntaxRegex;
        }

        protected abstract Regex SyntaxRegex { get; set; }
        public abstract IRegexFindResults GetRegexFindResults(Match match);
        protected abstract string GetStringMatchRegex(string matchName);
        protected abstract string GetOptionsMatchRegex(string optionsName, string namespaceName, string optionName);
    }
}
