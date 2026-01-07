using System;
using System.Windows.Controls;
using TopBarDock.Services;

namespace TopBarDock.Models
{
    public class MetricItem
    {
        public MetricType Type { get; set; }
        public bool Enabled { get; set; } = true;

        public Func<HardwareMonitor, string> Formatter { get; set; }
            = _ => "";

        public TextBlock Text { get; } = new TextBlock
        {
            Foreground = System.Windows.Media.Brushes.White,
            Margin = new System.Windows.Thickness(8, 0, 8, 0),
            FontSize = 12
        };
    }
}
