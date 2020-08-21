namespace FlameGraphNet.Core
{
    public class FlameGraphOptions
    {
        public string Title { get; set; }
        public int RowHeight { get; set; } = 18;
        public int Width { get; set; } = 1920;
        public int Height { get; set; } = 1000;

        public bool AutoHeight { get; set; }

        public int HeaderHeight { get; set; } = 50;

        public int WorkingSpaceHeight => Height - HeaderHeight;
    }
}