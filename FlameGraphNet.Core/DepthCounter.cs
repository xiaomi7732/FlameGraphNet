using System.Collections.Generic;
using System.Linq;

namespace FlameGraphNet.Core
{
    public class DepthCounter
    {
        public static DepthCounter Instance { get; } = new DepthCounter();
        private DepthCounter() { }

        public int GetDepth(IFlameGraphNode baseline)
        {
            if (baseline == null)
            {
                return 0;
            }

            IEnumerable<IFlameGraphNode> workingItems = new List<IFlameGraphNode> { baseline };
            int level = 0;
            while (workingItems.Count() > 0)
            {
                level++;
                workingItems = workingItems.SelectMany(item => item.Children.NullAsEmpty());
            }

            return level;
        }
    }
}