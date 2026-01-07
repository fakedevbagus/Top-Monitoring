![Windows](https://img.shields.io/badge/OS-Windows-blue)
![.NET](https://img.shields.io/badge/.NET-8-purple)
![License](https://img.shields.io/badge/license-MIT-green)

# Top Monitoring

> A lightweight, always-on-top system monitoring bar for Windows.
> Displays real-time CPU, GPU, RAM, Disk, Network, and FPS metrics in a clean top dock — inspired by diagnostic sidebars, redesigned for the top of your screen.

---

## ✨ Features

* 🧠 **CPU**

  * Load (%)
  * Temperature (Package)
  * Power (W)

* 🎮 **GPU**

  * Load (%)
  * Temperature (Core)
  * Power (W)
  * VRAM Total (MB)
  * VRAM Used (MB)
  * VRAM Free (MB)

* 🧮 **Memory**

  * RAM Used (%)
  * RAM Free (GB)

* 💾 **Storage**

  * Disk C Free Space
  * Disk E Free Space

* 🌐 **Network**

  * Live download / upload speed
  * Auto scale: KB/s → MB/s

* 🎞 **FPS Counter**

  * Real-time rendering FPS

* 🧩 **UI & UX**

  * Always-on-top top bar (AppBar, non-overlapping)
  * Centered metrics layout
  * Dark / Light theme settings
  * Background theme
  * Adjustable bar height
  * Adjustable opacity (including near-transparent mode)
  * Right-click settings panel
  * Enable / disable metrics individually
  * Reorder metrics (top ↔ bottom)
  * Settings persist between restarts

---

## 🖥 System Requirements

| Requirement  | Status                                                 |
| ------------ | ------------------------------------------------------ |
| OS           | **Windows 10 / 11**                                    |
| Architecture | x64                                                    |
| Runtime      | .NET 8                                                 |
| GPU          | NVIDIA / AMD (LibreHardwareMonitor compatible)         |
| Permissions  | Normal user (Admin recommended for full sensor access) |

> ❌ Windows 7 / 8 / XP are **not supported**
> This project uses modern WPF, .NET 8, and hardware APIs.

---

## 📦 Installation

### Option 1 — Download Release (Recommended)

1. Go to **Releases**
2. Download the latest `.zip`
3. Extract anywhere
4. Run `TopMonitoring.exe`

### Option 2 — Build from Source

```bash
git clone https://github.com/yourusername/Top-Monitoring.git
cd Top-Monitoring
dotnet build -c Release
```

---

## 🚀 Usage

* App starts docked at the **top of the screen**
* Windows automatically adjusts usable screen area (no overlap)
* **Right-click on the bar** to open Settings
* Enable / disable metrics freely
* Reorder metrics and apply instantly
* Close the app from Settings

---

## ⚙ Settings Overview

* **Appearance**

  * Bar height
  * Opacity (0–100%)
  * Dark / Light theme

* **Metrics**

  * Toggle each metric on/off
  * Change display order
  * Settings are saved automatically

---

## 🧠 Technical Notes

* Hardware data powered by **LibreHardwareMonitor**
* RAM uses native Win32 API for accuracy
* Network speed calculated via interface delta sampling
* FPS calculated using WPF `CompositionTarget.Rendering`
* AppBar API ensures:

  * No overlap with fullscreen apps
  * No interference with taskbar
  * Clean window snapping behavior

---

## 📁 Project Structure

```
TopMonitoring/
│
├── Models/
│   ├── AppConfig.cs
│   ├── MetricDefinition.cs
│
├── Services/
│   ├── HardwareMonitor.cs
│   ├── ConfigService.cs
│
├── Views/
│   ├── TopBarWindow.xaml
│   ├── TopBarWindow.xaml.cs
│   ├── SettingsWindow.xaml.cs
│
├── App.xaml
├── TopMonitoring.csproj
└── README.md
```

---

## 📌 Roadmap (Future Ideas)

* System tray icon
* Startup with Windows toggle
* Multiple profiles
* Per-metric color customization
* Plugin-based metrics
* Localization (EN / ID)

---

## 🛡 License

This project is licensed under the **MIT License**.

You are free to:

* Use
* Modify
* Fork
* Distribute

With **no warranty**.

See the [LICENSE](LICENSE) file for details.

---

## 🤝 Credits

* **LibreHardwareMonitor** — hardware sensors
* Windows AppBar API
* Inspired by diagnostic sidebars, reimagined for modern workflows

---

## ⭐ Support

If you find this project useful:

* ⭐ Star the repository
* 🐞 Open issues for bugs
* 💡 Suggest improvements

---

### Final note

This project is built for **daily real-world usage**, not demos.
If you use it, break it, or improve it — that’s exactly what it’s for.