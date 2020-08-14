using System.Collections.Generic;

namespace FlameGraphNet.Core
{
    public interface IFlameGraphNode
    {
        string Content { get; }
        double Metric { get; }

        List<IFlameGraphNode> Children { get; }
    }
}