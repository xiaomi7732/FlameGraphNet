using System;
using System.Collections.Generic;
using System.Linq;
using Svg;

namespace FlameGraphNet.Core
{
    public class DepthCounter
    {
        public static DepthCounter Instance { get; } = new DepthCounter();
        private DepthCounter() { }

        public int GetDepth(IFlameGraphNode baseline, Predicate<IFlameGraphNode> filter = null)
        {
            if (baseline == null)
            {
                return 0;
            }

            // Default filter that always true.
            filter = filter ?? ((node) => true);

            IEnumerable<IFlameGraphNode> workingItems = new List<IFlameGraphNode> { baseline };
            int level = 0;
            while (workingItems.Count() > 0)
            {
                level++;
                workingItems = workingItems.SelectMany(item => item.Children.NullAsEmpty()).Where((node) => filter(node));
            }

            return level;
        }
    }
}