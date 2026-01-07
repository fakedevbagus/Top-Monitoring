using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TopBarDock.Models;
using TopBarDock.Services;

namespace TopBarDock.Views
{
    public partial class SettingsWindow : Window
    {
        bool _dark = true;
        AppConfig cfg;

        List<MetricDefinition> _metrics;

        public SettingsWindow(AppConfig config, List<MetricDefinition> metrics)
        {
            cfg = config;
            _metrics = TopBarWindow.Instance!.Metrics;

            Width = 420;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;

            Content = BuildUI();
            ApplySettingsTheme();
        }

        void SaveMetricsToConfig()
        {
            cfg.Metrics.Clear();

            foreach (var m in _metrics)
            {
                cfg.Metrics.Add(new MetricConfig
                {
                    Key = m.Key,
                    Enabled = m.Enabled,
                    Order = m.Order
                });
            }

            
        }

        void SaveConfigToFile()
        {
            ConfigService.Save(cfg);
        }

        // ================= THEME =================
        void ApplySettingsTheme()
        {
            Background = _dark
                ? new SolidColorBrush(Color.FromRgb(30, 30, 30))
                : Brushes.White;

            Foreground = _dark ? Brushes.White : Brushes.Black;
        }
        private void OnGpuSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ComboBox box)
                return;

            int index = box.SelectedIndex;
            if (index < 0)
                return;

            // simpan config
            cfg.SelectedGpuIndex = index;
            ConfigService.Save(cfg);

            // apply ke TopBarWindow (runtime)
            var bar = TopBarWindow.Instance;
            if (bar == null)
                return;

            bar.Hw.SelectGpu(index);
            bar.RebuildMetrics();
        }

        // ================= UI =================
        UIElement BuildUI()
        {
            var root = new ScrollViewer();
            var panel = new StackPanel { Margin = new Thickness(16) };
            root.Content = panel;
            var gpuBox = new ComboBox
            {
                ItemsSource = TopBarWindow.Instance!.Hw.AvailableGpus,
                SelectedIndex = cfg.SelectedGpuIndex,
                Margin = new Thickness(0, 6, 0, 6)
            };

            gpuBox.SelectionChanged += OnGpuSelectionChanged;

            panel.Children.Add(gpuBox);


            // ===== SETTINGS THEME =====
            panel.Children.Add(Section("Settings Theme"));

            var themeToggle = new CheckBox
            {
                Content = "Dark Settings",
                IsChecked = _dark,
                Margin = new Thickness(0, 6, 0, 6)
            };

            themeToggle.Click += (_, _) =>
            {
                _dark = themeToggle.IsChecked == true;
                ApplySettingsTheme();
            };

            panel.Children.Add(themeToggle);

            // ===== BAR HEIGHT =====
            panel.Children.Add(Section("Bar Height"));
            panel.Children.Add(SliderWithInput(
                24, 80, cfg.BarHeight,
                v => { cfg.BarHeight = (int)v; Apply(); }
            ));

            // ===== OPACITY =====
            panel.Children.Add(Section("Opacity"));
            panel.Children.Add(SliderWithInput(
                5, 100, cfg.Opacity * 100,
                v => { cfg.Opacity = v / 100; Apply(); }
            ));

            // ===== COLORS =====
            panel.Children.Add(Section("Background Color"));

            string[] presets =
            {
                "#000000", "#ffffff", "#ff0000", "#0078D4", "#107c10",
                "#ffb900", "#5a5a5a", "#1a1f2b", "#2d2d30", "#0096fa"
            };

            var colorPanel = new WrapPanel();
            foreach (var c in presets)
            {
                var btn = new Button
                {
                    Width = 28,
                    Height = 28,
                    Margin = new Thickness(4),
                    Background = (Brush)new BrushConverter().ConvertFromString(c)!
                };

                btn.Click += (_, _) =>
                {
                    cfg.Background = c;
                    Apply();
                };

                colorPanel.Children.Add(btn);
            }
            panel.Children.Add(colorPanel);

            

            // ===== LIVE HEX INPUT =====
            panel.Children.Add(Label("Custom HEX Color"));

            var colorBox = new TextBox
            {
                Text = cfg.Background,
                Margin = new Thickness(0, 4, 0, 8)
            };

            colorBox.TextChanged += (_, _) =>
            {
                if (TryParseHex(colorBox.Text))
                {
                    cfg.Background = colorBox.Text;
                    Apply();
                }
            };

            panel.Children.Add(colorBox);

            // ===== METRICS =====
            panel.Children.Add(Section("Metrics"));

            foreach (var m in _metrics.OrderBy(x => x.Order))
            {
                var row = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 4, 0, 4)
                };

                var cb = new CheckBox
                {
                    Content = m.Label,
                    IsChecked = m.Enabled,
                    Width = 160
                };

                cb.Click += (_, _) =>
                {
                    m.Enabled = cb.IsChecked == true;
                    TopBarWindow.Instance!.SaveMetricsToConfig();
                    TopBarWindow.Instance?.RebuildMetrics();
                };

                var up = new Button
                {
                    Content = "↑",
                    Width = 28,
                    Margin = new Thickness(4, 0, 0, 0)
                };

                up.Click += (_, _) =>
                {
                    MoveMetric(m, -1);
                };

                var down = new Button
                {
                    Content = "↓",
                    Width = 28,
                    Margin = new Thickness(4, 0, 0, 0)
                };

                down.Click += (_, _) =>
                {
                    MoveMetric(m, +1);
                };

                row.Children.Add(cb);
                row.Children.Add(up);
                row.Children.Add(down);

                panel.Children.Add(row);
            }

            // ===== ACTIONS =====
            panel.Children.Add(Section("Actions"));

            panel.Children.Add(Button("Reset Default", () =>
            {
                cfg.BarHeight = 36;
                cfg.Opacity = 0.0 - 0.15;
                cfg.Background = "#1a1f2b";
                Apply();
            }));

            panel.Children.Add(Button("Exit Application", () =>
            {
                Application.Current.Shutdown();
            }));

            return root;
        }

        // ================= HELPERS =================
        TextBlock Section(string t) =>
            new() { Text = t, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 10, 0, 4) };

        TextBlock Label(string t) =>
            new() { Text = t, Margin = new Thickness(0, 6, 0, 2) };

        Button Button(string t, Action a)
        {
            var b = new Button { Content = t, Margin = new Thickness(0, 6, 0, 6) };
            b.Click += (_, _) => a();
            return b;
        }

        StackPanel SliderWithInput(double min, double max, double value, Action<double> onChange)
        
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal };

            var slider = new Slider
            {
                Minimum = min,
                Maximum = max,
                Value = value,
                Width = 260
            };

            var box = new TextBox
            {
                Text = value.ToString("0"),
                Width = 60,
                Margin = new Thickness(8, 0, 0, 0)
            };

            slider.ValueChanged += (_, e) =>
            {
                box.Text = e.NewValue.ToString("0");
                onChange(e.NewValue);
            };

            box.TextChanged += (_, _) =>
            {
                if (double.TryParse(box.Text, out var v))
                {
                    v = Math.Clamp(v, min, max);
                    slider.Value = v;
                }
            };

            row.Children.Add(slider);
            row.Children.Add(box);
            return row;
        }

        bool TryParseHex(string hex)
        {
            try
            {
                if (!hex.StartsWith("#")) return false;
                _ = (Color)ColorConverter.ConvertFromString(hex);
                return true;
            }
            catch { return false; }
        }

        void Apply()
        {
            SaveMetricsToConfig();   // ⬅️ PENTING
            SaveConfigToFile();      // ⬅️ PENTING
            TopBarWindow.Instance?.ApplyConfig();
            TopBarWindow.Instance?.RebuildMetrics();
        }


        void SaveMetrics()
        {
            if (TopBarWindow.Instance == null)
                return;

            cfg.Metrics.Clear();

            foreach (var m in TopBarWindow.Instance.Metrics)
            {
                cfg.Metrics.Add(new MetricConfig
                {
                    Key = m.Key,
                    Enabled = m.Enabled,
                    Order = m.Order
                });
            }
        }

        void MoveMetric(MetricDefinition metric, int direction)
        {
            var ordered = _metrics.OrderBy(x => x.Order).ToList();
            int index = ordered.IndexOf(metric);

            int target = index + direction;
            if (target < 0 || target >= ordered.Count)
                return;

            // tukar order
            int temp = ordered[target].Order;
            ordered[target].Order = metric.Order;
            metric.Order = temp;

            // simpan ke config
            TopBarWindow.Instance?.SaveMetricsToConfig();

            // rebuild bar LIVE
            TopBarWindow.Instance?.RebuildMetrics();

            // rebuild UI settings (biar urutan list ikut berubah)
            Content = BuildUI();
        }
    }
}