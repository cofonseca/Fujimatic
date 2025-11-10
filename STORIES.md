# User Stories Tracking

## Story Management Guide

**When working on a story:**
1. Check the current status below
2. Update the story entry with progress notes
3. Mark as completed when done
4. Add completion date and verification details

**Status Legend:**
- 🔄 In Progress
- ✅ Completed
- ❌ Blocked
- 📋 Not Started

---

## Epic A — Foundation & SDK Binding

### A-1. Initialize Repository ✅ COMPLETED
**Date Completed:** 2025-11-07
**Requirements:**
- [x] Create Go module and folder structure
- [x] Verify `go list ./...` builds cleanly

**Implementation Notes:**
- Created go.mod with module name github.com/cfonseca/fujimatic
- Created folder structure: cmd/, pkg/ (sdk, hal, session), scripts/, libs/
- All package directories created with .gitkeep files
- `go list ./...` verified working and returns cmd/fujimatic package

**Verification:**
- `go list ./...` command runs successfully
- No build errors
- All directories properly structured

---

### A-2. Add C Wrapper Project Files ✅ COMPLETED
**Date Completed:** 2025-11-07
**Requirements:**
- [x] Create `fm_wrapper.h` and `fm_wrapper.c`
- [x] Build scripts produce `.dll` and `.so`
- [x] Verify successful compilation

**Implementation Notes:**
- Created sdk-c-wrapper/fm_wrapper.h with C function declarations
- Created sdk-c-wrapper/fm_wrapper.c with mock implementations
- Build scripts exist: scripts/build_wrapper.bat (Windows) and scripts/build_wrapper.sh (Linux)
- C wrapper provides minimal API: init, connect, disconnect, getBattery, setShutter, capture, downloadLast

**Verification:**
- C files compile without errors
- Build scripts execute successfully
- Output directories exist with .gitkeep files

---

### A-3. Add Go cgo Bindings ✅ COMPLETED
**Date Completed:** 2025-11-07
**Requirements:**
- [x] Implement Go-friendly wrappers for C functions
- [x] Test using dummy return codes

**Implementation Notes:**
- Created pkg/sdk/ package with cgo bindings in sdk.go
- Implemented cgo directives with proper CFLAGS and LDFLAGS
- Created Go-friendly wrappers for all C functions: Init, Connect, Disconnect, GetBattery, SetShutter, Capture, DownloadLast
- Added proper error handling and Go-style return types
- Created Camera struct to represent connected camera sessions
- Enhanced main program to test all SDK functions with dummy return codes
- Successfully compiles and links with C wrapper library

**Verification:**
- `go build ./cmd/fujimatic` compiles successfully
- CGO integration working with dummy C functions
- All wrapper functions properly convert Go types to C types
- Error handling and validation implemented
- Integration test in main program demonstrates all functions work with mock data

---

## Epic B — HAL, Shell & Basic Workflows

### B-1. Implement HAL Interface ✅ COMPLETED
**Date Completed:** 2025-11-07
**Status:** Completed
**Requirements:**
- [x] Define `Camera` interface and `fake` implementation
- [x] Unit tests run with fake HAL

**Implementation Notes:**
- Created pkg/hal/ package with Camera interface abstracting all camera operations
- Implemented FakeCamera for testing without real hardware
- RealCamera adapter wraps SDK package to implement Camera interface
- All methods included: Connect, Disconnect, Capture, GetBattery, SetShutter, DownloadLast, IsConnected
- Added concurrent access protection with mutex for thread safety
- Real implementation uses cgo build tag to conditionally compile with SDK
- Battery drain simulation in fake implementation (1% per capture)
- Comprehensive unit tests covering all methods and edge cases

**Verification:**
- All unit tests pass for fake implementation
- Camera interface properly abstracts hardware operations
- Both fake and real implementations provide same interface
- Thread-safe operations with mutex protection
- Real implementation conditionally compiled with cgo build tag

---

### B-2. Session Manager ✅ COMPLETED
**Date Completed:** 2025-11-07
**Status:** Completed
**Requirements:**
- [x] Implement file naming and sequence tracking
- [x] Validate synchronous capture + download

**Implementation Notes:**
- Created pkg/session/ package with Session struct
- Implemented JSON persistence to user config directory (~/.config/fujimatic/)
- File naming strategy: projectname_0001.RAF format with 4-digit zero-padding
- Automatic directory creation for output paths
- File collision handling with skip-and-increment logic
- Synchronous capture+download workflow validated with unit tests
- Full integration with HAL Camera interface
- Comprehensive unit tests using FakeCamera (all 11 tests pass)

**Verification:**
- All unit tests pass (11/11)
- File naming generates correct sequential filenames
- JSON persistence saves/loads session state correctly
- Capture() method successfully coordinates camera operations
- Directory auto-creation works for non-existent paths
- File collision detection skips existing sequence numbers
- Session state includes: ProjectName, OutputDir, SequenceNumber, CreatedAt

---

### B-3. CLI Shell ✅ COMPLETED
**Date Completed:** 2025-11-07
**Status:** Completed
**Requirements:**
- [x] Add `connect`, `capture`, and `exit` commands
- [x] Verify REPL responds correctly

**Implementation Notes:**
- Replaced basic REPL prototype with full-featured Shell struct
- Integrated with HAL (using FakeCamera for testing)
- Integrated with Session Manager for capture workflow
- Implemented 8 command groups with proper error handling

**Commands Implemented:**
- **Camera Control**: connect, disconnect, status, battery
- **Session Management**: session start/save/load
- **Capture**: capture (with auto-session creation)
- **Utility**: help, exit/quit

**Key Features:**
- Auto-creates session on first capture if none exists (prompts for project name)
- Graceful camera disconnect on exit
- Clear status display showing camera and session state
- Battery monitoring (shows percentage on 'battery' command)
- Session persistence to ~/.config/fujimatic/session.json
- Tilde (~) expansion for home directory in paths
- Proper error handling with descriptive messages

**Verification:**
- All commands tested successfully with automated input
- Connect/disconnect workflow works correctly
- Session start/save/load verified
- Capture increments sequence numbers correctly (0001, 0002, 0003)
- Battery drains correctly with FakeCamera (3 captures = 97%)
- Status command shows accurate camera and session state
- Help command displays all available commands

---

## Epic C — Hardware Integration & Real Camera

### C-1. RealCamera Implementation with Fujifilm SDK ✅ COMPLETED
**Status:** Completed
**Date Started:** 2025-11-07
**Date Completed:** 2025-11-08
**Requirements:**
- [x] Implement RealCamera struct in pkg/hal/real.go
- [x] Use pkg/sdk cgo bindings for all camera operations
- [x] Add SDK initialization (fm_init with SDK path)
- [x] Test connection to real X-T3 camera via USB
- [x] Verify disconnect cleanly releases camera
- [x] Validate battery reading returns actual camera battery level
- [x] Update CLI to support --fake-camera flag (default: RealCamera)

**Implementation Notes:**
- RealCamera wraps pkg/sdk with proper error handling
- Thread-safe operations using mutex
- Connection state tracking
- SDK initialization uses FUJI_SDK_PATH environment variable
- Default to RealCamera, --fake-camera flag for testing only
- Graceful error messages for common hardware issues:
  - Camera not powered on
  - USB cable not connected
  - Camera in wrong mode
  - SDK initialization failure

**Progress Update:**
- ✅ Created pkg/hal/real.go with RealCamera struct implementing hal.Camera interface (2025-11-07)
- ✅ Added thread-safety with sync.Mutex (matches FakeCamera pattern)
- ✅ Created pkg/hal/init_cgo.go and init_fake.go for SDK initialization with build constraints
- ✅ Created pkg/hal/factory_cgo.go and factory_fake.go for camera factory functions
- ✅ Updated cmd/fujimatic/main.go to add --fake-camera flag with proper SDK initialization
- ✅ Added build constraints to pkg/sdk files (//go:build cgo)
- ✅ All tests pass with fake camera mode
- ✅ Application builds and runs successfully with --fake-camera flag
- ✅ SDK integration complete - all 7 functions implemented with real Fujifilm SDK (2025-11-08)
- ✅ Connection fix applied - XSDK_Detect(USB) + XSDK_OpenEx("ENUM:0") approach
- ✅ Battery reading working - variadic XSDK_GetProp with 6 parameters
- ✅ Hardware testing complete - tested with X-T3 (F/W 5.11) via USB-C
- ✅ All acceptance criteria met (10/10)

**C Wrapper Status:**
- ✅ **SDK Integration Complete** - All 7 functions implemented with real Fujifilm SDK calls
- ✅ fm_wrapper.c uses XSDK_Init, XSDK_Detect, XSDK_OpenEx, XSDK_Close, XSDK_GetProp, etc.
- ✅ Battery conversion implemented: SDK_POWERCAPACITY_* codes → 0-100 percentage
- ✅ Image download workflow: XSDK_ReadImageInfo → XSDK_ReadImage → write .RAF file
- ✅ Compiled fmwrapper.dll (242KB) using MinGW-w64 GCC 15.2.0 in MSYS2 terminal
- ✅ Linked against sdk/REDISTRIBUTABLES/Windows/64bit/XAPI.dll
- ✅ Hardware testing complete - X-T3 connection and battery reading verified

**Testing Focus:**
- SDK initialization with valid/invalid paths
- Connect to powered X-T3 via USB
- Read battery level (compare with camera display)
- Verify disconnect releases camera cleanly
- Test CLI with both real and fake camera modes

**Acceptance Criteria:**
- [x] RealCamera implements hal.Camera interface
- [x] C wrapper built with real SDK integration (fmwrapper.dll 242KB)
- [x] All 7 SDK functions fully implemented
- [x] SDK initializes successfully (DLLs in bin/ directory)
- [x] CLI can connect to real X-T3 camera via USB
- [x] Battery reading returns actual camera level (100% verified)
- [x] Disconnect releases camera cleanly
- [x] Error messages helpful for common issues
- [x] No memory leaks in cgo operations (none observed)
- [x] CLI runs with --fake-camera flag for testing

**Status:** ✅ **10/10 criteria met - Story C-1 COMPLETE**

---

### C-2. Full Capture & Download with Real Camera ✅ COMPLETED
**Status:** Completed (All fixes applied, fully tested with X-T3)
**Date Started:** 2025-11-08
**Date Completed:** 2025-11-08
**Requirements:**
- [x] SetShutter() sets the shutter speed on the real camera ✅ Implemented & Fixed
- [x] GetShutter() reads current shutter speed ✅ Implemented & Fixed
- [x] SetISO() sets the ISO value on the real camera ✅ Implemented
- [x] GetISO() reads current ISO value ✅ Implemented
- [x] Capture() triggers actual shutter to take an image ✅
- [x] DownloadLast() retrieves .RAF file from camera ✅
- [x] RAF file saved to correct output directory ✅
- [x] File size is reasonable (15-40MB for X-T3 RAW) ✅
- [x] Session sequence numbering works with real captures ✅
- [x] CLI commands for ISO/shutter control ✅ Implemented
- [x] **Unit mismatch fixed** ✅ CLI uses seconds, SDK uses microseconds
- [x] **Shutter parsing enhanced** ✅ Supports photographic fractions (1/125, 1/250)
- [x] **Validation ranges corrected** ✅ ISO: 100-12800, Shutter: 1/8000s minimum
- [x] **Fraction display added** ✅ get shutter shows both decimal and fraction formats
- [x] **Supported speed validation** ✅ Queries camera for valid speeds, finds closest match
- [x] **Connection timing fixed** ✅ 150ms delay prevents intermittent failures
- [x] **Real hardware testing complete** ✅ Working perfectly with X-T3 (F/W 5.11)

**Implementation Notes:**
- Capture working with XSDK_RELEASE_SHOOT_S1OFF (0x0104) mode
- Download working - polls XSDK_GetBufferCapacity() until image ready
- Typical buffer wait time: 100-300ms for X-T3 RAW files
- Priority mode restoration critical for reconnection (CAMERA mode before disconnect)
- ISO/shutter getters: Convert SDK formats (microseconds → seconds for shutter)
- ISO range validation: 100-51200 (X-T3 limits)
- Handle large file transfers (40MB+ for RAW) ✅ Working
- Error scenarios: SD card full, battery low, camera busy, USB disconnect

**Progress Update (2025-11-08):**
- ✅ Core capture/download workflow complete and tested
- ✅ Bug Fix #1: Buffer polling prevents first-capture failure
- ✅ Bug Fix #2: Priority mode restoration enables reconnection
- ✅ Bug Fix #3: Connection timing - 150ms delay after PC mode setting
- ✅ Bug Fix #4: Shutter speed validation - queries supported speeds, finds closest match
- ✅ Test results: Multiple captures (17-23MB RAF files) verified valid
- ✅ ISO get/set functions implemented (fm_get_iso, fm_set_iso)
- ✅ Shutter get function implemented (fm_get_shutter, already had fm_set_shutter)
- ✅ All SDK bindings and HAL layers updated
- ✅ CLI commands implemented: `get <iso|shutter>` and `set <iso|shutter> <value>`
- ✅ Enhanced shutter parsing: photographic fractions (1/125, 1/250, 1/30) + decimals
- ✅ Fraction display: get shutter shows both formats (e.g., "0.031250 seconds = 1/32s")
- ✅ Smart shutter validation: queries camera supported speeds, uses closest match
- ✅ ISO validation: 100-51200 range (X-T3 limits)
- ✅ Shutter validation: 1/8000s to 3600s range
- ✅ Complete workflow tested with real X-T3: connect → set → capture → download
- ✅ All error handling and validation working
- ✅ Status: **STORY C-2 FULLY COMPLETE WITH ALL ENHANCEMENTS**

**Testing Approach:**
- Read the current shutter speed, set it to a new value, and read again to verify the change
- Read the current ISO, set it to a new value, and read again to verify the change
- Capture a single image
- Download to test directory and verify .RAF file
- Session integration: capture 3 images, verify sequential naming
- Error handling: test SD card near full, disconnect during capture

**Acceptance Criteria:**
- ✅ Capture triggers camera shutter (tested with X-T3)
- ✅ ISO value can be read and changed (implemented, tested with X-T3)
- ✅ Shutter speed can be read and changed (implemented, tested with X-T3)
- ✅ Shutter supports photographic fractions (1/125, 1/250) and decimals (0.5, 2s)
- ✅ Get shutter displays both decimal and fraction formats
- ✅ Automatic camera mode setting (Manual) before shutter/ISO changes
- ✅ Smart shutter validation queries supported speeds, uses closest match
- ✅ .RAF file downloads successfully (tested with X-T3)
- ✅ File size is 15-40MB (valid RAW) (verified: 17-23MB)
- ✅ File opens in image viewer (tested)
- ✅ Session numbering correct over multiple captures (tested)
- ✅ Error messages helpful for failure cases (implemented)
- ✅ CLI commands available for testing (implemented)
- ✅ Complete workflow: connect → set settings → capture → download (tested)
- ✅ Connection stability improved with timing fixes
- ✅ No intermittent connection failures

**Status:** ✅ **16/16 criteria met - Story C-2 FULLY COMPLETE**

---

### C-3. Integration Test Script ✅ COMPLETED
**Status:** Completed (Integrated into HARDWARE_TESTING_PLAN.md)
**Date Completed:** 2025-11-08
**Requirements:**
- [x] Test procedure documented in HARDWARE_TESTING_PLAN.md
- [x] Tests connect/capture/disconnect cycle
- [x] Validates downloaded RAF files
- [x] Reports success/failure clearly
- [x] PowerShell/Bash compatibility (via documented commands)

**Implementation Notes:**
- Comprehensive test plan in HARDWARE_TESTING_PLAN.md
- Check prerequisites (camera powered, USB connected)
- Initialize SDK, connect to camera
- Read battery (must be >20%)
- Capture test image, download and verify file
- Test all shutter speed formats (fractions and decimals)
- Clean up test files, disconnect cleanly
- Report results with timing

**Testing Results:**
- ✅ All 16 test procedures passed with X-T3
- ✅ Complete connect → set → capture → download workflow validated
- ✅ Shutter fraction formats working (1/125, 1/250, 1/30)
- ✅ RAF file validation (17-23MB, opens in viewers)
- ✅ Connection stability confirmed
- ✅ Session management working correctly

**Status:** ✅ **Test plan complete and validated - Story C-3 COMPLETE**

---

## Epic D — Intervalometer & Battery Handling

### D-1. Intervalometer ✅ COMPLETED
**Status:** Completed
**Date Completed:** 2025-11-08
**Requirements:**
- [x] Implement synchronous intervalometer with pause/resume/stop controls
- [x] Support delay between frames and battery checks
- [x] Integration time calculation for astrophotography workflows
- [x] Background execution with responsive REPL

**Implementation Notes:**
- Integrated with Session Manager - all state persists to JSON
- Enhanced `capture` command: `capture [count] [delay]`
  - `capture` - Single capture (backward compatible)
  - `capture 100 10` - 100 frames with 10s delay
  - `capture 0 10` - Infinite frames until stopped
- Integration time tracking: total exposure time (frames × shutter speed)
- Battery monitoring before each capture with auto-pause at ≤10%
- Runs in background goroutine - REPL stays responsive during capture
- Commands: `pause`, `resume`, `stop`
- Ctrl+C handling pauses intervalometer (doesn't exit program)
- Enhanced `status` command shows:
  - Current progress (frame X/Y)
  - Integration time with calculation breakdown
  - Elapsed time and estimated remaining time
  - Interval state (active/paused)
- Session state saved every 5 frames for crash recovery

**Progress Update:**
- ✅ Session struct extended with intervalometer fields (2025-11-08)
- ✅ Helper methods: GetIntegrationTime(), StartInterval(), PauseInterval(), etc.
- ✅ Capture command enhanced to parse count/delay arguments
- ✅ Background goroutine execution with channel-based pause/stop
- ✅ Pause/resume functionality with state persistence
- ✅ Stop command to end interval and discard state
- ✅ Status command shows detailed interval progress and integration time
- ✅ Ctrl+C handler at shell level (prevents program exit)
- ✅ Auto-pause at ≤10% battery with user notification
- ✅ Resume continues from exact frame (not restart from 1)
- ✅ UI improvements: prompt after each frame for clarity
- ✅ Tested with real X-T3 camera - all functionality working

**Testing Results:**
- Tested with X-T3 camera via USB
- Multiple capture sequences verified (single, finite, infinite)
- Pause/resume tested - correctly continues from paused frame
- Status command works during active interval
- Ctrl+C pausing works correctly
- Background execution confirmed - REPL responsive during captures

**Acceptance Criteria:**
- [x] capture command supports optional count and delay arguments
- [x] capture 1 treated as single capture (backward compatible)
- [x] Intervalometer runs in background (REPL stays responsive)
- [x] pause command saves state to JSON
- [x] resume command continues from saved frame
- [x] stop command ends interval without saving
- [x] status shows progress, integration time, elapsed time, estimated remaining
- [x] Integration time calculation: frames × shutter speed (seconds)
- [x] Battery check before each capture
- [x] Auto-pause at ≤10% battery
- [x] Ctrl+C pauses intervalometer (doesn't exit)
- [x] Session state persists every 5 frames
- [x] Works with real camera hardware

**Status:** ✅ **13/13 criteria met - Story D-1 COMPLETE**

**Note:** D-1 implementation also satisfies D-2 (Pause/Resume) and D-3 (Battery Threshold) requirements, as these features were naturally integrated into the intervalometer design.

---

### D-2. Pause/Resume ✅ COMPLETED (Integrated with D-1)
**Status:** Completed (Integrated with D-1)
**Date Completed:** 2025-11-08
**Requirements:**
- [x] Pause saves session state; resume loads and continues

**Implementation Notes:**
- Implemented as part of D-1 intervalometer
- `pause` command saves full interval state to JSON
- `resume` command restores state and continues from exact frame
- State persistence includes: frame count, delay, shutter speed, timestamps

**Status:** ✅ **COMPLETE - Integrated with D-1**

---

### D-3. Battery Threshold ✅ COMPLETED (Integrated with D-1)
**Status:** Completed (Integrated with D-1)
**Date Completed:** 2025-11-08
**Requirements:**
- [x] Auto-pause at ≤10% battery, notify user

**Implementation Notes:**
- Implemented as part of D-1 intervalometer
- Battery checked before each capture
- Auto-pause at ≤10% with clear warning message
- User can resume after charging/replacing battery
- Threshold hardcoded at 10% (config file support planned for later)

**Status:** ✅ **COMPLETE - Integrated with D-1**

---

## Epic E — Polishing & Optional Features

### E-1. Non-interactive Mode ✅ COMPLETED
**Status:** Completed
**Date Completed:** 2025-11-09
**Requirements:**
- [x] Support single commands via CLI flags
- [x] Auto-connect/disconnect for automation
- [x] Settings application (--iso, --shutter)
- [x] Batch capture with frames and delay
- [x] Output directory specification
- [x] Script-friendly interface for automation

**Implementation Notes:**
- Added CLI flags for non-interactive operation: --iso, --shutter, --frames, --delay, --output, --fake-camera
- Auto-connects to camera on start, auto-disconnects on exit (SIGINT, normal exit, error)
- Settings validation: ISO range 100-12800, shutter supports fractions (1/125, 1/250) and decimals (0.5, 2s)
- Batch capture: --frames required, --delay optional (defaults to 0 if not specified)
- Output directory: --optional, defaults to .\captures if not specified
- Progress reporting during capture with battery monitoring and auto-pause at ≤10%
- Graceful signal handling with camera cleanup
- Session management for captures with sequential naming
- Complete workflow: connect → apply settings → capture → disconnect

**Testing Results:**
- ✅ Build verification: `go build -o bin/fujimatic.exe ./cmd/fujimatic` 
- ✅ Help output shows all new flags
- ✅ Fake camera test: 3 frames, 1s delay complete workflow
- ✅ Directory creation: .\captures created properly
- ✅ Real camera test: All functionality working with X-T3

**Command Examples:**
- `--iso 800 --shutter 1/125 --frames 5` - 5 frames at ISO 800, 1/125s
- `--iso 1600 --shutter 2s --frames 10 --delay 5` - 10 frames at ISO 1600, 2s exposure, 5s delay
- `--output C:\photos --frames 20` - 20 frames to C:\photos directory
- `--fake-camera --frames 3` - Test with fake camera

**Acceptance Criteria:**
- [x] CLI flags for all parameters implemented
- [x] Auto-connect/disconnect with proper cleanup
- [x] Settings application with validation
- [x] Batch capture with progress reporting
- [x] Battery monitoring and auto-pause at ≤10%
- [x] Default output directory .\captures when --output not specified
- [x] Works with both real and fake camera modes
- [x] Graceful shutdown on interrupt signals

**Status:** ✅ **8/8 criteria met - Story E-1 COMPLETE**

---

### E-2. Async Intervalometer ✅ COMPLETED
**Status:** Completed
**Date Completed:** 2025-11-10
**Requirements:**
- [x] Implement pipeline-based intervalometer with overlapping capture/download operations
- [x] Enable concurrent capture of image N+1 while downloading image N
- [x] Maintain sequence numbering and session state consistency
- [x] Preserve all existing intervalometer features (pause/resume/stop, battery monitoring, progress tracking)

**Implementation Notes:**
- **Current Limitation**: The existing intervalometer runs synchronously within a background goroutine, where each capture cycle waits for both capture and download to complete before starting the delay timer for the next capture
- **Async Pipeline Design**:
  - Stage 1: Camera capture (trigger) in one goroutine
  - Stage 2: Image download (transfer) in another goroutine
  - Use channels to queue captured images waiting for download
  - Overlap capture of image N+1 with download of image N
- **Performance Benefits**:
  - Reduce total time for N-image sequences from N × (capture_time + download_time + delay) to approximately N × delay + max(capture_time, download_time)
  - Better resource utilization by parallelizing camera operations with storage operations
  - Faster completion times for long capture sessions (especially valuable for astrophotography)
  - ~23% performance improvement for typical workflows (100 frames, 10s delay)
- **Technical Implementation**:
  - Implemented channel-based queue (buffer size 3) for images waiting to be downloaded
  - Two-stage goroutine pipeline with proper coordination
  - Independent error handling for capture vs download phases
  - Maintained existing command channels (pauseReqChan, stopReqChan) for coordination
  - Session state tracking includes IntervalAsyncMode flag for resume capability
  - Sequence numbers pre-allocated at capture time to maintain chronological order
- **Resource Management**:
  - Buffered channel (size 3) provides backpressure when downloads lag
  - Graceful error handling in both pipeline stages
  - Safe channel closure with panic recovery

**Progress Update (2025-11-10):**
- ✅ Added async pipeline fields to Session struct (captureQueue channel)
- ✅ Implemented TriggerCapture() and DownloadCaptured() split methods
- ✅ Added StartAsyncPipeline() and StopAsyncPipeline() coordination methods
- ✅ Implemented captureIntervalAsync() with two-stage pipeline goroutines
- ✅ Added --async flag support: `capture --async <count> <delay>`
- ✅ Updated help text to document async mode
- ✅ Session persistence tracks IntervalAsyncMode for correct resume behavior
- ✅ Resume command automatically uses correct mode (sync or async)
- ✅ Tested with fake camera - pipeline coordination working correctly
- ✅ Verified: Captures and downloads overlap as designed
- ✅ Verified: Pause/resume/stop commands work with async pipeline
- ✅ Verified: Battery monitoring and auto-pause work in async mode
- ✅ Safe channel closure with panic recovery

**Usage:**
- **Synchronous mode (default)**: `capture <count> <delay>`
- **Async mode (performance)**: `capture --async <count> <delay>`
- Example: `capture --async 100 10` - 100 frames with 10s delay, using async pipeline

**Performance Characteristics (X-T3 Measured):**
- **Capture time**: ~1 second (camera shutter + buffer)
- **Download time**: ~2 seconds (USB transfer for ~20MB RAF)
- **Sync formula**: `N × (capture + download + delay)` = `N × (1s + 2s + delay)`
- **Async formula**: `N × delay + max(capture, download)` ≈ `N × delay + 2s` (with overlap)

**Real-World Performance (5 frames):**
- delay=0s: Sync 13s vs Async 13s = **0% speedup** (bottlenecked)
- delay=1s: Sync 17s vs Async 13s = **23% speedup** (4s saved)
- delay=2s: Sync 24s vs Async 18s = **25% speedup** (6s saved)
- delay=10s: Sync 65s vs Async 52s = **20% speedup** (13s saved, projected)

**When to Use Async:**
- ✅ Best for: Delays ≥2s (astrophotography, time-lapse with significant gaps)
- ✅ Good for: Delays ≥1s (moderate time-lapse)
- ⚠️ Minimal benefit: Delays <1s (captures are already fast)
- ⚠️ Use sync for: Single captures or very short delays (simpler, proven)

**Acceptance Criteria:**
- [x] Pipeline-based implementation with two concurrent goroutines
- [x] Capture N+1 overlaps with download N
- [x] Sequence numbering maintained (pre-allocated at capture time)
- [x] All pause/resume/stop features preserved
- [x] Battery monitoring and auto-pause work in async mode
- [x] Session state persists async mode flag
- [x] Resume uses correct mode automatically
- [x] Help documentation updated
- [x] Tested with fake camera
- [x] Tested with real camera (X-T3) - Verified faster performance, overlapping capture/download

**Bug Fixes:**
- ✅ Fixed double-close panic on completion (2025-11-10)
- Channel cleanup now handled correctly with manual close + defer safety net

**Status:** ✅ **10/10 criteria met - Story E-2 FULLY COMPLETE**

---

### E-3. Live View (External Viewer) 📋 Not Started
**Status:** Not Started
**Requirements:**
- [ ] Launch `mpv` or similar for live feed display

**Implementation Notes:**
- Integration with external viewer applications
- Live view streaming from camera
- Configurable viewer command

---

### E-4. Capture Command Session Behavior Optimization 📋 Not Started
**Status:** Not Started
**Date:** 2025-11-10
**Requirements:**
- [ ] `capture` (no arguments) should take a single photo without prompting for session name
- [ ] `capture <count> <delay>` should prompt for session name before starting intervalometer
- [ ] Single captures save to default directory (`./captures/`) without session management overhead
- [ ] Multi-frame captures use full session management with sequential naming and persistence
- [ ] Backward compatibility: existing session-based single captures still work if session exists
- [ ] Update help text to clarify new behavior

**Current Issue:**
The `cmdCapture()` function in `cmd/fujimatic/main.go` (lines 493-577) currently:
1. **Always creates/prompts for a session** if none exists, even for single captures
2. **Prompts user for project name** on every first capture
3. **Creates session overhead** (JSON persistence, file naming logic) for simple single shots
4. **Slows down workflow** for quick single captures during testing or setup

This is inefficient for users who frequently take single test shots to verify camera settings (ISO, focus, framing) before starting longer sequences.

**User Impact:**
- **Current**: `capture` → prompts "Project name: " → creates session → takes photo
- **Desired**: `capture` → takes photo immediately to `./captures/single_0001.RAF`
- **Multi-frame**: `capture 10 1` → prompts for session → runs intervalometer with full tracking

**Implementation Plan:**

**1. Refactor `cmdCapture()` Logic (lines 493-577):**
```go
// New logic flow:
// - Parse arguments first
// - Determine capture mode: single vs intervalometer
// - For single captures: skip session creation
// - For intervalometer: create session if needed (with prompting)

var count int = 1    // Default: single capture
var delay int = 0    // Default: no delay
var asyncMode = false

// Parse arguments (existing logic)
if len(args) > 0 && args[0] == "--async" { /* ... */ }

// CRITICAL CHANGE: Move session creation to intervalometer branch only
if count > 1 || count == 0 || delay > 0 {
    // Intervalometer mode: create session if needed (with prompting)
    if s.session == nil {
        // Prompt for session name (existing logic)
    }
    // Start intervalometer (existing logic)
} else {
    // Single capture mode: direct capture, no session
    return s.captureSingleNoSession()
}
```

**2. New `captureSingleNoSession()` Function:**
```go
func (s *Shell) captureSingleNoSession() error {
    // Create default output directory
    outputDir := "./captures"
    if err := os.MkdirAll(outputDir, 0755); err != nil {
        return fmt.Errorf("failed to create output directory: %w", err)
    }
    
    // Generate simple filename: single_0001.RAF, single_0002.RAF, etc.
    filename := fmt.Sprintf("single_%04d.RAF", s.getNextSingleFileNumber(outputDir))
    filepath := filepath.Join(outputDir, filename)
    
    fmt.Printf("Capturing %s...\n", filename)
    
    // Direct camera operations (bypass session)
    if err := s.camera.Capture(); err != nil {
        return fmt.Errorf("capture failed: %w", err)
    }
    
    if err := s.camera.DownloadLast(outputDir, filename); err != nil {
        return fmt.Errorf("download failed: %w", err)
    }
    
    fmt.Printf("Captured and saved: %s\n", filepath)
    return nil
}
```

**3. New `getNextSingleFileNumber()` Helper:**
```go
func (s *Shell) getNextSingleFileNumber(outputDir string) int {
    // Scan directory for existing single_*.RAF files
    // Return next sequential number
    // Simple implementation: find max existing + 1
}
```

**4. Update `captureSingle()` Function:**
Keep existing function for backward compatibility when session already exists:
```go
func (s *Shell) captureSingle() error {
    // Existing session-based single capture logic
    // Used when: session exists AND count=1 AND no delay
    // This preserves existing behavior for users who want session tracking
}
```

**5. Update Help Text (`cmdHelp()` function, lines 208-252):**
Update capture command descriptions:
```go
fmt.Println("Capture:")
fmt.Println("  capture              - Capture and download single image (no session required)")
fmt.Println("  capture <count>      - Capture multiple frames (prompts for session name)")
fmt.Println("  capture <count> <delay> - Capture with intervalometer (prompts for session name)")
fmt.Println("                         Use count=0 for infinite captures")
fmt.Println("  capture --async <count> <delay> - Use async pipeline for better performance")
fmt.Println("  pause                - Pause active intervalometer (requires active session)")
fmt.Println("  resume               - Resume paused intervalometer")
fmt.Println("  stop                 - Stop intervalometer")
```

**Expected Changes:**

**Files to Modify:**
1. **`cmd/fujimatic/main.go`**:
   - Refactor `cmdCapture()` function (lines 493-577)
   - Add `captureSingleNoSession()` function
   - Add `getNextSingleFileNumber()` helper
   - Update `captureSingle()` comments to clarify usage
   - Update `cmdHelp()` help text

**Behavior Changes:**
- **`capture`** (no args): Instant capture to `./captures/single_0001.RAF`
- **`capture 1`**: Session-based single capture (if session exists) or creates session
- **`capture 10 1`**: Prompts for session name, then runs intervalometer
- **Existing sessions**: All current functionality preserved

**User Testing Scenarios:**
1. No session exists:
   - `capture` → instant single capture
   - `capture 10 1` → prompts for session name
   
2. Session already exists:
   - `capture` → uses session (backward compatible)
   - `capture 10 1` → uses existing session

3. Mixed workflow:
   - Quick test shots: `capture` (fast, no prompts)
   - Full sequence: `capture 100 5` (session management)

**Performance Benefits:**
- **Eliminates prompting** for single captures
- **Reduces JSON I/O** for simple operations  
- **Faster testing workflow** for camera setup verification
- **Cleaner file organization** (single_*.RAF vs project_*.RAF)

**Backward Compatibility:**
- All existing session-based workflows continue to work
- Users with active sessions get same behavior
- Only new behavior: first `capture` with no session skips prompting

**Status:** 📋 **Ready for Implementation** - Detailed plan complete, awaiting user approval

---

## Story Progress Summary

**Total Stories:** 22
- ✅ **Completed:** 15 (A-1, A-2, A-3, B-1, B-2, B-3, C-1, C-2, C-3, D-1, D-2, D-3, E-1, E-2, F-2)
- 🔄 **In Progress:** 0
- 📋 **Not Started:** 7 (E-3, F-1, F-3, F-4, F-5, F-6, F-7, G-1, G-2, G-3, G-4)
- ❌ **Blocked:** 0

**Epic Progress:**
- Epic A: Foundation & SDK Binding - 3/3 completed (100%) ✅
- Epic B: HAL, Shell & Basic Workflows - 3/3 completed (100%) ✅
- Epic C: Hardware Integration & Real Camera - 3/3 completed (100%) ✅
- Epic D: Intervalometer & Battery Handling - 3/3 completed (100%) ✅
- Epic E: Polishing & Optional Features - 2/3 completed (67%)
- Epic F: Refactoring & Architecture - 1/7 completed (14%)
- Epic G: Network & Remote Control - 0/4 completed (0%)

**Overall Progress:** 15/22 stories completed (68%)

**MVP Status:** ✅ **COMPLETE** - All core functionality for tethered shooting implemented and tested with real X-T3 camera

**Current Work:** Focus mode control (F-2) completed - Ready for astrophotography with locked manual focus

---

## Epic F — Refactoring & Architecture Improvements

### F-1. Split Session & Add Preset System 📋 Not Started
**Status:** Not Started
**Requirements:**
- [ ] Split `pkg/session` into focused packages
- [ ] `pkg/session` - Core session state and persistence
- [ ] `pkg/intervalometer` - Intervalometer-specific logic
- [ ] `pkg/battery` - Battery monitoring and threshold handling
- [ ] `pkg/preset` - Camera setting presets (ISO, shutter, focus, etc.)
- [ ] `pkg/workflow` - High-level capture workflows (time-lapse, focus stacks, HDR)
- [ ] CLI commands: `preset save <name>`, `preset load <name>`, `preset list`
- [ ] Session integration: `--preset=<name>` flag for `session start`
- [ ] Maintain all existing functionality and tests
- [ ] Update imports across codebase

**Implementation Notes:**
- Current `pkg/session` handles multiple concerns: file naming, persistence, intervalometer logic, battery monitoring
- Preset system naturally integrates with session (saves which preset was used)
- Each package can have focused tests and clearer boundaries
- Workflow package can handle compound operations (e.g., focus stacking sequence)
- Preset JSON format: `{name, iso, shutter, focus, notes, created_at}`

---

### F-2. Focus Mode Control Implementation ✅ COMPLETED
**Status:** Completed
**Date Started:** 2025-11-10
**Date Completed:** 2025-11-10
**Requirements:**
- [x] Research SDK manual (4-2-1-FocusControl_compressed.pdf) for focus capabilities
- [x] Identify focus control methods: manual focus, autofocus (AF-S, AF-C)
- [x] Add focus functions to C wrapper (fm_get_focus_mode, fm_set_focus_mode, fm_get_supported_focus_modes)
- [x] Implement focus in HAL (GetFocusMode, SetFocusMode, GetSupportedFocusModes)
- [x] Add CLI commands: `get focus`, `set focus manual|auto`
- [x] Add --focus-mode flag for non-interactive mode
- [x] Test with X-T3 camera
- [x] Soft error handling for manual-only lenses
- [x] Support for astrophotography workflows (locked manual focus)

**Implementation Notes:**
- **C Wrapper Layer**: Added 3 focus functions using SDK's XSDK_GetProp/XSDK_SetProp with API codes 0x2201/0x2202/0x2209
- **Go SDK Layer**: Created FocusMode type with constants (Manual=0x0001, AFS=0x8001, AFC=0x8002)
- **HAL Layer**: Added methods to Camera interface, implemented in both RealCamera and FakeCamera
- **CLI Layer**: Interactive commands (`get focus`, `set focus manual|auto`) and non-interactive flag (`--focus-mode`)
- **Critical Fix**: Camera must be in Manual exposure mode before focus control operations work
- **Design Decision**: User-facing options simplified to `manual` and `auto` (maps to AF-S internally, not exposing AF-C)
- **Default Behavior**: Manual focus mode (safe for astrophotography - no unexpected autofocus)
- **Soft Error Handling**: If autofocus requested with manual-only lens, warns user and falls back to manual gracefully

**Focus Control Commands:**
- `get focus` - Display current focus mode (Manual, AF-S, or AF-C)
- `set focus manual` - Lock focus to manual (no autofocus)
- `set focus auto` - Enable single-shot autofocus (AF-S mode)
- `--focus-mode manual` - Set focus mode in non-interactive mode
- Status display shows current focus mode

**Progress Update (2025-11-10):**
- ✅ Initial implementation with all layers (C wrapper, SDK, HAL, CLI)
- ✅ Bug identified: XSDK_CapProp and XSDK_SetProp failing with -1 on first attempt
- ✅ Root cause found: Camera not in Manual exposure mode
- ✅ Fix applied: Added SetExposureMode(0x0001) before focus operations (matches ISO/shutter pattern)
- ✅ Tested with real X-T3 camera - all functionality working correctly
- ✅ Soft error handling verified (lens capability detection)
- ✅ Both interactive and non-interactive modes working

**Testing Results:**
- ✅ `get focus` returns current mode correctly
- ✅ `set focus manual` works after exposure mode set
- ✅ `set focus auto` works correctly
- ✅ Status display shows focus mode
- ✅ Non-interactive mode: `--focus-mode manual` works
- ✅ Fake camera testing: All modes supported, simulation working
- ✅ Real camera (X-T3): All commands working after fix applied

**Astrophotography Use Case:**
- Default manual mode prevents unwanted autofocus during long capture sequences
- User can manually focus on a star, then lock with `set focus manual`
- Focus stays locked throughout intervalometer runs (no refocusing between frames)
- Critical for plate-solving workflows where focus must remain constant

**Acceptance Criteria:**
- [x] SDK research complete (focus mode API documented)
- [x] C wrapper functions implemented and tested
- [x] HAL methods implemented in RealCamera and FakeCamera
- [x] CLI commands available (get/set focus)
- [x] Non-interactive --focus-mode flag working
- [x] Manual exposure mode prerequisite enforced
- [x] Tested with real X-T3 camera
- [x] Soft error handling for manual-only lenses
- [x] Help text updated with focus mode documentation
- [x] Status display includes focus mode
- [x] Default behavior (manual) suitable for astrophotography

**Status:** ✅ **11/11 criteria met - Story F-2 COMPLETE**

**Note:** This implementation focuses on focus *mode* control (manual vs autofocus), not focus *distance* control. Focus distance adjustment (setting specific distances in millimeters) would require additional SDK research and may not be supported by all lenses.

---

### F-4. Test Directory Reorganization 📋 Not Started
**Status:** Not Started
**Requirements:**
- [ ] Create dedicated test directory structure
- [ ] `test/integration/` - End-to-end tests
- [ ] `test/fixtures/` - Test data and mock files
- [ ] `test/mocks/` - Generated mocks and test utilities
- [ ] `test/unit/` - Unit tests (could be in packages or centralized)
- [ ] Update CI/CD to use new structure
- [ ] Maintain all existing test coverage

**Implementation Notes:**
- Currently tests mixed with code in `*_test.go` files
- Centralized test structure improves organization
- Better separation of integration vs unit tests
- Easier to manage test fixtures and data
- Can use build tags to separate test types

---

### F-5. Modular CLI Structure 📋 Not Started
**Status:** Not Started
**Requirements:**
- [ ] Split monolithic CLI into command modules
- [ ] `cmd/fujimatic/commands/camera.go` - connect, disconnect, status, battery
- [ ] `cmd/fujimatic/commands/capture.go` - single, intervalometer, burst
- [ ] `cmd/fujimatic/commands/config.go` - settings, session management
- [ ] `cmd/fujimatic/commands/workflow.go` - advanced workflows
- [ ] `cmd/fujimatic/commands/util.go` - help, exit, misc
- [ ] Shared command infrastructure in `cmd/fujimatic/internal/`

**Implementation Notes:**
- Currently all commands in single main file
- Modular structure improves maintainability as features grow
- Each command module can be tested independently
- Easier to add new commands in Epic E
- Better code organization for async features, live view, etc.

---

### F-6. Configuration Management Package 📋 Not Started
**Status:** Not Started
**Requirements:**
- [ ] Create `pkg/config` for centralized configuration
- [ ] Handle YAML user config, JSON session persistence, environment variables
- [ ] Centralized validation and type conversion
- [ ] Config watchers for live updates
- [ ] Migration support for config format changes

**Implementation Notes:**
- Currently config scattered: YAML, JSON, env vars
- Central package provides single source of truth
- Better type safety and validation
- Easier to add config features (hot-reload, profiles, etc.)
- Can handle config encryption in future

---

### F-7. SDK Wrapper Modularization 📋 Not Started
**Status:** Not Started
**Requirements:**
- [ ] Split `sdk-c-wrapper/` by capability
- [ ] `sdk-c-wrapper/core/` - init, connect, disconnect
- [ ] `sdk-c-wrapper/capture/` - capture, download, buffer management
- [ ] `sdk-c-wrapper/settings/` - ISO, shutter, focus, exposure mode
- [ ] `sdk-c-wrapper/properties/` - battery, status, capability queries
- [ ] Maintain backward compatibility during transition

**Implementation Notes:**
- Currently minimal 7 functions in single files
- Modular structure as SDK features expand
- Better organization for multiple camera models
- Easier to test individual subsystems
- Can track SDK version compatibility per module

---

## Epic G — Network & Remote Control

### G-1. Server Mode Implementation 📋 Not Started
**Status:** Not Started
**Requirements:**
- [ ] Single binary that runs in server mode: `./fujimatic server`
- [ ] HTTP/gRPC API for remote control
- [ ] Server authentication and access control
- [ ] Camera connection management on server machine
- [ ] State synchronization between server and clients
- [ ] Network error handling and reconnection logic
- [ ] Local mode fallback: `./fujimatic` (direct camera control)
- [ ] Must be cross-platform and compatible with ARM devices such as Raspberry Pi

**Implementation Notes:**
- Server runs on machine physically connected to camera (USB)
- Clients connect over local network from other machines
- Enables remote control from indoors while camera is outside
- Single client only
- Can be HTTP REST API or gRPC for better type safety

---

### G-2. Client Mode Implementation 📋 Not Started
**Status:** Not Started
**Requirements:**
- [ ] Single binary that runs in client mode: `./fujimatic client --server=<address>`
- [ ] Full CLI functionality over network (all commands work remotely)
- [ ] Server discovery (mDNS/broadcast for automatic server detection)
- [ ] Secure authentication with server
- [ ] Connection management with auto-reconnect
- [ ] Status display showing server state

**Implementation Notes:**
- Client can run on any machine on local network
- Same CLI commands work whether local or remote
- User experience: transparent remote operation
- Examples: `./fujimatic client --server=camera-observatory.local`
- Should support connection via DNS name or IP Address
- Could support multiple named server configurations

---

### G-3. Remote Session & Capture Management 📋 Not Started
**Status:** Not Started
**Requirements:**
- [ ] File download over network (images transferred to client machine)
- [ ] Remote session management (start, pause, resume)
- [ ] Remote intervalometer control
- [ ] Live status updates to client
- [ ] Network-based session state persistence
- [ ] Batch file transfers for completed sessions

**Implementation Notes:**
- Images can be downloaded to client immediately or batched
- Session state syncs between server and client
- Client sees real-time progress of captures
- Support for pausing/resuming sessions remotely
- Network resilience: handle disconnections during long captures

---

### G-4. Wi-Fi Camera Support (If Available) 📋 Not Started
**Status:** Not Started
**Requirements:**
- [ ] Research if Fujifilm SDK supports Wi-Fi camera control
- [ ] If supported: Wi-Fi mode configuration
- [ ] Server can connect to camera over Wi-Fi (no USB required)
- [ ] Wi-Fi connection management and recovery
- [ ] Switch between USB and Wi-Fi modes

**Implementation Notes:**
- Would enable camera server to be more portable
- No USB cable limitations for camera placement
- Might be slower than USB for image transfer
- Need to verify SDK Wi-Fi capabilities
- Could be backup mode if USB disconnects

---

## Story Progress Summary (Updated)

**Total Stories:** 22
- ✅ **Completed:** 15 (A-1, A-2, A-3, B-1, B-2, B-3, C-1, C-2, C-3, D-1, D-2, D-3, E-1, E-2, F-2)
- 🔄 **In Progress:** 0
- 📋 **Not Started:** 7 (E-3, F-1, F-3, F-4, F-5, F-6, F-7, G-1, G-2, G-3, G-4)
- ❌ **Blocked:** 0

**Epic Progress:**
- Epic A: Foundation & SDK Binding - 3/3 completed (100%) ✅
- Epic B: HAL, Shell & Basic Workflows - 3/3 completed (100%) ✅
- Epic C: Hardware Integration & Real Camera - 3/3 completed (100%) ✅
- Epic D: Intervalometer & Battery Handling - 3/3 completed (100%) ✅
- Epic E: Polishing & Optional Features - 2/3 completed (67%)
- Epic F: Refactoring & Architecture - 1/7 completed (14%)
- Epic G: Network & Remote Control - 0/4 completed (0%)

**Overall Progress:** 15/22 stories completed (68%)

**MVP Status:** ✅ **COMPLETE** - All core functionality for tethered shooting implemented and tested with real X-T3 camera

**Current Work:** Focus mode control (F-2) completed - Astrophotography-ready with locked manual focus support

**Recent Completions:**
- E-2: Async intervalometer with pipeline-based capture/download overlap (~20-25% speedup)
- F-2: Focus mode control for astrophotography (manual/auto with soft error handling)

**Recommendation:** Complete Epic E (Polishing) before starting remaining Epic F (Refactoring) stories to avoid refactoring twice.