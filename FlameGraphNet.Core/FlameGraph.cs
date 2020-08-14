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
        private const string WhiteSpace = " ";
        private readonly int _maxDepth;
        private const int TextMargin = 3;
        private const int GraphMargin = 10;

        public int Width => _options.Width;
        public int Height => _options.Height;
        public int RowHeight => _options.RowHeight;


        public FlameGraph(FlameGraphOptions options = null)
        {
            _options = options ?? new FlameGraphOptions();
            _maxDepth = _options.WorkingSpaceHeight / _options.RowHeight - 1;
        }

        public MemoryStream Build(IFlameGraphNode root)
        {
            if (root == null)
            {
                return null;
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

        private void Build(
            SvgGroup workingGroup,
            IFlameGraphNode node,
            int width,
            int left,
            int depth,
            IFlameGraphNode parent)
        {
            if (depth > _maxDepth) return;

            var unitGroup = new SvgGroup();
            unitGroup.CustomAttributes.Add("class", "unit_g");
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

            var rect = new SvgRectangle
            {
                Fill = new SvgColourServer(Color.DarkOrange),
                X = left,
                Y = Height - (depth + 1) * RowHeight,
                CornerRadiusX = 1,
                CornerRadiusY = 1,
                Width = width - 1,
                Height = RowHeight - 1,
            };


            unitGroup.Children.Add(rect);

            var text = new SvgText(WhiteSpace)
            {
                Y = { Height - depth * RowHeight - 5 },
                X = { left + TextMargin },
                FontSize = 12,
                FontWeight = SvgFontWeight.W500,
                Fill = new SvgColourServer(Color.Black),
            };
            unitGroup.Children.Add(text);

            workingGroup.Children.Add(unitGroup);

            // Process children elements

            int? childrenCount = node.Children?.Count();

            if (childrenCount.HasValue)
            {
                double metricsSum = node.Children.Sum(item => item.Metric);
                if (metricsSum == 0) return;
                double metricsRatio = metricsSum / node.Metric;
                int childrenWidth = (int)(width * metricsRatio);
                int startPoint = left + (int)(width - childrenWidth) / 2;
                if (childrenCount.Value == 1)
                {
                    Build(workingGroup, node.Children.First(), (int)childrenWidth, startPoint, depth + 1, node);
                }
                else if (childrenCount.Value > 1)
                {
                    var last = node.Children.Last();
                    int remianingWidth = childrenWidth;
                    foreach (var child in node.Children)
                    {
                        if (!ReferenceEquals(last, child))
                        {
                            int ratioWidth = (int)(child.Metric / metricsSum * childrenWidth);
                            Build(workingGroup, child, ratioWidth, startPoint, depth + 1, node);
                            startPoint += ratioWidth;
                            remianingWidth -= ratioWidth;
                        }
                        else
                        {
                            Build(workingGroup, child, remianingWidth, startPoint, depth + 1, node);
                        }
                    }
                }
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
