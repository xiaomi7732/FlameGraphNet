using System.Collections.Generic;
using FlameGraphNet.Core;

namespace FlameGraphNet
{
    class SimpleNode : IFlameGraphNode
    {
        public string Content { get; set; }

        public double Metric { get; set; }

        public List<IFlameGraphNode> Children { get; } = new List<IFlameGraphNode>();
    }
}