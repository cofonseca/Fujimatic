# RealCamera Integration Story - Proposed

## Story Placement & Epic Reordering

**Current situation:**
- Epic B (HAL, Shell, Basic Workflows) is complete ✅
- Epic C (Intervalometer) is next in line
- Epic D (Integration & Hardware) comes after

**Proposal:**
Swap Epic C and D order. Complete hardware integration before adding advanced features.

**Rationale:**
- We have a working CLI with FakeCamera
- User has real X-T3 camera ready for testing
- Hardware validation should come before building intervalometer features on top
- RealCamera bugs are easier to diagnose without intervalometer complexity

**New Epic Order:**
1. Epic A: Foundation & SDK Binding ✅ (completed)
2. Epic B: HAL, Shell & Basic Workflows ✅ (completed)
3. **Epic C: Hardware Integration & Real Camera** (previously Epic D, rewritten)
4. Epic D: Intervalometer & Battery Handling (previously Epic C)
5. Epic E: Polishing & Optional Features (unchanged)

---

## Epic C (New) — Hardware Integration & Real Camera

### C-1. RealCamera Implementation with Fujifilm SDK

**Status:** Not Started

**Story Description:**
Implement RealCamera adapter in pkg/hal/real.go that uses the real Fujifilm SDK via pkg/sdk cgo bindings. Test with actual X-T3 camera hardware to validate connect, disconnect, battery read, and basic capture operations work with real hardware.

**Requirements:**
- [ ] Implement RealCamera struct in pkg/hal/real.go
- [ ] Use pkg/sdk cgo bindings for all camera operations
- [ ] Add SDK initialization (fm_init with SDK path)
- [ ] Test connection to real X-T3 camera via USB
- [ ] Verify disconnect cleanly releases camera
- [ ] Validate battery reading returns actual camera battery level
- [ ] Update CLI to support both FakeCamera and RealCamera modes

**Technical Details:**

**1. RealCamera Structure:**
```go
type RealCamera struct {
    sdkCamera *sdk.Camera
    mu        sync.Mutex
    connected bool
}
```

**2. SDK Initialization Flow:**
- Initialize SDK once at startup with SDK path from environment variable
- SDK path: FUJI_SDK_PATH → redistributables directory
- Handle initialization errors gracefully

**3. Implementation Requirements:**
- Wrap all pkg/sdk functions with proper error handling
- Thread-safe operations using mutex
- Connection state tracking
- Graceful error messages for common hardware issues:
  - Camera not powered on
  - USB cable not connected
  - Camera in wrong mode
  - SDK initialization failure

**4. CLI Integration:**
- Add `-camera` flag or environment variable to choose FakeCamera vs RealCamera
- Default to FakeCamera for safety
- Example: `CAMERA_MODE=real ./fujimatic` or `./fujimatic -camera real`

**5. Build Requirements:**
- Ensure C wrapper library (fmwrapper.dll/.so) is built
- Fujifilm SDK redistributables must be in place
- Update build instructions for real SDK linking

**Testing Approach:**

**Phase 1: SDK Initialization**
- Test SDK init with valid and invalid paths
- Verify error handling for missing SDK files

**Phase 2: Connection**
- Connect to powered-on X-T3 via USB
- Verify connection state
- Test disconnect

**Phase 3: Battery Reading**
- Read battery level from real camera
- Compare with camera LCD display
- Verify percentage is reasonable (0-100)

**Phase 4: CLI Integration**
- Test CLI commands with RealCamera:
  - `connect` → actual USB connection
  - `status` → real battery level
  - `disconnect` → camera released
  - `battery` → current charge level

**Phase 5: Basic Capture (if SDK supports it)**
- Test single capture command
- Verify RAF file created on camera
- Note: Full capture+download tested in C-2

**Acceptance Criteria:**
✅ RealCamera implements hal.Camera interface
✅ SDK initializes successfully with valid SDK path
✅ CLI can connect to real X-T3 camera
✅ Battery reading matches camera display (±5%)
✅ Disconnect releases camera cleanly
✅ Error messages are helpful for common issues
✅ No memory leaks in cgo operations
✅ CLI runs with both `-camera fake` and `-camera real`

**Implementation Notes:**
- Start with pkg/hal/real.go implementation
- Add SDK initialization in main.go or separate init package
- Consider creating pkg/config for SDK path management
- Use environment variable pattern for integration testing
- Document camera setup requirements (USB mode, power on, etc.)

**Gotchas & Considerations:**
- SDK may require camera to be in specific mode (check manual)
- USB connection on Windows may need driver installation
- Linux may require udev rules for camera access
- Some SDK functions may be blocking/synchronous
- Battery reading may not be available while camera is busy
- Camera may auto-sleep and require reconnection

**Dependencies:**
- Fujifilm SDK redistributables (HEADERS/ and REDISTRIBUTABLES/)
- C wrapper library (fmwrapper.dll/.so) already built
- pkg/sdk cgo bindings already implemented
- USB cable and powered X-T3 camera

**Success Metrics:**
- Can connect to camera within 2 seconds
- Battery reading accurate to within 5%
- Disconnect completes without hanging
- Zero memory leaks over 100 connect/disconnect cycles
- Helpful error messages for 5 common failure scenarios

---

### C-2. Full Capture & Download with Real Camera

**Status:** Not Started (blocked by C-1)

**Story Description:**
Test complete capture workflow with real X-T3 camera: trigger shutter, capture RAW image, download .RAF file to disk. Integrate with Session Manager for sequential numbering.

**Requirements:**
- [ ] Test SetShutter() with real camera shutter speeds
- [ ] Capture() triggers actual shutter
- [ ] DownloadLast() retrieves .RAF file from camera
- [ ] RAF file saved to correct output directory
- [ ] File size is reasonable (15-40MB for X-T3 RAW)
- [ ] Session sequence numbering works with real captures
- [ ] Handle SD card full error
- [ ] Handle camera busy error

**Technical Details:**

**1. Shutter Speed Support:**
- Test common astrophotography speeds: 1s, 5s, 10s, 30s
- Document supported shutter speed range
- Handle out-of-range values gracefully

**2. Download Mechanism:**
- Understand SDK download API
- May require temporary storage before final location
- Handle large file transfers (40MB+ for RAW)

**3. Error Scenarios:**
- Camera SD card full
- Camera battery too low
- Camera busy/processing
- USB disconnection during capture
- Disk full on computer

**Testing Approach:**

**Phase 1: Single Capture**
- Set shutter speed to 1s
- Trigger capture
- Wait for completion
- Verify image on camera LCD

**Phase 2: Download Test**
- Capture single image
- Download to test directory
- Verify .RAF file:
  - File size: 15-40MB
  - RAF file header valid
  - Can open in image viewer

**Phase 3: Session Integration**
- Start session with CLI
- Capture 3 images
- Verify sequential naming (0001, 0002, 0003)
- Verify all files downloaded

**Phase 4: Error Handling**
- Test with SD card near full
- Test disconnect during capture
- Test rapid capture attempts

**Acceptance Criteria:**
✅ Capture triggers camera shutter
✅ .RAF file downloads successfully
✅ File size is 15-40MB (valid RAW)
✅ File opens in darktable/RawTherapee
✅ Session numbering correct over multiple captures
✅ Error messages helpful for failure cases
✅ No hanging on camera disconnect

---

### C-3. Integration Test Script

**Status:** Not Started (blocked by C-2)

**Story Description:**
Create automated test script (PowerShell/Bash) that runs through complete hardware test workflow. Useful for regression testing after SDK updates or hardware changes.

**Requirements:**
- [ ] PowerShell script for Windows
- [ ] Bash script for Linux (bonus)
- [ ] Tests connect/capture/disconnect cycle
- [ ] Validates downloaded RAF files
- [ ] Reports success/failure clearly
- [ ] Can run in CI with hardware attached (future)

**Script Features:**
- Check prerequisites (camera powered, USB connected)
- Initialize SDK
- Connect to camera
- Read battery (must be >20%)
- Capture test image
- Download and verify file
- Clean up test files
- Disconnect cleanly
- Report results with timing

**Test Output Example:**
```
Fujimatic Hardware Integration Test
====================================
[✓] SDK initialized
[✓] Camera connected (3.2s)
[✓] Battery: 85%
[✓] Shutter set: 1s
[✓] Capture complete (2.1s)
[✓] Download complete (4.5s)
[✓] File size: 32.4MB
[✓] RAF header valid
[✓] Disconnect clean

All tests passed! (12.3s total)
```

---

## Summary of Proposed Changes

**Epic Reordering:**
- Swap Epic C (Intervalometer) and Epic D (Hardware)
- New order: A → B → C (Hardware) → D (Intervalometer) → E (Polish)

**New Stories:**
- C-1: RealCamera Implementation (detailed above)
- C-2: Full Capture & Download (revised from old D-2)
- C-3: Integration Test Script (same as old D-3)

**Rationale:**
- Validate hardware early before building complex features
- User has camera ready for testing now
- Easier debugging without intervalometer complexity
- Natural progression: abstractions → basic hardware → advanced features

**Next Steps After Approval:**
1. Update STORIES.md with new epic order
2. Implement C-1 (RealCamera)
3. Test with X-T3 camera hardware
4. Complete C-2 (full workflow)
5. Create C-3 (test automation)
6. Then proceed to Epic D (Intervalometer)

---

## Questions for Review

1. **Epic Reordering:** Agree with swapping Epic C and D order?
2. **Camera Mode Selection:** Prefer `-camera` flag, environment variable, or config file?
3. **SDK Initialization:** Should it be in main.go or separate init package?
4. **Error Handling:** What level of detail for camera connection errors?
5. **Testing Scope:** Should C-1 include basic capture test or wait for C-2?
6. **Build Process:** Any concerns about linking with real SDK libraries?

Please review and let me know if this story structure makes sense or if you'd like any changes before we proceed with implementation.
