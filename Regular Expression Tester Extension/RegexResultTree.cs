using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RegexTester
{
    /// <summary>
    /// Visitor interface for regular expression result tree.
    /// </summary>
    public interface IRegexResultTreeNodeVisitor
    {
        void Visit(RootNode node, bool beforeChildren);
        void Visit(MatchNode node, bool beforeChildren);
        void Visit(GroupNode node, bool beforeChildren);
        void Visit(LiteralNode node);
    }

    /// <summary>
    /// Base class for regular expression tree nodes.
    /// </summary>
    public abstract class RegexResultTreeNode : IComparable
    {
        private SortedDictionary<RegexResultTreeNode, RegexResultTreeNode> children = new SortedDictionary<RegexResultTreeNode, RegexResultTreeNode>();

        public RegexResultTreeNode(int startIndex, int endIndex)
        {
            StartIndex = startIndex;
            EndIndex = endIndex;
        }

        public void AddChildNode(RegexResultTreeNode child)
        {
            children.Add(child, child);
        }

        public void RemoveChildNode(RegexResultTreeNode child)
        {
            children.Remove(child);
        }

        public IEnumerable<RegexResultTreeNode> ChildNodes { get { return children.Values; } }

        public abstract void Accept(IRegexResultTreeNodeVisitor visitor);

        public int StartIndex { get; private set; }
        public int EndIndex { get; private set; }

        public int CompareTo(object obj)
        {
            RegexResultTreeNode other = obj as RegexResultTreeNode;
            if (this.StartIndex < other.StartIndex) { return -1; }
            if (other.StartIndex < this.StartIndex) { return 1; }
            if (this.EndIndex < other.EndIndex) { return -1; }
            if (other.EndIndex < this.EndIndex) { return 1; }
            return 0;
        }

        public bool IsChildOf(RegexResultTreeNode other)
        {
            return (this.StartIndex >= other.StartIndex && this.EndIndex <= other.EndIndex);
        }
    }

    /// <summary>
    /// Node representing an entire string containing regular expression
    /// matches.
    /// </summary>
    public class RootNode : RegexResultTreeNode
    {
        public RootNode(int startIndex, int endIndex) : base(startIndex, endIndex) { }

        public override void Accept(IRegexResultTreeNodeVisitor visitor)
        {
            visitor.Visit(this, true);
            foreach (RegexResultTreeNode child in this.ChildNodes)
            {
                child.Accept(visitor);
            }
            visitor.Visit(this, false);
        }
    }

    /// <summary>
    /// Node containing a match.
    /// </summary>
    public class MatchNode : RegexResultTreeNode
    {
        public MatchNode(int startIndex, int endIndex) : base(startIndex, endIndex) { }

        public override void Accept(IRegexResultTreeNodeVisitor visitor)
        {
            visitor.Visit(this, true);
            foreach (RegexResultTreeNode child in this.ChildNodes)
            {
                child.Accept(visitor);
            }
            visitor.Visit(this, false);
        }
    }

    /// <summary>
    /// Node containing a group within a match.
    /// </summary>
    public class GroupNode : RegexResultTreeNode
    {
        public GroupNode(string groupName, int startIndex, int endIndex) : base(startIndex, endIndex)
        {
            this.GroupName = groupName;
        }

        public string GroupName { get; private set; }

        public override void Accept(IRegexResultTreeNodeVisitor visitor)
        {
            visitor.Visit(this, true);
            foreach (RegexResultTreeNode child in this.ChildNodes)
            {
                child.Accept(visitor);
            }
            visitor.Visit(this, false);
        }
    }

    /// <summary>
    /// Node containing a literal.
    /// </summary>
    public class LiteralNode : RegexResultTreeNode
    {
        public LiteralNode(string literal, int startIndex, int endIndex) : base(startIndex, endIndex)
        {
            this.Literal = literal;
        }

        public string Literal { get; private set; }

        public override void Accept(IRegexResultTreeNodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
