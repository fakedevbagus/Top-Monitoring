using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TopBarDock.Models;
using TopBarDock.Services;

namespace TopBarDock.Views
{
    public partial class TopBarWindow : Window
    {
        // ================= FPS =================
        int frameCount = 0;
        double fps = 0;
        DateTime lastFpsTime = DateTime.Now;
        private DateTime _lastFpsUpdate = DateTime.Now;
        public static TopBarWindow? Instance;
        private const double NET_ALPHA = 0.15;
        public HardwareMonitor Hw => hw;
    

        AppConfig cfg;
        HardwareMonitor hw = new();
        DispatcherTimer timer = new();

        public List<MetricDefinition> Metrics { get; } = new();

        readonly Dictionary<string, TextBlock> blocks = new();

        SettingsWindow? settings;

        public TopBarWindow()
        {
            InitializeComponent();
            Instance = this;
            Closed += (_, _) =>
            {
                CompositionTarget.Rendering -= OnRendering;
                AppBar.Unregister(this);
            };


            cfg = ConfigService.Load();

            hw.SelectGpu(cfg.SelectedGpuIndex);

            InitMetrics();
            ApplyConfig();
            BuildUI();       

            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (_, _) => UpdateMetrics();
            timer.Start();

            SourceInitialized += (_, _) =>
            {
                AppBar.Register(this);
                AppBar.SetPosition(this);
            };

            Closed += (_, _) => AppBar.Unregister(this);
        }

        void OnRendering(object? sender, EventArgs e)
        {
            frameCount++;

            var now = DateTime.Now;
            var dt = (now - lastFpsTime).TotalSeconds;

            if (dt >= 1.0)
            {
                fps = frameCount / dt;
                frameCount = 0;
                lastFpsTime = now;
            }
        }


        public void RebuildMetrics()
        {
            // 🔥 SORT ULANG SESUAI ORDER TERBARU
            Metrics.Sort((a, b) => a.Order.CompareTo(b.Order));

            BuildUI();
            UpdateMetrics();
        }


        // ================= METRICS (SUPER LENGKAP) =================
        void InitMetrics()
        {
            Metrics.Clear();
            Metrics.Add(new MetricDefinition(
                "fps",
                "FPS",
                true,
                () => $"{hw.Fps:0}"
                
            ));

            // ===== CPU =====
            Metrics.Add(new MetricDefinition(
                "cpu_load", "CPU Load", true,
                () => $"{hw.CpuLoad:0}%", 0));

            Metrics.Add(new MetricDefinition(
                "cpu_temp", "CPU Temp", true,
                () => $"{hw.CpuTemp:0}°C", 1));

            Metrics.Add(new MetricDefinition(
                "cpu_power", "CPU Power", false,
                () => $"{hw.CpuPower:0} W", 2));

            // ===== GPU =====
            Metrics.Add(new MetricDefinition(
                "gpu_load", "GPU Load", true,
                () => $"{hw.GpuLoad:0}%", 3));

            Metrics.Add(new MetricDefinition(
                "gpu_temp", "GPU Temp", false,
                () => $"{hw.GpuTemp:0}°C", 4));

            Metrics.Add(new MetricDefinition(
                "gpu_power",
                "GPU Power",
                false,
                () => FormatPower(hw.GpuPower)));

                string FormatPower(float w)
                {
                    return w > 0 ? $"{w:0.0} W" : "N/A";
                }

            Metrics.Add(new MetricDefinition(
                "vram_used",
                "VRAM Used",
                false,
                () => $"{hw.VramUsedMB:0} MB"
            ));

            Metrics.Add(new MetricDefinition(
                "vram_total",
                "VRAM Total",
                false,
                () => $"{hw.VramTotalMB:0} MB"
            ));

            Metrics.Add(new MetricDefinition(
                "vram_percent",
                "VRAM %",
                false,
                () =>
                    hw.VramTotalMB > 0
                    ? $"{(hw.VramUsedMB / hw.VramTotalMB) * 100:0}%"
                    : "N/A"
            ));

            // ===== MEMORY =====
            Metrics.Add(new MetricDefinition(
                "ram_used", "RAM Used", false,
                () => $"{hw.RamUsedPercent:0}%", 7));

            Metrics.Add(new MetricDefinition(
                "ram_free", "RAM Free", true,
                () => $"{hw.RamFreeGB:0} GB", 8));

            // ===== STORAGE =====
            Metrics.Add(new MetricDefinition(
                "disk_c", "Disk C Free", false,
                () => $"{hw.DiskCFreeGB:0} GB", 9));

            Metrics.Add(new MetricDefinition(
                "disk_e", "Disk E Free", false,
                () => $"{hw.DiskEFreeGB:0} GB", 10));

            // ===== NETWORK =====
            Metrics.Add(new MetricDefinition(
                "net", "Network", true,
                () => FormatNet(hw.NetDownKB, hw.NetUpKB)
            ));

            string FormatNet(double downKB, double upKB)
            {
                string d = downKB >= 1024
                    ? $"{downKB / 1024:0.00} MB/s"
                    : $"{downKB:0} KB/s";

                string u = upKB >= 1024
                    ? $"{upKB / 1024:0.00} MB/s"
                    : $"{upKB:0} KB/s";

                return $"↓ {d} ↑ {u}";
            }


            // ===== RESTORE FROM CONFIG =====
            foreach (var m in Metrics)
            {
                var saved = cfg.Metrics.FirstOrDefault(x => x.Key == m.Key);
                if (saved != null)
                {
                    m.Enabled = saved.Enabled;
                    m.Order = saved.Order;
                }
            }

            Metrics.Sort((a, b) => a.Order.CompareTo(b.Order));
        }

        // ================= UI =================
       void BuildUI()
        {
            MetricPanel.Children.Clear();
            blocks.Clear();

            foreach (var m in Metrics
                .Where(x => x.Enabled)
                .OrderBy(x => x.Order))
            {
                var tb = new TextBlock
                {
                    FontSize = 12,
                    Margin = new Thickness(8, 0, 8, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = cfg.DarkTheme ? Brushes.White : Brushes.Black
                };

                MetricPanel.Children.Add(tb);
                blocks[m.Key] = tb;
            }
        }


        // ================= UPDATE =================
        void UpdateMetrics()
        {
            hw.Update();

            foreach (var m in Metrics)
            {
                if (!m.Enabled) continue;
                if (!blocks.TryGetValue(m.Key, out var tb)) continue;

                tb.Text = $"{m.Label}: {m.ValueProvider()}";
            }
        }

        // ================= CONFIG =================
        public void ApplyConfig()
        {
            Height = Math.Clamp(cfg.BarHeight, 28, 80);

            var baseColor =
                (Color)ColorConverter.ConvertFromString(cfg.Background);

            byte alpha =
                (byte)(Math.Clamp(cfg.Opacity, 0.0, 1.0) * 255);

            RootBorder.Background = new SolidColorBrush(
                Color.FromArgb(alpha, baseColor.R, baseColor.G, baseColor.B)
            );

            Foreground = cfg.DarkTheme ? Brushes.White : Brushes.Black;
        }

        // ⬇⬇⬇ METHOD HARUS DI SINI (LEVEL CLASS)
        public void SaveMetricsToConfig()
        {
            cfg.Metrics.Clear();

            foreach (var m in Metrics)
            {
                cfg.Metrics.Add(new MetricConfig
                {
                    Key = m.Key,
                    Enabled = m.Enabled,
                    Order = m.Order
                });
            }

            ConfigService.Save(cfg);
        }


        // ================= SETTINGS =================
         void OnRightClick(object sender, MouseButtonEventArgs e)
        {
            if (settings != null)
            {
                settings.Activate();
                return;
            }

            settings = new SettingsWindow(cfg, Metrics);
            settings.Closed += (_, _) =>
            {
                settings = null;
                BuildUI();
                UpdateMetrics();
            };
            settings.Show();
        }
    }
}

