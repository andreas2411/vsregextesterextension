using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CSharp;
using System.Globalization;
using System.CodeDom.Compiler;

namespace RegexTester.Parsing.CSharp
{
    public class CSharpRegexFormatProvider : BaseRegexFormatProvider
    {
        private static Regex syntaxRegex;

        private string GetStringFromGroup(Group matchGroup, CSharpRegexFindResults results)
        {
            if (matchGroup.Success)
            {
                StringBuilder result = new StringBuilder();
                foreach (Capture capture in matchGroup.Captures)
                {
                    string value = capture.Value;
                    if (value.StartsWith("@\""))
                    {
                        result.Append(results.EditToDisplayString(value.Substring(2, value.Length - 3), true));
                    }
                    else
                    {
                        result.Append(results.EditToDisplayString(value.Substring(1, value.Length - 2), false));
                    }
                }
                return result.ToString();
            }
            return null;
        }

        public override IRegexFindResults GetRegexFindResults(Match match)
        {
            CSharpRegexFindResults results = new CSharpRegexFindResults();

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
            results.CodeDomProvider = new CSharpCodeProvider();

            return results;
        }

        protected override string GetStringMatchRegex(string matchName)
        {
            /*string oneString = string.Format(CultureInfo.InvariantCulture,
                @"?<{0}>(?(@)@""(?("")""|.*?[^""]""(?!""))|""(?("")""|.*?[^\\]""))", matchName);*/
            string oneString = string.Format(CultureInfo.InvariantCulture,
                @"?<{0}>(?(@)@""(""""|[^""])*""|""(?("")""|.*?[^\\]""))", matchName);
            string concatenatedStrings = string.Format(CultureInfo.InvariantCulture, @"(({0})\s*\+\s*)*({0})", oneString);
            return concatenatedStrings;
        }

        protected override string GetOptionsMatchRegex(string optionsName, string namespaceName, string optionName)
        {
            string result = String.Format(CultureInfo.InvariantCulture, @"(?<{0}>" +
                @"(\s*\|?\s*(?<{1}>System\s*\.\s*Text\s*\.\s*RegularExpressions\s*\.\s*|" +
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
