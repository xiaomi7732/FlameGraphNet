using System.Collections.Generic;

namespace FlameGraphNet
{
    public class TreeNode
    {
        public string Text { get; }

        public double Value { get; }

        public TreeNode(string text, double value)
        {
            Text = text;
            Value = value;
        }

        public List<TreeNode> Children { get; } = new List<TreeNode>();
    }
}