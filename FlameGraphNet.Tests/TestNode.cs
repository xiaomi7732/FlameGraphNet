using System.Collections.Generic;
using FlameGraphNet.Core;

namespace FlameGraphNet.Tests
{
    public class TestNode : IFlameGraphNode
    {
        public string Content { get; set; }

        public double Metric { get; set; }

        public List<IFlameGraphNode> Children { get; set; }
    }
}