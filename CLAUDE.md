# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**fujimatic** is a CLI/TUI for tethered control of Fujifilm cameras (X-T3) for film scanning and astrophotography. The project uses Go with cgo bindings to a C wrapper around the Fujifilm Camera Control SDK.

**Primary Use Case**: Synchronous capture+download to support **plate-solving** in astrophotography. Images must be immediately available on disk after capture for external processing tools to analyze star positions and calculate telescope pointing accuracy.

**Architecture**: User → CLI → Session Manager → HAL (Hardware Abstraction Layer) → SDK Wrapper → Fujifilm SDK → Camera

**Current Status**: Early development - Epic A and B completed (Foundation, SDK Binding, HAL, Session, CLI). Track progress in STORIES.md (6/13 stories completed). Currently working on Epic C (Hardware Integration & Real Camera).

## MVP Scope

The minimal viable product focuses on essential tethered shooting capabilities:

1. **Camera Control**: Connect/disconnect via USB, set shutter speed, capture RAW (.RAF) images
2. **Immediate Download**: Images downloaded synchronously to project directory (critical for plate-solving)
3. **Intervalometer**: Start/pause/resume/stop with configurable delays and integration-time calculations
4. **Battery Safety**: Monitor battery level, auto-pause at 10% threshold
5. **Dual Modes**: Interactive shell (REPL) + `--non-interactive` for scripting
6. **Configuration**: YAML config for defaults (output dir, battery threshold, SDK path)

**Minimal API Philosophy**: Only 7 core functions in C wrapper (init, connect, disconnect, getBattery, setShutter, capture, downloadLast). Keep scope tight for MVP.

## Build Commands

### C Wrapper Build

**Windows:**
```cmd
scripts\build_wrapper.bat
```
Outputs to `libs\windows\fmwrapper.dll`

**Linux:**
```bash
scripts/build_wrapper.sh
```
Outputs to `libs/linux/libfmwrapper.so`

### Go Application Build

**Standard build:**
```cmd
go build -o bin\fujimatic.exe ./cmd/fujimatic
```

**Run tests:**
```cmd
go test ./...
```

**Run specific package tests:**
```cmd
go test ./pkg/hal
go test ./pkg/sdk
```

**Cross-compile for Raspberry Pi:**
```bash
GOARCH=arm64 GOOS=linux go build -o bin/fujimatic ./cmd/fujimatic
```

### Environment Setup

The C wrapper links against Fujifilm SDK redistributables. Set the SDK path:

**Windows:**
```powershell
setx FUJI_SDK_PATH "C:\Users\cfonseca\Documents\fujimatic\sdk\REDISTRIBUTABLES"
```

**Linux:**
```bash
export FUJI_SDK_PATH=/path/to/sdk/redist
```

### Build Requirements

**Compilers:**
- **Windows**: MSVC (Microsoft Visual C++ compiler, `cl` command)
- **Linux**: GCC (GNU Compiler Collection)

**Dependencies:**
- Go 1.24.3+ (as specified in go.mod)
- C compiler (MSVC/GCC)
- Fujifilm Camera Control SDK (obtained separately, not in repo)
- YAML library (TBD - for config file parsing)

## Architecture & Code Organization

### Three-Layer Architecture

1. **pkg/sdk** - cgo bindings to C wrapper
   - Direct interface to fm_wrapper C functions
   - Handles C string conversion and memory management
   - Returns Go-friendly errors and types
   - Camera struct tracks connection state

2. **pkg/hal** - Hardware abstraction layer
   - Camera interface defines all camera operations
   - RealCamera wraps pkg/sdk for production use (default)
   - FakeCamera for testing/debugging only (use --fake-camera flag)
   - Thread-safe with mutex protection
   - Real implementation uses cgo build tag

3. **pkg/session** - Session management (✅ implemented)
   - Handles file naming and sequence tracking
   - JSON persistence for pause/resume
   - Integration point for intervalometer logic
   - Integration point for battery monitoring

### Data Flow Example

```
CLI Command "capture"
  → Session Manager (tracks sequence)
    → HAL Camera.Capture()
      → SDK Camera.Capture() [cgo]
        → C fm_capture()
          → Fujifilm SDK API
            → Physical Camera
```

### C Wrapper API

All C functions return `int` status codes (0 = success):

- `fm_init(const char* sdk_path)` - Initialize SDK
- `fm_connect()` - Connect to camera
- `fm_disconnect()` - Disconnect camera
- `fm_get_battery(int* percent)` - Get battery level
- `fm_set_shutter(int seconds)` - Set shutter speed
- `fm_capture()` - Trigger capture
- `fm_download_last(const char* outdir, const char* filename)` - Download image

### HAL Interface Pattern

The HAL uses interface-based abstraction for testability:

```go
type Camera interface {
    Connect() error
    Disconnect() error
    IsConnected() bool
    GetBattery() (int, error)
    SetShutter(seconds int) error
    Capture() error
    DownloadLast(outputDir, filename string) error
}
```

**RealCamera** wraps pkg/sdk for production use with real hardware (default).
**FakeCamera** simulates hardware for unit tests and debugging only (use --fake-camera flag).

## File Structure

```
fujimatic/
├── cmd/fujimatic/          # Main CLI entry point
├── pkg/
│   ├── sdk/               # cgo bindings to C wrapper (✅ implemented)
│   ├── hal/               # Hardware abstraction layer (✅ implemented)
│   └── session/           # Session manager (not yet implemented)
├── sdk-c-wrapper/          # C wrapper around Fujifilm SDK (✅ implemented)
│   ├── fm_wrapper.h
│   └── fm_wrapper.c
├── sdk/                    # Fujifilm SDK files (not in repo)
│   ├── HEADERS/           # Camera Control API headers
│   ├── REDISTRIBUTABLES/  # Platform-specific SDK libraries (.dll, .so)
│   ├── SAMPLES/           # SDK example code
│   └── MANUAL/            # SDK documentation
├── libs/                   # Compiled wrapper libraries
│   ├── windows/           # fmwrapper.dll
│   └── linux/             # libfmwrapper.so
├── scripts/               # Build utilities
│   ├── build_wrapper.bat  # Windows C wrapper build
│   └── build_wrapper.sh   # Linux C wrapper build
├── STORIES.md            # User story tracking (Scrum methodology)
└── README.md             # Project documentation
```

## Naming Conventions

**Consistency across layers:**
- **Project name**: lowercase (`fujimatic`)
- **C functions**: `fm_*` prefix (e.g., `fm_init`, `fm_connect`, `fm_capture`)
- **Go packages**: lowercase, no underscores (`sdk`, `hal`, `session`)
- **Go functions**: PascalCase for exported (e.g., `Init`, `Connect`, `GetBattery`)
- **Config keys**: snake_case (e.g., `default_outdir`, `battery_threshold_percent`)
- **Story IDs**: Letter-Number format (e.g., `A-1`, `B-2`, `C-3`)

## Testing Strategy

### Unit Tests
- Use FakeCamera from pkg/hal for testing without hardware
- Mock implementations return realistic test data
- Battery simulation drains 1% per capture
- No external dependencies required

### Integration Tests
- Use environment variable pattern (NOT build tags)
- Skip tests when required environment variables are not set
- Example: `DB_ADDR`, `CAMERA_CONNECTED`, etc.

```go
func TestIntegrationCapture(t *testing.T) {
    if os.Getenv("CAMERA_CONNECTED") == "" {
        t.Skip("set CAMERA_CONNECTED to run this test")
    }
    // Test with real hardware
}
```

## Development Patterns

### cgo Memory Management
- Always free C strings with `defer C.free(unsafe.Pointer(cStr))`
- Convert Go strings to C: `C.CString(goString)`
- Convert C ints to Go: `int(cInt)`
- Prevent memory leaks in all SDK wrapper functions

### Error Handling
- SDK functions return descriptive Go errors
- Include C error codes in error messages for debugging
- Check connection state before operations
- Validate inputs (empty strings, negative values)

### Camera Mode Selection
- **Default**: RealCamera (production use with real hardware)
- **Testing/Debugging**: FakeCamera (use `--fake-camera` flag)
- CLI flag: `./fujimatic --fake-camera` to use simulated camera
- FakeCamera useful for development without hardware attached
- Normal users will never need the --fake-camera flag

### Build Tags
- RealCamera uses `// +build cgo` to conditionally compile
- FakeCamera always available for testing
- Allows testing without C compiler or SDK

## Configuration

Expected YAML config file format (not yet implemented):

```yaml
default_outdir: "C:\\Users\\cfonseca\\Pictures\\fujimatic"
last_project_name: "orion_m42"
battery_threshold_percent: 10
sdk_path: "C:\\Users\\cfonseca\\Documents\\fujimatic\\sdk\\REDISTRIBUTABLES"
viewer_cmd: "mpv"
```

**Config Key Explanations:**
- `default_outdir`: Base directory for captured images
- `last_project_name`: Last used project/session name (for quick resume)
- `battery_threshold_percent`: Auto-pause threshold when battery drops below this level (default: 10%)
- `sdk_path`: Path to Fujifilm SDK redistributables directory
- `viewer_cmd`: External viewer command for live view (e.g., `mpv` or `vlc`)

**Session Persistence**: Uses JSON format to save/restore intervalometer state for pause/resume functionality. Includes frame count, sequence numbers, project name, and capture settings.

## Important Constraints

1. **Synchronous MVP**: First version MUST use synchronous capture+download for plate-solving workflow. Images must be on disk immediately after capture so external tools can analyze them. Async implementation comes later (Epic E).
2. **Minimal API Surface**: Keep C wrapper to exactly 7 functions (init, connect, disconnect, getBattery, setShutter, capture, downloadLast). Don't add extra SDK features until MVP complete.
3. **cgo Required**: Building requires C compiler (MSVC on Windows, GCC on Linux) and CGO_ENABLED=1
4. **SDK Licensing**: Fujifilm SDK redistributables NOT in repo - must obtain separately from Fujifilm
5. **Cross-platform Paths**: Windows uses backslashes, Linux uses forward slashes in file paths
6. **Battery Safety**: Auto-pause at configurable threshold (default 10%) to protect camera battery
7. **RAW Format Only**: Captures .RAF files only (Fujifilm RAW format)
8. **Platform Support**: windows-x86_64, linux-x86_64, linux-arm64 (Raspberry Pi for remote observatory use)

## Story Tracking

This project follows Scrum methodology with user stories in STORIES.md:

- **Epic A**: Foundation & SDK Binding (✅ 3/3 completed)
- **Epic B**: HAL, Shell & Basic Workflows (✅ 3/3 completed)
- **Epic C**: Hardware Integration & Real Camera (0/3)
- **Epic D**: Intervalometer & Battery Handling (0/3)
- **Epic E**: Polishing & Optional Features (0/3)

**Epic Order Rationale**: Epics C and D were swapped to prioritize hardware validation before building advanced intervalometer features. This allows earlier testing with real camera hardware and easier debugging.

When implementing features, update STORIES.md with progress notes and completion status.

## Next Development Priorities

Based on STORIES.md, the next stories to implement are:

1. **C-1**: RealCamera Implementation - integrate real Fujifilm SDK via cgo bindings, test with X-T3 hardware
2. **C-2**: Full Capture & Download - test complete workflow with real camera (SetShutter, Capture, DownloadLast)
3. **C-3**: Integration Test Script - automated PowerShell/Bash test script for hardware validation

## Common Gotchas

- **CGO_ENABLED**: Must be set to 1 for cgo builds (default on native compilation)
- **Library Paths**: cgo LDFLAGS must point to correct libs/ subdirectory for platform
- **C Headers**: fm_wrapper.h must be in sdk-c-wrapper/ directory
- **SDK Path**: Set FUJI_SDK_PATH before running application
- **Connection State**: Always check Camera.IsConnected() before operations
- **Thread Safety**: HAL implementations use mutexes for concurrent access
