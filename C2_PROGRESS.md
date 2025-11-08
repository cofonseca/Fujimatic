# Story C-2 Progress Summary

## ✅ Completed (2025-11-08)

### Capture & Download Implementation

**Status:** ✅ **WORKING WITH REAL X-T3!**

#### Core Functionality Tested
- ✅ **fm_capture()** - Triggers shutter release via XSDK_Release
- ✅ **fm_download_last()** - Downloads RAF files from camera buffer
- ✅ **Session workflow** - Sequential file naming (test_0001.RAF, test_0002.RAF, etc.)
- ✅ **File validation** - RAF files open successfully in image viewers

#### Test Results

**Hardware:** X-T3 (Firmware 5.11) via USB-C cable

**Capture Test Output:**
```
> capture
Capturing test_0001.RAF...
Triggering shutter release (mode: 0x0104)...
Capture triggered successfully (AF status: 1, shot_opt: 1)
Waiting for image to be written to camera buffer...
Image available in buffer after 200 ms (1 frames)
Downloaded image: captures\test_0001.RAF (17653760 bytes)
Captured and saved: captures\test_0001.RAF
```

**File Sizes:**
- test_0001.RAF: 17.6 MB
- test_0002.RAF: 23.1 MB

Both files verified as valid RAF images (opened in viewer).

---

## 🐛 Bugs Fixed (2025-11-08)

### Bug 1: First Capture Always Failed to Download

**Issue:** First capture would fire shutter but fail with "No images in buffer"

**Root Cause:** We were checking `XSDK_GetReleaseStatus()` which only tracks mechanical shutter completion. The X-T3 needs 1-3 seconds to write the 20MB RAW file to its internal buffer.

**Solution:** Changed to poll `XSDK_GetBufferCapacity()` as specified in SDK manual:
```c
// Wait for image to appear in buffer
// SDK manual: "poll the camera buffer by XSDK_GetBufferCapacity()"
while (attempts < 100) {  // Max 10 seconds
    XSDK_GetBufferCapacity(g_hCamera, &shoot_frames, &total_frames);
    if (shoot_frames > 0) {
        printf("Image available in buffer after %d ms\n", attempts * 100);
        break;
    }
    Sleep(100);
    attempts++;
}
```

**Fix Location:** `sdk-c-wrapper/fm_wrapper.c:299-323`

**Test Result:** ✅ First capture now downloads successfully

---

### Bug 2: Cannot Reconnect After Disconnect

**Issue:** After disconnecting, all subsequent connection attempts would fail with:
```
fm_connect: XSDK_SetPriorityMode failed with code: -1
```

Only fix was to physically disconnect USB and power off camera.

**Root Cause:** Fujifilm cameras have two control modes:
- **CAMERA mode** (0x0001) - User has control
- **PC mode** (0x0002) - Software has control

We were setting PC mode on connect but never restoring CAMERA mode on disconnect. The camera stayed in PC mode, blocking subsequent connections.

**Solution:** Restore CAMERA priority before closing connection:
```c
int fm_disconnect() {
    // CRITICAL: Restore camera to CAMERA priority mode before closing
    // Without this, the camera stays in PC mode and reconnection fails
    #define XSDK_PRIORITY_CAMERA 0x0001

    printf("Restoring camera to CAMERA priority mode...\n");
    long result = XSDK_SetPriorityMode(g_hCamera, XSDK_PRIORITY_CAMERA);
    if (result != 0) {
        fprintf(stderr, "Warning - failed to restore camera priority\n");
    }

    result = XSDK_Close(g_hCamera);
    // ... rest of disconnect logic
}
```

**Fix Location:** `sdk-c-wrapper/fm_wrapper.c:179-206`

**Test Result:** ✅ Disconnect → Connect cycle now works perfectly

---

## 📊 Story C-2 Acceptance Criteria

### Completed (5/9 criteria)

- [x] **Capture() triggers actual shutter** - Working with XSDK_RELEASE_SHOOT_S1OFF mode
- [x] **.RAF file downloads successfully** - Images downloaded from camera buffer
- [x] **File size is 15-40MB (valid RAW)** - Confirmed 17-23MB files
- [x] **File opens in viewer** - Both test images verified
- [x] **Session numbering correct** - Sequential naming working (0001, 0002)

### Pending (4/9 criteria)

- [ ] **SetShutter() sets shutter speed** - Function exists but not tested
- [ ] **Test SetISO() sets ISO value** - Need to implement fm_set_iso() and fm_get_iso()
- [ ] **Shutter speed can be read** - Need to implement fm_get_shutter()
- [ ] **ISO value can be read** - Need to implement fm_get_iso()

**Current Progress:** 5/9 criteria met (56%)

---

## 🔑 Key Technical Discoveries

### 1. XSDK_Release Requires Complete Shutter Sequence

From SDK manual page 113:
> "To operate full-press shutter button, half-shutter control prior to the full-press shutter is required."

**Solution:** Use `XSDK_RELEASE_SHOOT_S1OFF` (0x0104) which performs:
- Half-press (S1) for focus/metering
- Full-press (S2) to capture
- Release (S1OFF)

All in one API call.

### 2. Buffer Polling is Critical

The SDK returns immediately from `XSDK_Release`, but the image isn't ready yet. Must poll buffer:
```c
XSDK_GetBufferCapacity(g_hCamera, &shoot_frames, &total_frames);
if (shoot_frames > 0) {
    // Image is ready to download
}
```

Typical delay: 100-300ms for X-T3 RAW files.

### 3. Priority Mode Management

**PC Priority Mode** (required for capture):
```c
XSDK_SetPriorityMode(g_hCamera, XSDK_PRIORITY_PC);  // 0x0002
```

**MUST restore CAMERA mode before disconnect:**
```c
XSDK_SetPriorityMode(g_hCamera, XSDK_PRIORITY_CAMERA);  // 0x0001
```

Otherwise camera stays locked in PC mode.

### 4. AF Status Feedback

The `pStatus` parameter of `XSDK_Release` returns autofocus status:
- **0** = AF_FAILURE (focus didn't lock)
- **1** = AF_OK (focus locked successfully)
- **2** = AF_UNCHECK (AF status not available)

When AF fails, the camera may not save the image.

---

## ✅ ISO and Shutter Speed Getter/Setter Implementation (2025-11-08)

### Completed Implementation

**C Wrapper Functions** (sdk-c-wrapper/fm_wrapper.c):
- ✅ `fm_get_shutter(int* seconds)` - Reads current shutter speed via XSDK_GetShutterSpeed
  - Converts SDK microseconds to seconds with rounding
  - Returns shutter speed in seconds
- ✅ `fm_get_iso(int* iso)` - Reads current ISO via XSDK_GetSensitivity
  - Returns ISO value directly from camera
- ✅ `fm_set_iso(int iso)` - Sets ISO via XSDK_SetSensitivity
  - Range validation: 100-51200
  - Already had `fm_set_shutter()` from initial implementation

**Go SDK Bindings** (pkg/sdk/sdk.go):
- ✅ `GetShutter() (int, error)` - Go wrapper for fm_get_shutter
- ✅ `GetISO() (int, error)` - Go wrapper for fm_get_iso
- ✅ `SetISO(iso int) error` - Go wrapper for fm_set_iso with validation
- ✅ Already had `SetShutter(seconds int) error`

**HAL Layer** (pkg/hal/):
- ✅ Updated Camera interface with all 4 methods (GetShutter, SetShutter, GetISO, SetISO)
- ✅ RealCamera implementation - wraps SDK with thread-safety
- ✅ FakeCamera implementation - simulates settings for testing (default ISO: 800)

**Build Status:**
- ✅ C wrapper rebuilt: fmwrapper.dll (248KB)
- ✅ Import library: libfmwrapper.a (7.1KB)
- ✅ Go application compiled successfully
- ✅ DLLs copied to bin/ directory

### Pending Tasks

1. **Add CLI commands** - Expose new functions in interactive shell
   - Need commands: `get-shutter`, `set-shutter`, `get-iso`, `set-iso`

2. **Integration testing with X-T3** - Test read → set → read verification cycle
   - Test shutter speeds: 1s, 5s, 10s, 30s (astrophotography)
   - Test ISO values: 100, 200, 400, 800, 1600, 3200

3. **Update STORIES.md** - Mark remaining C-2 criteria as complete when tested

---

## 🎯 Success Criteria for C-2 Completion

To complete Story C-2, we need:

- [x] Capture triggers camera shutter ✅ WORKING
- [x] RAF file downloads successfully ✅ WORKING
- [x] File size valid (15-40MB) ✅ WORKING
- [x] File opens in viewer ✅ WORKING
- [x] Session numbering works ✅ WORKING
- [x] Shutter speed get/set implemented ✅ DONE (needs hardware testing)
- [x] ISO get/set implemented ✅ DONE (needs hardware testing)
- [ ] Error messages helpful (mostly done)
- [ ] CLI commands added for ISO/shutter control
- [ ] Hardware testing with X-T3

**Current Status:** All C wrapper, SDK bindings, and HAL implementations complete (7/9 criteria). Need CLI commands and hardware testing for full C-2 completion.

---

## 🔧 Useful Commands

**Test capture workflow:**
```bash
cd /c/Users/cfonseca/Documents/fujimatic/bin
./fujimatic.exe

> connect
> capture
> capture
> capture
> disconnect
> exit
```

**Rebuild after changes:**
```bash
export PATH="/c/msys64/mingw64/bin:$PATH"
cd /c/Users/cfonseca/Documents/fujimatic

# Build C wrapper
gcc -shared -o libs/windows/fmwrapper.dll sdk-c-wrapper/fm_wrapper.c \
    -I sdk-c-wrapper -I sdk/HEADERS \
    -L sdk/REDISTRIBUTABLES/Windows/64bit -lXAPI \
    -Wl,--out-implib,libs/windows/libfmwrapper.a -D_WIN32

# Copy DLL to bin
cp libs/windows/fmwrapper.dll bin/fmwrapper.dll

# Build Go app
go build -o bin/fujimatic.exe ./cmd/fujimatic
```

**Check captured images:**
```bash
ls -lh bin/captures/*.RAF
```
