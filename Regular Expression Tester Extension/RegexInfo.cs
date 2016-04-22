using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RegexTester
{
    public class RegexInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Regex { get; set; }
        public string TextToMatch { get; set; }
        public string ReplacePattern { get; set; }
        public RegexOptions Options { get; set; }
        public List<TestCaseInfo> TestCaseInfos { get; set; }
    }
}
