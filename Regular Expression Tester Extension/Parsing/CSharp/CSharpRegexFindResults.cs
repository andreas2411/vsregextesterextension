using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.CodeDom;
using System.Text.RegularExpressions;
using System.Globalization;
using System.CodeDom.Compiler;

namespace RegexTester.Parsing.CSharp
{
    public class CSharpRegexFindResults : IRegexFindResults
    {
        public bool FoundMatch { get; set; }
        public bool IsMatchOnlyRegex { get; set; }
        public string ToMatch { get; set; }
        public string ToMatchExpression { get; set; }
        public string ReplacePattern { get; set; }
        public string Regex { get; set; }
        public string RegexNamespace { get; set; }
        public RegexOptions RegexOptions { get; set; }
        public bool IsEdit { get; set; }
        public int Index { get; set; }
        public int Length { get; set; }
        public CodeDomProvider CodeDomProvider { get; set; }

        public void ConvertToEdit()
        {
            if (!IsEdit)
            {
                if (ToMatch != null)
                {
                    ToMatch = DisplayToEditString(ToMatch, true);
                }
                if (Regex != null)
                {
                    Regex = DisplayToEditString(Regex, true);
                }
                if (ReplacePattern != null)
                {
                    ReplacePattern = DisplayToEditString(ReplacePattern, true);
                }
                IsEdit = true;
            }
        }

        public void ConvertToDisplay()
        {
            if (IsEdit)
            {
                if (ToMatch != null)
                {
                    ToMatch = EditToDisplayString(ToMatch, true);
                }
                if (Regex != null)
                {
                    Regex = EditToDisplayString(Regex, true);
                }
                if (ReplacePattern != null)
                {
                    ReplacePattern = EditToDisplayString(ReplacePattern, true);
                }
                IsEdit = false;
            }
        }

        public string GetRegexString()
        {
            if (!IsEdit)
            {
                throw new InvalidOperationException("Cannot convert while not in edit mode");
            }
            StringBuilder result = new StringBuilder();
            if (IsMatchOnlyRegex)
            {
                result.Append("@\"");
                result.Append(Regex);
                result.Append("\"");
            }
            else
            {
                if (!FoundMatch)
                {
                    if (ReplacePattern == null)
                    {
                        result.Append("new Regex");
                    }
                    else
                    {
                        result.Append("Regex.Replace");
                    }
                }
                result.Append("(");
                result.Append(ToMatch != null ? string.Format(CultureInfo.InvariantCulture, "@\"{0}\", ", ToMatch) : "");
                if (ToMatchExpression != null)
                {
                    result.Append(ToMatchExpression);
                    result.Append(", ");
                }
                result.Append("@\"");
                result.Append(Regex);
                result.Append("\"");
                result.Append(ReplacePattern != null ? string.Format(CultureInfo.InvariantCulture, ", @\"{0}\"", ReplacePattern) : "");
                if (RegexOptions != RegexOptions.None)
                {
                    result.Append(", ");
                    result.Append(string.Join(" | ",
                        from int v in Enum.GetValues(typeof(RegexOptions))
                        where v > 0 && (RegexOptions & ((RegexOptions)v)) == ((RegexOptions)v)
                        select string.Format(CultureInfo.InvariantCulture, "{0}RegexOptions.{1}", RegexNamespace, Enum.GetName(typeof(RegexOptions), v))));
                }
                result.Append(")");
            }
            return result.ToString();
        }

        public string EditToDisplayString(string value, bool isVerbatim)
        {
            if (isVerbatim)
            {
                value = value.Replace("\"\"", "\"");
            }
            else
            {
                value = value.Replace(@"\""", @"""");
            }
            return value;
        }

        public string DisplayToEditString(string value, bool isVerbatim)
        {
            if (isVerbatim)
            {
                value = value.Replace("\"", "\"\"");
            }
            else
            {
                StringWriter stringWriter = new StringWriter();
                CodeDomProvider.GenerateCodeFromExpression(new CodePrimitiveExpression(value), stringWriter, null);
                value = stringWriter.ToString();
                value = value.Substring(1, value.Length - 2);
            }
            return value;
        }
    }
}
