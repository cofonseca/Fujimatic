# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**fujimatic** is a CLI/TUI for tethered control of Fujifilm cameras (X-T3) for film scanning and astrophotography. The project uses Go with cgo bindings to a C wrapper around the Fujifilm Camera Control SDK.

**Primary Use Case**: Synchronous capture+download to support **plate-solving** in astrophotography. Images must be immediately available on disk after capture for external processing tools to analyze star positions and calculate telescope pointing accuracy.

**Architecture**: User → CLI → Session Manager → HAL (Hardware Abstraction Layer) → SDK Wrapper → Fujifilm SDK → Camera

**Current Status**: Core functionality complete - All critical hardware integration working with X-T3 camera. See STORIES.md for detailed progress (12/13 stories completed). Ready to implement Epic E (Polishing & Optional Features).

## Development Workflow

**CRITICAL RULE**: Never start working on a new story or feature without asking the user first. Always:
1. Complete current documentation updates
2. Ask the user what to work on next
3. Wait for explicit approval before implementing new features
4. Do not assume the next step - always confirm with the user
5. If you need to create temp files/folders for testing purposes, clean up after yourself at the end

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
- `fm_get_shutter(int* microseconds)` - Get current shutter speed in microseconds
- `fm_set_shutter(int microseconds)` - Set shutter speed in microseconds
- `fm_get_iso(int* iso)` - Get current ISO sensitivity
- `fm_set_iso(int iso)` - Set ISO sensitivity (100-51200)
- `fm_capture()` - Trigger capture
- `fm_download_last(const char* outdir, const char* filename)` - Download image
- `fm_get_supported_shutter_count(int* count)` - Get number of supported shutter speeds
- `fm_get_supported_shutter_speeds(int* count, int* speeds)` - Get list of supported speeds

### HAL Interface Pattern

The HAL uses interface-based abstraction for testability:

```go
type Camera interface {
    Connect() error
    Disconnect() error
    IsConnected() bool
    GetBattery() (int, error)
    GetShutter() (int, error)
    SetShutter(microseconds int) error
    GetISO() (int, error)
    SetISO(iso int) error
    GetSupportedShutterSpeeds() ([]int, error)
    Capture() error
    DownloadLast(outputDir, filename string) error
}
```

**RealCamera** wraps pkg/sdk for production use with real hardware (default).
**FakeCamera** simulates hardware for unit tests and debugging only (use --fake-camera flag).

**For complete project structure, see README.md.**

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

### Code Change Authorization Requirements

**CRITICAL**: Never make code changes without explicit user permission. 

**Rules:**
- **No Code Changes**: Do not edit, modify, or implement code without explicit authorization
- **No Commits**: Do not make git commits or any version control changes
- **Documentation Only**: Reference existing documentation and provide analysis
- **Ask First**: Always request permission before any code work, even if seemingly minor
- **No Assumptions**: Always ask clarifying questions rather than making assumptions.

**When User Asks for Documentation Research:**
- Reference SDK manuals and API documentation only
- Provide analysis and recommendations
- Do not implement or modify code
- Wait for explicit approval before any development work

### Manual Exposure Mode Only

**Design Decision**: Fujimatic is designed for tethered control in film scanning and astrophotography, which requires full manual control. Automatic exposure modes would interfere with the workflow.

**Implementation:**
- Camera is automatically set to Manual exposure mode when connecting
- User cannot select other exposure modes
- All shutter/ISO controls work immediately after connection
- Simplified workflow: connect → control settings → capture

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

**For complete build instructions, see BUILD_INSTRUCTIONS.md. For configuration details, see README.md.**

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

This project follows Scrum methodology with user stories in STORIES.md.

**Current Progress (2025-11-09)**: 12/13 stories completed (92%) - All critical hardware integration and intervalometer functionality complete. Ready to implement Epic E (Polishing & Optional Features).

When implementing features, update STORIES.md with progress notes and completion status.

## Common Gotchas

- **MinGW PATH (Windows)**: **CRITICAL** - Must add `/c/msys64/mingw64/bin` to PATH before building with CGO. Without this, you'll get cryptic "cgo.exe: exit status 2" errors because cc1.exe can't find its DLLs. See BUILD_INSTRUCTIONS.md for details.
- **CGO_ENABLED**: Must be set to 1 for cgo builds (default on native compilation)
- **Library Paths**: cgo LDFLAGS must point to correct libs/ subdirectory for platform
- **C Headers**: fm_wrapper.h must be in sdk-c-wrapper/ directory
- **SDK Path**: Set FUJI_SDK_PATH before running application (runtime requirement, separate from build-time PATH)
- **Connection State**: Always check Camera.IsConnected() before operations
- **Thread Safety**: HAL implementations use mutexes for concurrent access
- **Shutter Speed Formats**: Supports both photographic fractions (1/125, 1/250) and decimals (0.5, 2s)
- **Camera Mode**: Application automatically sets Manual exposure mode before shutter/ISO changes
- **Supported Speeds**: Shutter validation queries camera for supported speeds, uses closest match
- **Build Troubleshooting**: If CGO builds fail mysteriously, first check: (1) Is MinGW in PATH? (2) Does `gcc --version` work? (3) Can GCC compile a simple C file? (4) Run `ldd` on cc1.exe to check DLL dependencies. See BUILD_INSTRUCTIONS.md for comprehensive troubleshooting.

## Hardware Testing

**Real Camera Testing Plan**: See `HARDWARE_TESTING_PLAN.md` for comprehensive testing procedure with X-T3 camera.

**Test Results for C-2 Completion:**
- ✅ SDK initialization
- ✅ Camera connection via USB
- ✅ Battery level reading
- ✅ ISO/shutter read/write functionality (ISO: 100-12800, Shutter: 1/8000s minimum)
- ✅ Complete capture and download workflow
- ✅ Real hardware validation (COMPLETE with X-T3)
- ✅ Enhanced shutter speed support (photographic fractions)
- ✅ Connection stability improvements
- ✅ Smart shutter validation with supported speed lookup

**Current Status**: All hardware integration complete and tested. CLI fully functional with real X-T3 camera. All critical bugs fixed and features working.

## References

- **Project Overview**: README.md
- **Build Instructions**: BUILD_INSTRUCTIONS.md  
- **Story Tracking**: STORIES.md
- **Hardware Testing**: HARDWARE_TESTING_PLAN.md
- **SDK Manual/Instructions**: /sdk/MANUAL and /sdk/SDK_REFERENCE
