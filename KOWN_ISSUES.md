# Known Issues & Limitations

## Hardware Metrics
- GPU Power and VRAM sensors depend on driver & hardware support
- Some CPUs expose multiple temperature sensors; package is preferred
- FPS counter reflects WPF render loop, not game FPS

## Network
- Initial speed spike may occur during first sample
- VPNs may distort per-interface reporting

## UI
- Live metric reordering requires explicit rebuild
- Click-through mode not yet implemented
- Settings window limited to single instance

## Platform
- Windows 10/11 only
- Requires .NET 8
- Admin rights recommended for full sensor access