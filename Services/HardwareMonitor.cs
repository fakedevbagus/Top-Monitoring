using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using LibreHardwareMonitor.Hardware;

namespace TopBarDock.Services
{
    public class HardwareMonitor
    {
        private IHardware? selectedGpu;
        public int SelectedGpuIndex { get; private set; } = 0;
        public List<string> AvailableGpus { get; } = new();


        // ===== NETWORK SMOOTHING =====
        private const double NET_ALPHA = 0.15;

        // ===================== LHM =====================
        private readonly Computer computer;

        // ===================== CPU =====================
        public float CpuLoad { get; private set; }
        public float CpuTemp { get; private set; }
        public float CpuPower { get; private set; }

        // ===================== GPU =====================
        public float GpuLoad { get; private set; }
        public float GpuTemp { get; private set; }
        public float GpuPower { get; private set; }
        public float VramUsedMB { get; private set; }
        public float VramTotalMB { get; private set; }

        // ===================== RAM =====================
        public float RamFreeGB { get; private set; }
        public float RamUsedPercent { get; private set; }

        // ===================== DISK =====================
        public float DiskCFreeGB { get; private set; }
        public float DiskEFreeGB { get; private set; }

        // ===================== NETWORK =====================
        
        public double NetDownKB { get; private set; }
        public double NetUpKB { get; private set; }

        private long lastRx, lastTx;
        private DateTime lastNetTime = DateTime.Now;

        private DateTime lastFrame = DateTime.Now;
        public float Fps { get; private set; }

        public void UpdateFps()
        {
            var now = DateTime.Now;
            var delta = (now - lastFrame).TotalSeconds;

            if (delta > 0)
                Fps = (float)(1.0 / delta);

            lastFrame = now;
        }

        // ===================== CTOR =====================
        public HardwareMonitor()
        {
            computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsStorageEnabled = true,
                IsNetworkEnabled = true
            };
            computer.Open();

            if (selectedGpu != null)
            {
                selectedGpu.Update();

                foreach (var s in selectedGpu.Sensors)
                {
                    // GPU LOAD
                    if (s.SensorType == SensorType.Load &&
                        s.Name.Contains("Core", StringComparison.OrdinalIgnoreCase))
                        GpuLoad = s.Value ?? 0;

                    // GPU TEMP
                    if (s.SensorType == SensorType.Temperature &&
                        s.Name.Contains("Core", StringComparison.OrdinalIgnoreCase))
                        GpuTemp = s.Value ?? 0;

                    // GPU POWER
                    if (s.SensorType == SensorType.Power &&
                        s.Name.Contains("Core", StringComparison.OrdinalIgnoreCase))
                        GpuPower = s.Value ?? 0;

                    // VRAM USED
                    if (s.SensorType == SensorType.SmallData &&
                        s.Name.Contains("Memory Used", StringComparison.OrdinalIgnoreCase))
                        VramUsedMB = s.Value ?? 0;
                }
            }

            SelectGpu(0);

        }

        public void SelectGpu(int index)
        {
            int i = 0;
            foreach (var hw in computer.Hardware)
            {
                if (hw.HardwareType == HardwareType.GpuNvidia ||
                    hw.HardwareType == HardwareType.GpuAmd ||
                    hw.HardwareType == HardwareType.GpuIntel)
                {
                    if (i == index)
                    {
                        selectedGpu = hw;
                        SelectedGpuIndex = index;
                        break;
                    }
                    i++;
                }
            }
        }

        // ===================== UPDATE =====================
        public void Update()
        {
            UpdateCpu();
            UpdateGpu();
            UpdateRam();
            UpdateDisk();
            UpdateNetwork();
            UpdateFps();
        }

        // ===================== CPU =====================
        private void UpdateCpu()
        {
            foreach (var hw in computer.Hardware)
            {
                if (hw.HardwareType != HardwareType.Cpu)
                    continue;

                hw.Update();

                foreach (var s in hw.Sensors)
                {
                    if (s.Value == null) continue;

                    if (s.SensorType == SensorType.Load &&
                        s.Name.Contains("Total"))
                        CpuLoad = s.Value.Value;

                    if (s.SensorType == SensorType.Temperature &&
                        s.Name.Contains("Package"))
                        CpuTemp = s.Value.Value;

                    if (s.SensorType == SensorType.Power &&
                        s.Name.Contains("Package"))
                        CpuPower = s.Value.Value;
                }
            }
        }

        // ===================== GPU =====================
        private void UpdateGpu()
        {
        GpuPower = 0;
        VramUsedMB = 0;
        VramTotalMB = 0;

        foreach (var hw in computer.Hardware)
        {
            if (hw.HardwareType != HardwareType.GpuNvidia &&
                hw.HardwareType != HardwareType.GpuAmd)
                continue;

            hw.Update();

            foreach (var s in hw.Sensors)
            {
                // GPU LOAD
                if (s.SensorType == SensorType.Load &&
                    s.Name.Equals("GPU Core", StringComparison.OrdinalIgnoreCase))
                {
                    GpuLoad = s.Value ?? 0;
                }

                // GPU TEMP
                if (s.SensorType == SensorType.Temperature &&
                    s.Name.Contains("Core", StringComparison.OrdinalIgnoreCase))
                {
                    GpuTemp = s.Value ?? 0;
                }

                // GPU POWER (FINAL)
                if (s.SensorType == SensorType.Power &&
                    s.Name.Contains("GPU Core", StringComparison.OrdinalIgnoreCase))
                {
                    GpuPower = s.Value ?? 0;
                }

                // VRAM USED
                if (s.SensorType == SensorType.SmallData &&
                    s.Name.Contains("Memory Used", StringComparison.OrdinalIgnoreCase))
                {
                    VramUsedMB = s.Value ?? 0;
                }

                // VRAM TOTAL
                if (s.SensorType == SensorType.SmallData &&
                    s.Name.Contains("Memory Total", StringComparison.OrdinalIgnoreCase))
                {
                    VramTotalMB = s.Value ?? 0;
                }
            }
        }

        }

        // ===================== RAM =====================
        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [DllImport("kernel32.dll")]
        private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        private void UpdateRam()
        {
            MEMORYSTATUSEX mem = new MEMORYSTATUSEX();
            mem.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));

            if (!GlobalMemoryStatusEx(ref mem))
                return;

            float total = mem.ullTotalPhys / (1024f * 1024 * 1024);
            float free  = mem.ullAvailPhys / (1024f * 1024 * 1024);

            RamFreeGB = free;
            RamUsedPercent = ((total - free) / total) * 100f;
        }

        // ===================== DISK =====================
        private void UpdateDisk()
        {
            try
            {
                var c = new DriveInfo("C");
                if (c.IsReady)
                    DiskCFreeGB = c.AvailableFreeSpace / (1024f * 1024 * 1024);
            }
            catch { DiskCFreeGB = float.NaN; }

            try
            {
                var e = new DriveInfo("E");
                if (e.IsReady)
                    DiskEFreeGB = e.AvailableFreeSpace / (1024f * 1024 * 1024);
            }
            catch { DiskEFreeGB = float.NaN; }
        }

        // ===================== NETWORK =====================
        private const int NET_SMOOTH = 5;
        private readonly Queue<double> downSamples = new();
        private readonly Queue<double> upSamples = new();

        private bool netInitialized = false;
        private double smoothDown = 0;
        private double smoothUp = 0;

        private void UpdateNetwork()
        {
            var now = DateTime.Now;
            var dt = (now - lastNetTime).TotalSeconds;
            if (dt <= 0) return;

            long rx = 0, tx = 0;

            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up)
                    continue;

                var stat = nic.GetIPv4Statistics();
                rx += stat.BytesReceived;
                tx += stat.BytesSent;
            }

            if (!netInitialized)
            {
                lastRx = rx;
                lastTx = tx;
                lastNetTime = now;
                netInitialized = true;
                NetDownKB = 0;
                NetUpKB = 0;
                return;
            }

            var down = (rx - lastRx) / 1024.0 / dt;
            var up   = (tx - lastTx) / 1024.0 / dt;

            smoothDown = smoothDown * 0.85 + down * 0.15;
            smoothUp   = smoothUp   * 0.85 + up   * 0.15;

            NetDownKB = smoothDown;
            NetUpKB   = smoothUp;

            lastRx = rx;
            lastTx = tx;
            lastNetTime = now;
        }
    }
}


