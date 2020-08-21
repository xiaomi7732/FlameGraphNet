using System;
using System.Collections.Generic;
using FlameGraphNet.Core;
using Xunit;

namespace FlameGraphNet.Tests
{
    public class DepthCounterTests
    {
        [Fact]
        public void ShouldCountNull()
        {
            DepthCounter target = DepthCounter.Instance;

            var actual = target.GetDepth(null, 10000, node => true);

            Assert.Equal(0, actual);
        }

        [Fact]
        public void ShouldCountOneLevel()
        {
            DepthCounter target = DepthCounter.Instance;

            var actual = target.GetDepth(new TestNode(), 10000, node => true);

            Assert.Equal(1, actual);
        }

        [Fact]
        public void ShouldCountMoreThanOneLevels()
        {
            DepthCounter target = DepthCounter.Instance;

            var actual = target.GetDepth(new TestNode()
            {
                Children = new List<IFlameGraphNode>{
                    new TestNode(),
                }
            }, 100000, (node) => true);

            Assert.Equal(2, actual);
        }

        [Fact]
        public void ShouldCountMoreThanOneOnOtherThanTheFirstChild()
        {
            DepthCounter target = DepthCounter.Instance;

            var actual = target.GetDepth(new TestNode()
            {
                Children = new List<IFlameGraphNode>{
                    new TestNode(),
                    new TestNode(){
                        Children = new List<IFlameGraphNode>(){
                            new TestNode(),
                        }
                    }
                }
            }, 1000000, (node) => true);

            Assert.Equal(3, actual);
        }
    }
}
