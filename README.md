# Fujimatic

`fujimatic` is an interactive CLI/TUI for tethered control of Fujifilm mirrorless cameras (initial focus: X-T3) to support film scanning and astrophotography workflows. Written in Go with a small C wrapper for the Fujifilm Camera Control SDK.

## Design Goals
- CLI/TUI client to interact with camera
- Full manual control over exposure settings
- Ability to start/pause/stop an imaging session with pre-defined number of frames or integration time
- Session state storage
- Intervalometer with delay feature
- Ability to view camera status, including battery level
- Fake HAL implementation for unit tests (integration tests run manually against real hardware)
- Cross-platform: Windows and Linux (incl. Raspberry Pi) support
- Client/Server model for remote control

---

## Architecture Overview

### Components
- **sdk-c-wrapper (C)** – Minimal API over the Fujifilm SDK.
- **pkg/sdk (Go)** – cgo bindings that expose `Init`, `Connect`, etc.
- **pkg/hal** – Hardware abstraction layer; `Camera` interface + real/fake implementations.
- **pkg/session** – Session manager (handles intervalometer, file naming, persistence).
- **cmd/fujimatic** – CLI shell frontend (REPL).
- **libs/** – SDK redistributables (not committed).
- **scripts/** – build/test utilities.

### Data Flow
User → CLI → Session Manager → HAL → SDK Wrapper → Fujifilm SDK → Camera.

### Config File Example
```yaml
default_outdir: "C:\\Users\\cfonseca\\Pictures\\fujimatic"
last_project_name: "orion_m42"
battery_threshold_percent: 10
sdk_path: "C:\\Users\\cfonseca\\Documents\\fujimatic\\sdk\\REDISTRIBUTABLES"
viewer_cmd: "mpv"
```

---

**Project Structure:**
```
fujimatic/
├── cmd/fujimatic/      # Main CLI
├── pkg/                # Go packages (sdk, hal, session)
├── sdk-c-wrapper/      # C wrapper around Fujifilm SDK
├── libs/               # Compiled C wrapper libraries
├── scripts/            # Build utilities
├── README.md           # This file
├── BUILD_INSTRUCTIONS.md # Build documentation
├── STORIES.md          # Development progress
└── CLAUDE.md           # AI assistant guidance
```

---

## Quick Start

### Build

See **[BUILD_INSTRUCTIONS.md](BUILD_INSTRUCTIONS.md)** for detailed build documentation.

**Basic workflow:**
1. Install Go 1.24.3+ and MSYS2 with MinGW64
2. Obtain Fujifilm Camera Control SDK (not included in repo)
3. Build C wrapper: `gcc -shared -o libs/windows/fmwrapper.dll ...`
4. Build Go app: `go build -o bin/fujimatic.exe ./cmd/fujimatic`
5. Copy DLL: `cp libs/windows/fmwrapper.dll bin/`

---

### Run

```bash
# With real camera
./bin/fujimatic.exe

# With fake camera (for testing)
./bin/fujimatic.exe --fake-camera
```

---

## Features

### Intervalometer with Async Pipeline

Fujimatic supports both synchronous and asynchronous intervalometer modes:

#### Synchronous Mode (Default)
Traditional sequential operation: capture → download → delay → repeat.

```bash
capture 100 10  # 100 frames, 10s delay between frames
```

**Timing**: Each cycle takes `capture_time + download_time + delay`

#### Async Mode (Performance Optimized)
Pipeline architecture: capture frame N+1 while downloading frame N.

```bash
capture --async 100 10  # Same capture, ~20-25% faster
```

**Timing**: Overlapped operations, time ≈ `N × delay + overhead`

#### Performance Comparison (X-T3 Measured)

| Frames | Delay | Sync Time | Async Time | Speedup |
|--------|-------|-----------|------------|---------|
| 5      | 0s    | 13s       | 13s        | 0%      |
| 5      | 1s    | 17s       | 13s        | 23%     |
| 5      | 2s    | 24s       | 18s        | 25%     |
| 100    | 10s   | 500s      | 402s       | 20%*    |

*Projected based on measured capture (~1s) and download (~2s) times.

#### When to Use Async

**Best for:**
- ✅ Astrophotography (long delays between exposures)
- ✅ Time-lapse with delays ≥2 seconds
- ✅ Long capture sessions (100+ frames)

**Use sync for:**
- ⚠️ Single captures or very short delays
- ⚠️ When you need simpler, proven behavior

#### How It Works

**Synchronous Flow:**
```
Frame 1: [Capture 1s] → [Download 2s] → [Delay 10s]
Frame 2: [Capture 1s] → [Download 2s] → [Delay 10s]
Total: 2 × (1 + 2 + 10) = 26 seconds
```

**Async Pipeline Flow:**
```
Frame 1: [Capture 1s] ────────────────┐
Frame 2:              [Capture 1s] ───┼──┐
                                       ↓  ↓
                      [Download 2s] [Download 2s]
         [Delay 10s] [Delay 10s]
Total: ~20 seconds (captures/downloads overlap during delays)
```

### Other Features

- **Manual exposure control**: Set ISO, shutter speed, focus
- **Session management**: Save/load capture sessions with state persistence
- **Battery monitoring**: Auto-pause at configurable threshold (default 10%)
- **Pause/Resume**: Interrupt and continue intervalometer sessions
- **File naming**: Sequential numbering with collision detection

---

### Development Status

See **[STORIES.md](STORIES.md)** for detailed development progress.

# License
MIT License
