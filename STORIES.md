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

### C-2. Full Capture & Download with Real Camera 📋 Not Started
**Status:** Not Started (blocked by C-1)
**Requirements:**
- [ ] Test SetShutter() with real camera shutter speeds
- [ ] Capture() triggers actual shutter
- [ ] DownloadLast() retrieves .RAF file from camera
- [ ] RAF file saved to correct output directory
- [ ] File size is reasonable (15-40MB for X-T3 RAW)
- [ ] Session sequence numbering works with real captures
- [ ] Handle SD card full error
- [ ] Handle camera busy error

**Implementation Notes:**
- Test common astrophotography speeds: 1s, 5s, 10s, 30s
- Document supported shutter speed range
- Handle large file transfers (40MB+ for RAW)
- Error scenarios: SD card full, battery low, camera busy, USB disconnect

**Testing Approach:**
- Single capture with 1s shutter speed
- Download to test directory and verify .RAF file
- Session integration: capture 3 images, verify sequential naming
- Error handling: test SD card near full, disconnect during capture

**Acceptance Criteria:**
- ✅ Capture triggers camera shutter
- ✅ .RAF file downloads successfully
- ✅ File size is 15-40MB (valid RAW)
- ✅ File opens in darktable/RawTherapee
- ✅ Session numbering correct over multiple captures
- ✅ Error messages helpful for failure cases

---

### C-3. Integration Test Script 📋 Not Started
**Status:** Not Started (blocked by C-2)
**Requirements:**
- [ ] PowerShell script for Windows
- [ ] Bash script for Linux (bonus)
- [ ] Tests connect/capture/disconnect cycle
- [ ] Validates downloaded RAF files
- [ ] Reports success/failure clearly

**Implementation Notes:**
- Check prerequisites (camera powered, USB connected)
- Initialize SDK, connect to camera
- Read battery (must be >20%)
- Capture test image, download and verify file
- Clean up test files, disconnect cleanly
- Report results with timing

**Script Features:**
- Automated hardware test workflow
- Useful for regression testing after SDK updates
- Can run in CI with hardware attached (future)

---

## Epic D — Intervalometer & Battery Handling

### D-1. Intervalometer 📋 Not Started
**Status:** Not Started
**Requirements:**
- [ ] Implement synchronous intervalometer (`start`, `stop`)
- [ ] Support delay between frames and battery checks

**Implementation Notes:**
- Should integrate with Session Manager
- Need integration time calculations
- Battery monitoring during capture sequence
- Graceful start/stop functionality

---

### D-2. Pause/Resume 📋 Not Started
**Status:** Not Started
**Requirements:**
- [ ] Pause saves session state; resume loads and continues

**Implementation Notes:**
- Session state should persist to JSON
- Resume should restore all capture parameters
- File sequence numbering continuity

---

### D-3. Battery Threshold 📋 Not Started
**Status:** Not Started
**Requirements:**
- [ ] Auto-pause at ≤10% battery, notify user

**Implementation Notes:**
- Battery threshold configurable (default 10% from README)
- User notification system
- Resume functionality when battery recharged

---

## Epic E — Polishing & Optional Features

### E-1. Non-interactive Mode 📋 Not Started
**Status:** Not Started
**Requirements:**
- [ ] Support single commands via CLI flags

**Implementation Notes:**
- Add command-line flags for non-interactive operation
- Script-friendly interface
- Support batch operations

---

### E-2. Async Intervalometer 📋 Not Started
**Status:** Not Started
**Requirements:**
- [ ] Enable concurrent capture + download

**Implementation Notes:**
- Parallel capture and download operations
- Performance optimization
- Resource management

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

## Story Progress Summary

**Total Stories:** 13
- ✅ **Completed:** 7 (A-1, A-2, A-3, B-1, B-2, B-3, C-1)
- 🔄 **In Progress:** 0
- 📋 **Not Started:** 6
- ❌ **Blocked:** 0

**Epic Progress:**
- Epic A: Foundation & SDK Binding - 3/3 completed (100%) ✅
- Epic B: HAL, Shell & Basic Workflows - 3/3 completed (100%) ✅
- Epic C: Hardware Integration & Real Camera - 1/3 completed (33%)
- Epic D: Intervalometer & Battery Handling - 0/3 completed (0%)
- Epic E: Polishing & Optional Features - 0/3 completed (0%)

**Overall Progress:** 7/13 stories completed (54%)