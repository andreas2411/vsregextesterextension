using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.IO;
using System.Xml;

namespace RegexTester
{
    public class ParsedRegex
    {
        public Regex Regex { get; set; }
        public string ParseError { get; set; }
    }

    /// <summary>
    /// Logic for parsing regular expression results.
    /// </summary>
    public class RegexTesterModel
    {
        /// <summary>
        /// Returns regular expression for <paramref name="regularExpression"/>
        /// or null if string could not be parsed as regular expression.
        /// </summary>
        /// <param name="regularExpression"></param>
        /// <returns></returns>
        public ParsedRegex GetRegularExpression(string regularExpression, RegexOptions options)
        {
            ParsedRegex result = new ParsedRegex();
            if (regularExpression != null)
            {
                try
                {
                    result.Regex = new Regex(regularExpression, options);
                }
                catch (Exception e)
                {
                    string[] errorParts = e.Message.Split(new string[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
                    result.ParseError = errorParts[errorParts.Length - 1];
                }
            }
            return result;
        }

        private void AddRegexResultTreeNode(RegexResultTreeNode parent, RegexResultTreeNode child)
        {
            List<RegexResultTreeNode> toRemove = new List<RegexResultTreeNode>();
            foreach (RegexResultTreeNode otherNode in parent.ChildNodes)
            {
                if (child.IsChildOf(otherNode))
                {
                    AddRegexResultTreeNode(otherNode, child);
                    return;
                }
                else if (otherNode.IsChildOf(child))
                {
                    toRemove.Add(otherNode);
                    child.AddChildNode(otherNode);
                }
            }
            foreach (RegexResultTreeNode node in toRemove)
            {
                parent.RemoveChildNode(node);
            }
            parent.AddChildNode(child);
        }

        private void AddLiteralNode(List<LiteralNode> nodes, int startIndex, int endIndex, string toMatch)
        {
            if (startIndex < endIndex)
            {
                string literal = toMatch.Substring(startIndex, endIndex - startIndex);
                nodes.Add(new LiteralNode(literal, startIndex, endIndex));
            }
        }

        private void AddLiteralNodes(RegexResultTreeNode regexResultTreeNode, string toMatch)
        {
            int currentIndex = regexResultTreeNode.StartIndex;
            List<LiteralNode> literalNodes = new List<LiteralNode>();
            foreach (RegexResultTreeNode child in regexResultTreeNode.ChildNodes)
            {
                AddLiteralNode(literalNodes, currentIndex, child.StartIndex, toMatch);
                AddLiteralNodes(child, toMatch);
                currentIndex = child.EndIndex;
            }
            AddLiteralNode(literalNodes, currentIndex, regexResultTreeNode.EndIndex, toMatch);
            foreach (LiteralNode literalNode in literalNodes)
            {
                regexResultTreeNode.AddChildNode(literalNode);
            }
        }

        private RegexResultTreeNode BuildRegexResultTree(Regex regularExpression, string toMatch)
        {
            RootNode root = new RootNode(0, toMatch.Length);
            MatchCollection matches = regularExpression.Matches(toMatch);
            foreach (Match match in matches)
            {
                MatchNode matchNode = new MatchNode(match.Index, match.Index + match.Length);
                root.AddChildNode(matchNode);
                int groupIndex = 0;
                foreach (Group group in match.Groups)
                {
                    if (groupIndex != 0 && group.Success)
                    {
                        string groupName = regularExpression.GroupNameFromNumber(groupIndex);
                        foreach (Capture capture in group.Captures)
                        {
                            AddRegexResultTreeNode(matchNode, new GroupNode(groupName, capture.Index, capture.Index + capture.Length));
                        }
                    }
                    groupIndex++;
                }
            }
            AddLiteralNodes(root, toMatch);
            return root;
        }

        public RegexResultTreeNode BuildMatchTree(Regex regularExpression, string toMatch)
        {
            if (toMatch != null)
            {
                if (regularExpression == null)
                {
                    return new RootNode(0, toMatch.Length);
                }
                return BuildRegexResultTree(regularExpression, toMatch);
            }
            return null;
        }

        private string fileName = "RegexTesterSaves.xml";

        private Stream GetStorage()
        {
            return IsolatedStorageFile.GetUserStoreForAssembly().OpenFile(fileName, FileMode.OpenOrCreate);
        }

        private void ResetStorage()
        {
            IsolatedStorageFile.GetUserStoreForAssembly().DeleteFile(fileName);
        }

        public void SaveRegexs(IEnumerable<RegexInfo> regexInfos)
        {
            ResetStorage();
            XmlDocument doc = new XmlDocument();
            using (StreamReader reader = new StreamReader(GetStorage()))
            {
                try
                {
                    doc.LoadXml(reader.ReadToEnd());
                }
                catch (XmlException)
                {
                    // Ignore and overwrite invalid document.
                }
            }

            XmlElement rootNode = null;
            if (string.IsNullOrEmpty(doc.OuterXml))
            {
                doc.CreateXmlDeclaration("1.0", "utf-8", "yes");
                rootNode = doc.CreateElement("SavedRegexs");
                doc.AppendChild(rootNode);
            }
            else
            {
                rootNode = doc.DocumentElement;
            }
            foreach (RegexInfo toSave in regexInfos)
            {
                XmlElement savedRegexNode = doc.CreateElement("SavedRegex");
                rootNode.AppendChild(savedRegexNode);
                XmlElement idNode = doc.CreateElement("Id");
                idNode.InnerText = toSave.Id.ToString();
                savedRegexNode.AppendChild(idNode);
                XmlElement nameNode = doc.CreateElement("Name");
                nameNode.InnerText = toSave.Name ?? "";
                savedRegexNode.AppendChild(nameNode);
                XmlElement regexNode = doc.CreateElement("Regex");
                regexNode.InnerText = toSave.Regex ?? "";
                savedRegexNode.AppendChild(regexNode);
                XmlElement replaceNode = doc.CreateElement("ReplacePattern");
                replaceNode.InnerText = toSave.ReplacePattern ?? "";
                savedRegexNode.AppendChild(replaceNode);
                XmlElement optionsNode = doc.CreateElement("Options");
                savedRegexNode.AppendChild(optionsNode);
                foreach (int v in Enum.GetValues(typeof(RegexOptions)))
                {
                    if ((((RegexOptions)v) & toSave.Options) == ((RegexOptions)v))
                    {
                        XmlElement optionNode = doc.CreateElement("Option");
                        optionNode.InnerText = Enum.GetName(typeof(RegexOptions), v);
                        optionsNode.AppendChild(optionNode);
                    }
                }
                XmlElement testCasesNode = doc.CreateElement("TestCases");
                savedRegexNode.AppendChild(testCasesNode);
                if (toSave.TestCaseInfos != null)
                {
                    foreach (TestCaseInfo testCaseInfo in toSave.TestCaseInfos)
                    {
                        XmlElement testCaseNode = doc.CreateElement("TestCase");
                        testCasesNode.AppendChild(testCaseNode);
                        XmlElement testCaseIdNode = doc.CreateElement("Id");
                        testCaseIdNode.InnerText = testCaseInfo.Id.ToString();
                        testCaseNode.AppendChild(testCaseIdNode);
                        XmlElement testCaseNameNode = doc.CreateElement("Name");
                        testCaseNameNode.InnerText = testCaseInfo.Name;
                        testCaseNode.AppendChild(testCaseNameNode);
                        XmlElement textToMatchNode = doc.CreateElement("TextToMatch");
                        textToMatchNode.InnerText = testCaseInfo.TextToMatch;
                        testCaseNode.AppendChild(textToMatchNode);
                    }
                }
            }
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;
            settings.Indent = true;
            using (Stream storageStream = GetStorage())
            {
                XmlWriter xmlWriter = XmlWriter.Create(storageStream, settings);
                doc.WriteTo(xmlWriter);
                xmlWriter.Flush();
                xmlWriter.Close();
            }
        }

        public IEnumerable<RegexInfo> GetRegularExpressions()
        {
            List<RegexInfo> result = new List<RegexInfo>();
            XmlDocument doc = new XmlDocument();
            using (StreamReader reader = new StreamReader(GetStorage()))
            {
                try
                {
                    doc.LoadXml(reader.ReadToEnd());
                }
                catch (XmlException)
                {
                }
            }

            if (doc.DocumentElement != null)
            {
                try
                {
                    foreach (XmlElement savedRegexNode in doc.DocumentElement.ChildNodes)
                    {
                        RegexInfo regexInfo = new RegexInfo
                        {
                            TestCaseInfos = new List<TestCaseInfo>()
                        };
                        foreach (XmlElement child in savedRegexNode.ChildNodes)
                        {
                            switch (child.Name)
                            {
                                case "Id":
                                    regexInfo.Id = Guid.Parse(child.InnerText);
                                    break;
                                case "Name":
                                    regexInfo.Name = child.InnerText;
                                    break;
                                case "Regex":
                                    regexInfo.Regex = child.InnerText;
                                    break;
                                case "ReplacePattern":
                                    regexInfo.ReplacePattern = child.InnerText;
                                    break;
                                case "Options":
                                    foreach (XmlElement optionChild in child.ChildNodes)
                                    {
                                        regexInfo.Options |= (RegexOptions)Enum.Parse(typeof(RegexOptions), optionChild.InnerText);
                                    }
                                    break;
                                case "TestCases":
                                    foreach (XmlElement testCase in child.ChildNodes)
                                    {
                                        TestCaseInfo testCaseInfo = new TestCaseInfo();
                                        foreach (XmlElement testCaseChild in testCase.ChildNodes)
                                        {
                                            switch (testCaseChild.Name)
                                            {
                                                case "Id":
                                                    testCaseInfo.Id = Guid.Parse(testCaseChild.InnerText);
                                                    break;
                                                case "Name":
                                                    testCaseInfo.Name = testCaseChild.InnerText;
                                                    break;
                                                case "TextToMatch":
                                                    testCaseInfo.TextToMatch = testCaseChild.InnerText;
                                                    break;
                                            }
                                        }
                                        regexInfo.TestCaseInfos.Add(testCaseInfo);
                                    }
                                    break;
                            }
                        }
                        result.Add(regexInfo);
                    }
                }
                catch (Exception)
                {
                    // Storage was corrupt. Reset it.
                    //ResetStorage();
                }
            }

            return result;
        }
    }
}
