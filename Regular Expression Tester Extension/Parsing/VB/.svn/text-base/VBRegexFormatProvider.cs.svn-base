using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RegexTester.Parsing.CSharp;
using Microsoft.VisualBasic;
using System.Globalization;

namespace RegexTester.Parsing.VB
{
    public class VBRegexFormatProvider : BaseRegexFormatProvider
    {
        private static Regex syntaxRegex;

        private string GetStringFromGroup(Group matchGroup, VBRegexFindResults results)
        {
            if (matchGroup.Success)
            {
                StringBuilder result = new StringBuilder();
                foreach (Capture capture in matchGroup.Captures)
                {
                    string value = capture.Value;
                    result.Append(results.EditToDisplayString(value.Substring(1, value.Length - 2)));
                }
                return result.ToString();
            }
            return null;
        }

        public override IRegexFindResults GetRegexFindResults(Match match)
        {
            VBRegexFindResults results = new VBRegexFindResults();

            results.FoundMatch = match.Success;
            if (results.FoundMatch)
            {
                results.ToMatch = GetStringFromGroup(match.Groups["tomatchstring"], results);
                if (match.Groups["tomatchnotstring"].Success)
                {
                    results.ToMatchExpression = match.Groups["tomatchnotstring"].Value;
                }
                results.ReplacePattern = GetStringFromGroup(match.Groups["replace"], results);
                if (match.Groups["onlyregex"].Success)
                {
                    results.IsMatchOnlyRegex = true;
                    results.Regex = GetStringFromGroup(match.Groups["onlyregex"], results);
                }
                else
                {
                    results.Regex = GetStringFromGroup(match.Groups["regex"], results);
                }
                results.RegexNamespace = match.Groups["regexnamespace"].Success ? match.Groups["regexnamespace"].Captures[0].Value : null;
                RegexOptions options = RegexOptions.None;
                foreach (Capture capture in match.Groups["option"].Captures)
                {
                    options |= (RegexOptions)Enum.Parse(typeof(RegexOptions), capture.Value, false);
                }
                results.RegexOptions = options;
            }
            results.IsEdit = false;
            results.CodeDomProvider = new VBCodeProvider();

            return results;
        }

        protected override string GetStringMatchRegex(string matchName)
        {
            string oneString = string.Format(CultureInfo.InvariantCulture,
                @"?<{0}>""(?("")""|.*?[^""]""(?!""))", matchName);
            string concatenatedStrings = string.Format(CultureInfo.InvariantCulture, @"(({0})\s*\&\s*_?\s*)*({0})", oneString);
            return concatenatedStrings;
        }

        protected override string GetOptionsMatchRegex(string optionsName, string namespaceName, string optionName)
        {
            string result = String.Format(CultureInfo.InvariantCulture, @"(?<{0}>" +
                @"(\s*(Or)?\s*(?<{1}>System\s*\.\s*Text\s*\.\s*RegularExpressions\s*\.\s*|" +
                @"Text\s*.\s*RegularExpressions\s*\.\s*|" +
                @"RegularExpressions\s*\.\s*)?" +
                @"RegexOptions\s*\.\s*(?<{2}>[a-zA-Z]+))+)", optionsName, namespaceName, optionName);
            return result;
        }

        protected override Regex SyntaxRegex
        {
            get { return syntaxRegex; }
            set { syntaxRegex = value; }
        }
    }
}
