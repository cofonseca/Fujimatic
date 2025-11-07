# CRUSH.md - Fujimatic Development Guide

## Project Overview

**fujimatic** is an interactive CLI/TUI for tethered control of Fujifilm mirrorless cameras (initial focus: X-T3) to support film scanning and astrophotography workflows. Written in Go with a small C wrapper for the Fujifilm Camera Control SDK.

**Current Status**: Early prototype stage - most functionality is stub/mock implementations.

## Architecture

### Components
- **sdk-c-wrapper (C)** – Minimal API over the Fujifilm SDK
- **pkg/sdk (Go)** – cgo bindings (not yet implemented)
- **pkg/hal** – Hardware abstraction layer (not yet implemented)
- **pkg/session** – Session manager (not yet implemented)
- **cmd/fujimatic** – CLI shell frontend (basic REPL implemented)

### Data Flow
User → CLI → Session Manager → HAL → SDK Wrapper → Fujifilm SDK → Camera

## Project Structure

```
fujimatic/
├── cmd/fujimatic/          # Main entry point
├── sdk-c-wrapper/          # C wrapper for Fuji SDK
├── sdk/                    # Fujifilm SDK redistributables and headers
│   ├── HEADERS/           # Camera API headers
│   ├── REDISTRIBUTABLES/  # Platform-specific SDK libraries
│   ├── SAMPLES/           # SDK sample code
│   └── MANUAL/            # SDK documentation
├── scripts/               # Build utilities
└── README.md             # Project documentation
```

## Build Commands

### C Wrapper Build

**Windows:**
```cmd
scripts\build_wrapper.bat
```
- Requires MSVC compiler (`cl`)
- Outputs to `libs\windows\fmwrapper.dll`

**Linux/Raspberry Pi:**
```bash
scripts/build_wrapper.sh
```
- Requires GCC
- Outputs to `libs/linux/libfmwrapper.so`

### Go Application Build

**Current Status**: No go.mod exists yet, will need to be created.

**Planned Command:**
```cmd
go build -o bin\fujimatic.exe ./cmd/fujimatic
```

**Cross-compilation for ARM64:**
```bash
GOARCH=arm64 GOOS=linux go build -o bin/fujimatic ./cmd/fujimatic
```

### Environment Setup

**Windows:**
```powershell
setx FUJI_SDK_PATH "C:\Users\cfonseca\Documents\fujimatic\sdk\REDISTRIBUTABLES"
```

**Linux:**
```bash
export FUJI_SDK_PATH=/home/pi/fujimatic/sdk/redist
```

## Development Status

### Implemented
- [x] Project structure and documentation
- [x] Basic C wrapper with mock functions
- [x] Simple Go REPL prototype (`cmd/fujimatic/main.go`)
- [x] Build scripts for C wrapper
- [x] Fujifilm SDK headers and redistributables

### Not Yet Implemented
- [ ] Go module setup (`go.mod`)
- [ ] CGO bindings (`pkg/sdk/`)
- [ ] Hardware abstraction layer (`pkg/hal/`)
- [ ] Session manager (`pkg/session/`)
- [ ] Configuration management (YAML)
- [ ] Real camera integration
- [ ] Battery monitoring
- [ ] Intervalometer functionality
- [ ] File download and management

## Code Patterns and Conventions

### Current Go Code Style
- Uses standard Go formatting
- Simple CLI with switch-based command handling
- `bufio.NewReader` for input reading
- Basic error handling (ignoring errors with `_`)

### C Wrapper API
All C functions return `int` status codes:
- `0` = Success
- Non-zero = Error code

**Core Functions:**
- `fm_init(const char* sdk_path)` - Initialize SDK
- `fm_connect()` - Connect to camera
- `fm_disconnect()` - Disconnect camera
- `fm_get_battery(int* percent)` - Get battery level
- `fm_set_shutter(int seconds)` - Set shutter speed
- `fm_capture()` - Trigger capture
- `fm_download_last(const char* outdir, const char* filename)` - Download last image

### Naming Conventions
- **Project**: lowercase with underscores (`fujimatic`)
- **C Functions**: `fm_*` prefix (fm_init, fm_connect, etc.)
- **Go Packages**: lowercase (`sdk`, `hal`, `session`)
- **Config Keys**: snake_case (`default_outdir`, `battery_threshold_percent`)

## Configuration

### Expected Config File Format (YAML)
```yaml
default_outdir: "C:\\Users\\cfonseca\\Pictures\\fujimatic"
last_project_name: "orion_m42"
battery_threshold_percent: 10
sdk_path: "C:\\Users\\cfonseca\\Documents\\fujimatic\\sdk\\REDISTRIBUTABLES"
viewer_cmd: "mpv"
```

### Session Persistence
- Session data stored as JSON
- Supports pause/resume functionality
- File naming and sequence tracking

## Testing Approach

### Unit Tests
- Will use fake HAL implementation for unit tests
- Mock C functions for Go testing

### Integration Tests
- Manual testing against real hardware
- PowerShell script for automation (planned)

## Platforms Supported
- **windows-x86_64** (primary development platform)
- **linux-x86_64**
- **linux-arm64** (Raspberry Pi support)

## Important Gotchas

1. **CGO Requirement**: Go code must link with C wrapper, requires proper compiler setup
2. **SDK Path**: Must set `FUJI_SDK_PATH` environment variable pointing to redistributables
3. **Cross-compilation**: ARM64 builds need `GOARCH=arm64 GOOS=linux`
4. **File Paths**: Windows uses backslashes, Linux uses forward slashes
5. **SDK Licensing**: Redistributables not committed to repo, must be obtained separately
6. **Current State**: Most functionality is mocked/stubbed, not yet functional

## Next Development Steps

1. Initialize Go module: `go mod init github.com/cfonseca/fujimatic`
2. Implement CGO bindings in `pkg/sdk/`
3. Create HAL interface and fake implementation in `pkg/hal/`
4. Build session manager in `pkg/session/`
5. Enhance CLI with real commands
6. Add configuration file support
7. Implement real camera integration

## Dependencies

- **Go** (version TBD)
- **C Compiler** (MSVC for Windows, GCC for Linux)
- **Fujifilm Camera Control SDK** (obtained separately)
- **YAML library** (for config, TBD which one)

## Development Notes

- Author: Corey Fonseca
- License: MIT
- Version: 0.1.0
- Focus on synchronous capture+download for MVP to support plate-solving
- Prioritize minimal APIs: init, connect, disconnect, getBattery, setShutter, capture, downloadLast