# Changelog

## [Unreleased]
### Planned
- GPU metrics stabilization
- Live color picker
- Tray icon
- Startup with Windows
- Click-through mode
- Auto-hide on fullscreen apps
- Multi-profile configuration

---

## [1.0.0] – Initial Public Release
### Added
- Top dock monitoring bar (AppBar, non-overlapping)
- CPU load, temperature, power
- GPU load, temperature, power
- RAM usage & free memory
- Disk free space (C & E)
- Network speed with auto unit scaling
- FPS counter
- Dark / Light theme
- Adjustable bar height & opacity
- Persistent settings
- Metrics enable/disable & reordering
- Settings panel via right-click

### Fixed
- Incorrect CPU temperature source (package vs core distance)
- GPU load misreporting
- Network speed spike on startup
- RAM usage inaccuracies
- Window overlapping fullscreen apps

### Known Issues
- GPU power reporting varies by driver
- VRAM sensors unavailable on some GPUs
- Network speed may fluctuate briefly after resume