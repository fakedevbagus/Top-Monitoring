using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace TopBarDock.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        void OnChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

        // ===== BAR HEIGHT =====
        double _barHeight = 32;
        public double BarHeight
        {
            get => _barHeight;
            set { _barHeight = value; OnChanged(); OnChanged(nameof(BarHeightPercent)); }
        }

        public int BarHeightPercent
        {
            get => (int)(BarHeight / 64 * 100);
            set => BarHeight = Math.Clamp(value, 20, 100) / 100.0 * 64;
        }

        // ===== BACKGROUND COLOR (ARGB) =====
        byte _a = 200, _r = 15, _g = 17, _b = 24;

        public byte BgA { get => _a; set { _a = value; UpdateBrush(); } }
        public byte BgR { get => _r; set { _r = value; UpdateBrush(); } }
        public byte BgG { get => _g; set { _g = value; UpdateBrush(); } }
        public byte BgB { get => _b; set { _b = value; UpdateBrush(); } }

        public int OpacityPercent
        {
            get => (int)(BgA / 255.0 * 100);
            set => BgA = (byte)(Math.Clamp(value, 10, 100) / 100.0 * 255);
        }

        public int BgRPercent
        {
            get => (int)(BgR / 255.0 * 100);
            set => BgR = (byte)(Math.Clamp(value, 0, 100) / 100.0 * 255);
        }

        public int BgGPercent
        {
            get => (int)(BgG / 255.0 * 100);
            set => BgG = (byte)(Math.Clamp(value, 0, 100) / 100.0 * 255);
        }

        public int BgBPercent
        {
            get => (int)(BgB / 255.0 * 100);
            set => BgB = (byte)(Math.Clamp(value, 0, 100) / 100.0 * 255);
        }

        Brush _background = new SolidColorBrush(Color.FromArgb(200, 15, 17, 24));
        public Brush BackgroundBrush
        {
            get => _background;
            private set { _background = value; OnChanged(); }
        }

        void UpdateBrush()
        {
            BackgroundBrush =
                new SolidColorBrush(Color.FromArgb(BgA, BgR, BgG, BgB));
            OnChanged(nameof(OpacityPercent));
            OnChanged(nameof(BgRPercent));
            OnChanged(nameof(BgGPercent));
            OnChanged(nameof(BgBPercent));
        }

        public void ResetColor()
        {
            BgA = 200;
            BgR = 15;
            BgG = 17;
            BgB = 24;
        }

        // ===== METRIC TOGGLES (LEBIH LENGKAP) =====
        public bool ShowCpuLoad { get; set; } = true;
        public bool ShowCpuTemp { get; set; } = true;
        public bool ShowCpuPower { get; set; } = false;
        public bool ShowGpuLoad { get; set; } = true;
        public bool ShowGpuTemp { get; set; } = true;
        public bool ShowGpuPower { get; set; } = false;
        public bool ShowRamUsed { get; set; } = true;
        public bool ShowRamFree { get; set; } = false;
        public bool ShowDiskC { get; set; } = false;
        public bool ShowDiskE { get; set; } = false;
        public bool ShowNet { get; set; } = true;
        public bool ShowTime { get; set; } = true;
    }
}
