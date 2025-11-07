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

### A-3. Add Go cgo Bindings 📋 Not Started
**Status:** Not Started
**Requirements:**
- [ ] Implement Go-friendly wrappers for C functions
- [ ] Test using dummy return codes

**Implementation Notes:**
- Will create pkg/sdk/ package with cgo bindings
- Need to implement cgo build tags and linking with C library
- Plan to use `// #cgo` directives for library linking

---

## Epic B — HAL, Shell & Basic Workflows

### B-1. Implement HAL Interface 📋 Not Started
**Status:** Not Started
**Requirements:**
- [ ] Define `Camera` interface and `fake` implementation
- [ ] Unit tests run with fake HAL

**Implementation Notes:**
- Will create pkg/hal/ package
- Camera interface should abstract camera operations
- Fake implementation for testing without real hardware
- Plan to include interfaces for: Connect, Disconnect, Capture, GetBattery, SetShutter

---

### B-2. Session Manager 📋 Not Started
**Status:** Not Started
**Requirements:**
- [ ] Implement file naming and sequence tracking
- [ ] Validate synchronous capture + download

**Implementation Notes:**
- Will create pkg/session/ package
- Need to implement session persistence (JSON format as mentioned in README)
- File naming strategy for captured images
- Project directory management

---

### B-3. CLI Shell 📋 Not Started
**Status:** Not Started
**Requirements:**
- [ ] Add `connect`, `capture`, and `exit` commands
- [ ] Verify REPL responds correctly

**Implementation Notes:**
- Current cmd/fujimatic/main.go has basic REPL prototype
- Need to enhance with real command implementations
- Plan to integrate with HAL and Session Manager
- Should support both interactive and non-interactive modes

---

## Epic C — Intervalometer & Battery Handling

### C-1. Intervalometer 📋 Not Started
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

### C-2. Pause/Resume 📋 Not Started
**Status:** Not Started
**Requirements:**
- [ ] Pause saves session state; resume loads and continues

**Implementation Notes:**
- Session state should persist to JSON
- Resume should restore all capture parameters
- File sequence numbering continuity

---

### C-3. Battery Threshold 📋 Not Started
**Status:** Not Started
**Requirements:**
- [ ] Auto-pause at ≤10% battery, notify user

**Implementation Notes:**
- Battery threshold configurable (default 10% from README)
- User notification system
- Resume functionality when battery recharged

---

## Epic D — Integration & Hardware

### D-1. Connect/Disconnect with Real SDK 📋 Not Started
**Status:** Not Started
**Requirements:**
- [ ] Camera connects and disconnects cleanly

**Implementation Notes:**
- Will require real Fujifilm SDK integration
- Error handling for connection failures
- Device detection and selection

---

### D-2. Capture RAW & Download 📋 Not Started
**Status:** Not Started
**Requirements:**
- [ ] `.RAF` file successfully saved to configured folder

**Implementation Notes:**
- RAW file format support (.RAF)
- File download from camera
- Storage to configured output directory

---

### D-3. Integration Test Script 📋 Not Started
**Status:** Not Started
**Requirements:**
- [ ] PowerShell script automates basic camera tests

**Implementation Notes:**
- Will create integration test script
- Should test connect/capture/disconnect flow
- Automated verification of camera functionality

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
- ✅ **Completed:** 2 (A-1, A-2)
- 🔄 **In Progress:** 0
- 📋 **Not Started:** 11
- ❌ **Blocked:** 0

**Epic Progress:**
- Epic A: 2/3 completed (66%)
- Epic B: 0/3 completed (0%)
- Epic C: 0/3 completed (0%)
- Epic D: 0/3 completed (0%)
- Epic E: 0/3 completed (0%)

**Overall Progress:** 2/13 stories completed (15%)