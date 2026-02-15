# Overlap

Lightweight always-on-top iRacing radar overlay MVP for close proximity awareness.

## Features
- Transparent WPF overlay with topmost behavior.
- Locked mode (click-through) and move/edit mode toggle (`Ctrl+Shift+M`).
- Shows up to 6 closest on-track cars within +/-10 meters.
- Danger highlight with subtle pulse when `abs(deltaMeters) < 2.0`.
- Telemetry poll target >=20Hz, rendering via WPF frame loop (~60fps).
- Overlay position, scale, and opacity persisted to `%AppData%/Overlap/settings.json`.

## Project layout
- `Overlap.sln`
- `src/Overlap.App` - WPF overlay UI and window interop.
- `src/Overlap.Core` - radar math, filtering, settings, telemetry reader abstractions.
- `tests/Overlap.Core.Tests` - unit tests for wrap/delta/danger and filtering.

## Run commands
```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/Overlap.App
```

## Notes
- `IracingSharedMemoryReader` connects gracefully and returns a disconnected frame when iRacing shared memory is unavailable or expected variables are missing.
- MVP parser is safety-first; integrate a full IRSDK var-header parser for production-grade variable extraction.
