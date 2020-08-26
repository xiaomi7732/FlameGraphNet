using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using Svg;

namespace FlameGraphNet.Core
{
    public class FlameGraph
    {
        private FlameGraphOptions _options;
        private readonly DepthCounter _depthCounter;
        private const string WhiteSpace = " ";
        private int _maxDepth;
        private const int TextMargin = 3;
        private const int GraphMargin = 10;

        private readonly Func<IFlameGraphNode, Color> _getFrameBackground;

        public int Width => _options.Width;
        public int Height { get; set; }
        public int RowHeight => _options.RowHeight;

        public FlameGraph(
            FlameGraphOptions options = null,
            DepthCounter depthCounter = null)
        {
            _options = options ?? new FlameGraphOptions();
            _depthCounter = depthCounter ?? DepthCounter.Instance;
            _maxDepth = _options.WorkingSpaceHeight / _options.RowHeight - 1;
            Height = _options.Height;
            _getFrameBackground = options.FrameBackroundProvider ?? GetDefaultFrameBackground;
        }

        /// <summary>
        /// Build a flame graph and return the svg as a stream.
        /// </summary>
        /// <param name="root">Root node for a flame graph tree.</param>
        /// <returns>A memory stream that contains the svg content.</returns>
        public MemoryStream Build(IFlameGraphNode root)
        {
            if (root == null)
            {
                return null;
            }

            if (_options.AutoHeight)
            {
                var baselineWidth = _options.Width - GraphMargin * 2;
                int actualDepth = _depthCounter.GetDepth(root, (node) => node.Metric / root.Metric * baselineWidth > 1);
                _maxDepth = actualDepth;
                Height = (_maxDepth + 1) * _options.RowHeight + _options.HeaderHeight;
            }

            SvgDocument svgDoc;
            svgDoc = BuildSvgDocument();

            var group = svgDoc.Children.OfType<SvgGroup>().FirstOrDefault();

            AppendTitle(group);

            Build(group, root, width: Width - GraphMargin * 2, left: GraphMargin, depth: 0, parent: null);

            MemoryStream outputStream = new MemoryStream();
            svgDoc.Write(outputStream);
            outputStream.Position = 0;
            return outputStream;
        }

        /// <summary>
        /// Build a flame graph to a svg file.
        /// </summary>
        /// <param name="root">Root node for a flame graph tree.</param>
        /// <param name="path">Target svg file path.</param>
        /// <returns>True when build succeeded. Otherwise, false.</returns>
        public bool BuildTo(IFlameGraphNode root, string path)
        {
            if (path is null)
            {
                throw new System.ArgumentNullException(nameof(path));
            }

            using (Stream sourceStream = Build(root))
            {
                if (sourceStream == null)
                {
                    return false;
                }
                string dirName = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dirName))
                {
                    Directory.CreateDirectory(dirName);
                }
                using (FileStream outputStream = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
                {
                    sourceStream.Position = 0;
                    sourceStream.CopyTo(outputStream);
                }
                return true;
            }
        }

        private SvgDocument BuildSvgDocument()
        {
            SvgDocument svgDoc;
            Assembly callingAssembly = Assembly.GetCallingAssembly();
            using (Stream svgTemplate = GetTemplate(callingAssembly))
            {
                svgDoc = SvgDocument.Open<SvgDocument>(svgTemplate);
            }
            svgDoc.Width = Width;
            svgDoc.Height = Height;
            svgDoc.ViewBox = new SvgViewBox(0, 0, Width, Height);
            svgDoc.CustomAttributes.Add("onload", "init(evt)");
            svgDoc.Children.Add(new SvgScript()
            {
                Content = GetScript(callingAssembly),
            });
            return svgDoc;
        }

        private void AppendTitle(SvgGroup group)
        {
            if (!string.IsNullOrEmpty(_options.Title))
            {
                group.Children.Add(new SvgText(_options.Title)
                {
                    TextAnchor = SvgTextAnchor.Middle,
                    X = { (int)_options.Width / 2 },
                    Y = { 20 },
                    FontSize = 16,
                    Color = new SvgColourServer(Color.FromArgb(0, 9, 9, 9)),
                });
            }
        }

        private void Build(
            SvgGroup workingGroup,
            IFlameGraphNode node,
            SvgUnit width,
            SvgUnit left,
            int depth,
            IFlameGraphNode parent)
        {
            // Reach the max depth
            if (depth > _maxDepth) return;

            // Adjust for space between columns.
            SvgUnit adjustedWidth = width - 1;
            if (adjustedWidth < 0) return;

            // Adjust height for space between rows
            SvgUnit top = Height - (depth + 1) * RowHeight;
            SvgUnit adjustedHeight = RowHeight - 1;
            Debug.Assert(adjustedHeight > 0);

            var unitGroup = new SvgGroup();
            workingGroup.Children.Add(unitGroup);
            unitGroup.CustomAttributes.Add("class", "unit_g");

            // Add tooltip for full content
            var title = new SvgTitle()
            {
                Content = node.Content
            };

            // onclick="zoom(this)"
            unitGroup.CustomAttributes.Add("onclick", @"zoom(this)");
            // onmouseover="s('sh_xfree (4 samples, 0.13%)')" 
            unitGroup.CustomAttributes.Add("onmouseover", $@"s('{node.Content} ({node.Metric.ToString("0.00")} ms)')");
            // onmouseout="c()"
            unitGroup.CustomAttributes.Add("onmouseout", @"c()");

            unitGroup.Children.Add(title);

            // Create the rectangle:
            var rect = new SvgRectangle
            {
                Fill = new SvgColourServer(_getFrameBackground(node)),
                X = left,
                Y = top,
                CornerRadiusX = 1,
                CornerRadiusY = 1,
                Width = adjustedWidth,
                Height = adjustedHeight,
            };

            unitGroup.Children.Add(rect);

            var text = new SvgText(GetFitText(node.Content, adjustedWidth))
            {
                Y = { top + RowHeight - 5 },
                X = { left + TextMargin },
                FontSize = 12,
                FontWeight = SvgFontWeight.W500,
                Fill = new SvgColourServer(Color.Black),
            };
            unitGroup.Children.Add(text);

            // Process children elements
            int? childrenCount = node.Children?.Count();

            if (childrenCount.HasValue)
            {
                double metricsSum = node.Children.Sum(item => item.Metric);
                if (metricsSum == 0) return;
                double metricsRatio = metricsSum / node.Metric;
                Debug.Assert(metricsRatio <= 1);
                SvgUnit childrenWidth = (float)(width * metricsRatio);
                SvgUnit childLeft = left + (float)(width - childrenWidth) / 2; // Center the child

                foreach (var child in node.Children)
                {
                    SvgUnit ratioWidth = (float)(child.Metric / metricsSum * childrenWidth);
                    Build(workingGroup, child, ratioWidth, childLeft, depth + 1, node);
                    childLeft += ratioWidth;
                }
            }
        }

        private Color GetDefaultFrameBackground(IFlameGraphNode node)
        {
            return Color.DarkOrange;
        }

        private string GetFitText(string fullText, SvgUnit width)
        {
            if (width < 2 * 12 * .6)
            {
                // Won't fit anything.
                return WhiteSpace;
            }

            string content = fullText;
            if (string.IsNullOrWhiteSpace(fullText))
            {
                return content;
            }

            int fitCount = (int)(width / 7);
            if (content.Length < fitCount)
            {
                return content;
            }
            else if (fitCount - 2 < 0)
            {
                return WhiteSpace;
            }
            else
            {
                return content.Substring(0, fitCount - 2) + "..";
            }
        }

        private Stream GetTemplate(Assembly from)
        {
            string templateResId = $"{from.GetName().Name}.template.svg";
            return from.GetManifestResourceStream(templateResId);
        }
        private string GetScript(Assembly from)
        {
            string scriptResId = $"{from.GetName().Name}.script.min.js";
            using (var scriptStream = from.GetManifestResourceStream(scriptResId))
            using (StreamReader reader = new StreamReader(scriptStream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
