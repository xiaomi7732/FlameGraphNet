using System;
using System.Collections.Generic;
using System.Linq;

namespace FlameGraphNet.Core
{
    public class FlameGraphNode<T> : IFlameGraphNode
    {
        private readonly T _obj;
        private readonly Func<T, string> _getContent;
        private readonly Func<T, double> _getMetric;
        private readonly Func<T, IEnumerable<T>> _getChildren;

        public FlameGraphNode(
            T root,
            Func<T, string> getContent,
            Func<T, double> getMetric,
            Func<T, IEnumerable<T>> getChildren)
        {
            _getContent = getContent ?? throw new ArgumentNullException(nameof(getContent));
            _getMetric = getMetric ?? throw new ArgumentNullException(nameof(getMetric));
            _getChildren = getChildren ?? throw new ArgumentNullException(nameof(getChildren));
            _obj = root;
            CopyTree(_obj);
        }

        public string Content => _getContent(_obj);

        public double Metric => _getMetric(_obj);

        public void AddChild(IFlameGraphNode child)
        {
            AddChildren(child.Yield());
        }

        public void AddChildren(IEnumerable<IFlameGraphNode> children)
        {
            Children.AddRange(children);
        }

        public List<IFlameGraphNode> Children { get; } = new List<IFlameGraphNode>();

        private void CopyTree(T current)
        {
            IEnumerable<T> children = _getChildren(current);
            if (children != null && children.Any())
            {
                foreach (var child in children)
                {
                    AddChild(new FlameGraphNode<T>(child, _getContent, _getMetric, _getChildren));
                }
            }
        }
    }
}