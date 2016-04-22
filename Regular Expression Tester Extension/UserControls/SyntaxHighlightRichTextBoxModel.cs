using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RegexTester.UserControls
{
    public class SyntaxHighlightRichTextBoxModel
    {
        private Dictionary<char, char> matchingEndParentheses =
            new Dictionary<char, char>{
                {'(', ')'},
                {'[', ']'},
                {'{', '}'}
            };

        private Dictionary<char, char> matchingStartParentheses =
            new Dictionary<char, char> {
                {')', '('},
                {']', '['},
                {'}', '{'}
            };

        private IDictionary<int, int> GetMatchingParentheses(string value)
        {
            IDictionary<int, int> result = new Dictionary<int, int>();
            Stack<KeyValuePair<char, int>> stack = new Stack<KeyValuePair<char, int>>();
            int escapeCount = 0;
            char[] chars = value.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (escapeCount % 2 == 0)
                {
                    if (matchingEndParentheses.ContainsKey(chars[i]))
                    {
                        stack.Push(new KeyValuePair<char, int>(chars[i], i));
                    }
                    else if (matchingStartParentheses.ContainsKey(chars[i]))
                    {
                        if (stack.Count > 0 && matchingEndParentheses[stack.Peek().Key] == chars[i])
                        {
                            result.Add(stack.Pop().Value, i);
                        }
                        else
                        {
                            return result;
                        }
                    }
                }
                escapeCount = chars[i] == '\\' ? escapeCount + 1 : 0;
            }
            return result;
        }

        public int GetMatchingParentheses(string value, int index)
        {
            char[] chars = value.ToCharArray();
            IDictionary<int, int> result = GetMatchingParentheses(value);
            foreach (KeyValuePair<int, int> pair in result)
            {
                if (pair.Value == index - 1)
                {
                    return pair.Key;
                }
                if (pair.Key == index)
                {
                    return pair.Value;
                }
            }
            return -1;
        }
    }
}
