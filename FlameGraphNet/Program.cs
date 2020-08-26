using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using FlameGraphNet.Core;

namespace FlameGraphNet
{
    class Program
    {
        static void Main(string[] args)
        {
            SimpleNodeExample();

            NodeAdapterExample();

            ColorizerExample();
        }

        private static void ColorizerExample()
        {
            const int nodeCount = 20;
            SimpleNode root = new SimpleNode()
            {
                Content = $"Node {nodeCount.ToString("0")}",
                Metric = nodeCount,
            };
            root = AppendChildren(root, nodeCount);

            FlameGraph newGraph = new FlameGraph(new FlameGraphOptions()
            {
                Title = "Hello Flame Graph",
                Width = 800,
                Height = 600,
                FrameBackgroundProvider= node =>
                {
                    if (node.Metric > 10)
                    {
                        return Color.OrangeRed;
                    }
                    return Color.DarkOrange;
                },
            });

            string fileName = Path.Combine("Examples", nameof(ColorizerExample) + ".svg");
            DeleteFileWhenExists(fileName);
            newGraph.BuildTo(root, fileName);
        }

        #region Simple Node Example
        private static void SimpleNodeExample()
        {
            const int nodeCount = 20;
            SimpleNode root = new SimpleNode()
            {
                Content = $"Node {nodeCount.ToString("0")}",
                Metric = nodeCount,
            };
            root = AppendChildren(root, nodeCount);

            FlameGraph newGraph = new FlameGraph(new FlameGraphOptions()
            {
                Title = "Hello Flame Graph",
                Width = 800,
                Height = 600,
            });

            string fileName = Path.Combine("Examples", nameof(SimpleNodeExample) + ".svg");
            DeleteFileWhenExists(fileName);
            newGraph.BuildTo(root, fileName);
        }

        // Generate a simple tree.
        private static SimpleNode AppendChildren(SimpleNode current, double metricValue)
        {
            metricValue--;
            if (metricValue > 0)
            {
                SimpleNode newChild = new SimpleNode()
                {
                    Content = $"Node {metricValue.ToString("0")}",
                    Metric = metricValue,
                };

                current.Children.Add(AppendChildren(newChild, metricValue));
            }
            return current;
        }
        #endregion

        #region Node adapter example
        private static void NodeAdapterExample()
        {
            // Create delegates for adaption
            Func<TreeNode, string> getContent = n => n.Text;
            Func<TreeNode, double> getMetric = n => n.Value;
            Func<TreeNode, IEnumerable<TreeNode>> getChildren = n => n.Children;

            // Generate an example tree
            const int levels = 20;
            TreeNode root = new TreeNode("Root", levels * 4);
            root = AppendChildren(root, levels * 4);
            TreeNode root2 = new TreeNode("Root2", levels);
            root2 = AppendChildren(root2, levels);
            TreeNode fullTree = new TreeNode("Full Tree", levels * 4 + levels);
            fullTree.Children.Add(root);
            fullTree.Children.Add(root2);

            // Adapter the tree
            var wrappedRoot = new FlameGraphNode<TreeNode>(fullTree, getContent, getMetric, getChildren);

            // Output the svg stream to file.
            FlameGraph graph = new FlameGraph(new FlameGraphOptions()
            {
                Title = "Hello Flame Graph",
                Width = 800,
                Height = 800,
                AutoHeight = true,
            });

            using Stream svgStream = graph.Build(wrappedRoot);
            using FileStream fileStream = new FileStream(Path.Combine("Examples", $"{nameof(NodeAdapterExample)}.svg"), FileMode.Create, FileAccess.Write);
            svgStream.CopyTo(fileStream);
        }

        private static TreeNode AppendChildren(TreeNode current, double metricValue)
        {
            metricValue--;
            if (metricValue > 0)
            {
                TreeNode newChild = new TreeNode($"Node {metricValue.ToString("0")}", metricValue);
                current.Children.Add(AppendChildren(newChild, metricValue));
            }
            return current;
        }
        #endregion

        private static void DeleteFileWhenExists(string resultFilePath)
        {
            if (File.Exists(resultFilePath))
            {
                File.Delete(resultFilePath);
            }
        }
    }
}
