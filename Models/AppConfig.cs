using System.Collections.Generic;

namespace TopBarDock.Models
{
    public class AppConfig
    {
        // ===== BAR APPEARANCE =====
        public int BarHeight { get; set; } = 52;
        public double Opacity { get; set; } = 1.0;
        public string Background { get; set; } = "#1A1F2B";
        public bool DarkTheme { get; set; } = true;
        public int SelectedGpuIndex { get; set; } = 0;

        // ===== METRICS STATE (PERSIST) =====
        public List<MetricConfig> Metrics { get; set; } = new();
        public NetworkDisplayMode NetDisplayMode { get; set; } = NetworkDisplayMode.Smoothed;
    }

    public enum NetworkDisplayMode
    {
        Instant,    // tanpa smoothing
        Smoothed,   // default (rekomendasi)
        Average5s   // rata-rata 5 detik
    }

    public class MetricConfig
    {
        public string Key { get; set; } = "";
        public bool Enabled { get; set; }
        public int Order { get; set; }
    }
}
