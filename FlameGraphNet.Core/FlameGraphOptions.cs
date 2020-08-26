using System;
using System.Drawing;

namespace FlameGraphNet.Core
{
    /// <summary>
    /// Options for creating the flamegraph
    /// </summary>
    public class FlameGraphOptions
    {
        /// <summary>
        /// Gets or sets the title for the flame graph
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the row height of the frames. Default value is 18.
        /// </summary>
        /// <value></value>
        public int RowHeight { get; set; } = 18;

        /// <summary>
        /// Gets or sets the total width of the image. Default value is 1920.
        /// </summary>
        public int Width { get; set; } = 1920;

        /// <summary>
        /// Gets or sets the total height of the svg. Default is 1000. This value will be overwrite when AutoHeight is set to true.
        /// </summary>
        public int Height { get; set; } = 1000;

        /// <summary>
        /// Gets or sets whether to auto-compute the height of the generated svg.
        /// </summary>
        public bool AutoHeight { get; set; }

        /// <summary>
        /// Gets or sets the header height of the generated svg. Default to 50.
        /// </summary>
        public int HeaderHeight { get; set; } = 50;

        /// <summary>
        /// Gets or sets a delegate to determine the frame backend color. The default color used for a frame is DarkOrange when not provided.
        /// </summary>
        public Func<IFlameGraphNode, Color> FrameBackgroundProvider { get; set; }
        
        /// <summary>
        /// Gets the working space height.
        /// </summary>
        public int WorkingSpaceHeight => Height - HeaderHeight;
    }
}