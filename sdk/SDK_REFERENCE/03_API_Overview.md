# Fujifilm SDK API Overview

## API Organization

APIs are classified into two categories:

1. **COMMON APIs** - Mandatory functions supported by ALL cameras (including X-T3)
2. **MODEL DEPENDENT APIs** - Optional functions that vary by camera model

## Header Files

| File | Purpose | Usage |
|------|---------|-------|
| `XAPI.h` | All COMMON APIs | Required - include in all code |
| `XAPIOpt.h` | Optional API helpers | Referenced by model-specific headers |
| `(model).h` | Model-specific functions | Example: `GFX100S.h`, `XT3.h` (if exists) |

**For fujimatic X-T3:** Start with `XAPI.h` only - contains all essential functions.

## Library Architecture

```
Application
    ↓
XAPI.dll (main SDK library - load this one)
    ↓
├─ FF0000API.dll (camera-specific)
├─ FF0001API.dll (camera-specific)
├─ FF0002API.dll (camera-specific)
└─ ... (20+ camera-specific DLLs)
    ↓
├─ FTLPTP.dll (USB protocol)
└─ FTLPTPIP.dll (Network protocol)
```

**Important:** Only load `XAPI.dll` - it automatically loads all dependencies.

---

## COMMON APIs - Complete Function List

### Initialize/Finalize (2 functions)

| Function | Description | When to Call |
|----------|-------------|--------------|
| `XSDK_Init` | Initializes SDK | Once at startup (S0→S1) |
| `XSDK_Exit` | Finalizes SDK | Once at shutdown (S1→S0) |

### Enumeration (2 functions)

| Function | Description | When to Call |
|----------|-------------|--------------|
| `XSDK_Detect` | Scans for cameras, builds internal list | After init, or to refresh list (S1→S2) |
| `XSDK_Append` | Updates camera list | To add newly connected cameras |

**Note:** `XSDK_Detect` and `XSDK_Append` not supported on Android (single camera only).

### Session Management (4 functions)

| Function | Description | When to Call |
|----------|-------------|--------------|
| `XSDK_OpenEx` | Opens session with specific camera | After detection (S2→S3) |
| `XSDK_SetUSBDeviceHandle` | Android-only alternative to OpenEx | Android apps only |
| `XSDK_Close` | Closes camera session cleanly | When done with camera (S3→S2) |
| `XSDK_PowerOFF` | Closes session AND powers off camera | Shutdown camera remotely |

### Basic Functions (3 functions)

| Function | Description | Returns |
|----------|-------------|---------|
| `XSDK_GetErrorNumber` | Gets error code from last function call | Error code integer |
| `XSDK_GetErrorDetails` | Gets detailed busy error info | Error details when RUNNING_OTHER_FUNCTION |
| `XSDK_GetVersionString` | Gets SDK version string | Version as string |

### Device Information (6 functions)

| Function | Description | Returns |
|----------|-------------|---------|
| `XSDK_GetDeviceInfo` | Gets camera information | Camera model, serial, etc. |
| `XSDK_GetDeviceInfoEx` | Gets camera info + supported APIs | Extended device info |
| `XSDK_WriteDeviceName` | Assigns custom name to camera | Status |
| `XSDK_GetFirmwareVersion` | Gets camera firmware version | Version string |
| `XSDK_GetLensInfo` | Gets attached lens information | Lens model, focal length, etc. |
| `XSDK_GetLensVersion` | Gets lens firmware version | Version string |

**For Battery:** Check `XSDK_GetDeviceInfo` or `XSDK_GetDeviceInfoEx` - may include battery level in device info structure.

### Camera Operation Mode (8 functions)

| Function | Description |
|----------|-------------|
| `XSDK_CapPriorityMode` | Query supported priority modes (PC vs Camera control) |
| `XSDK_SetPriorityMode` | Set priority mode |
| `XSDK_GetPriorityMode` | Get current priority mode |
| `XSDK_CapDriveMode` | Query supported drive modes (single, burst, etc.) |
| `XSDK_SetDriveMode` | Set drive mode |
| `XSDK_GetDriveMode` | Get current drive mode |
| `XSDK_CapMode` | Query supported camera modes (P/A/S/M) |
| `XSDK_SetMode` | Set camera mode |
| `XSDK_GetMode` | Get current camera mode |

**Priority Modes:**
- **PC Priority:** Computer controls camera (use `XSDK_Release`)
- **Camera Priority:** Camera can be used independently (use `XSDK_ReleaseEx`)

### Release Control (5 functions) ⭐ **CAPTURE FUNCTIONS**

| Function | Description | Priority Mode |
|----------|-------------|---------------|
| `XSDK_CapRelease` | Query supported release operations | PC Priority |
| `XSDK_Release` | **Trigger shutter/AF-L/AE-L** | PC Priority |
| `XSDK_CapReleaseEx` | Query supported release operations | Camera Priority |
| `XSDK_ReleaseEx` | **Trigger shutter/AF-L/AE-L** | Camera Priority |
| `XSDK_GetReleaseStatus` | Get status of release operation | Both modes |

**This is how you capture images!** Call `XSDK_Release()` or `XSDK_ReleaseEx()` to trigger shutter.

### Image Acquisition (5 functions) ⭐ **DOWNLOAD FUNCTIONS**

| Function | Description |
|----------|-------------|
| `XSDK_ReadImageInfo` | Get info about image at top of camera buffer |
| `XSDK_ReadPreview` | Get low-res preview of image in buffer |
| `XSDK_ReadImage` | **Download full image from buffer (and delete from buffer)** |
| `XSDK_DeleteImage` | Delete image from top of buffer without downloading |
| `XSDK_GetBufferCapacity` | Get status of in-camera buffer |

**Download Flow:**
1. Capture image → goes into camera's internal buffer
2. `XSDK_ReadImageInfo` → check what's available
3. `XSDK_ReadImage` → downloads full RAW/JPEG and removes from buffer

### Exposure Control (18 functions)

<details>
<summary>Click to expand exposure functions</summary>

| Function | Description |
|----------|-------------|
| `XSDK_CapAEMode` | Query exposure modes (P/A/S/M) |
| `XSDK_SetAEMode` | Set exposure mode |
| `XSDK_GetAEMode` | Get exposure mode |
| `XSDK_CapShutterSpeed` | Query available shutter speeds |
| `XSDK_SetShutterSpeed` | **Set shutter speed** ⭐ |
| `XSDK_GetShutterSpeed` | Get shutter speed |
| `XSDK_CapExposureBias` | Query exposure compensation range |
| `XSDK_SetExposureBias` | Set exposure compensation |
| `XSDK_GetExposureBias` | Get exposure compensation |
| `XSDK_CapDynamicRange` | Query dynamic range options |
| `XSDK_SetDynamicRange` | Set dynamic range |
| `XSDK_GetDynamicRange` | Get dynamic range |
| `XSDK_CapSensitivity` | Query ISO values |
| `XSDK_SetSensitivity` | Set ISO |
| `XSDK_GetSensitivity` | Get ISO |
| `XSDK_CapMeteringMode` | Query metering modes |
| `XSDK_SetMeteringMode` | Set metering mode |
| `XSDK_GetMeteringMode` | Get metering mode |
| `XSDK_CapLensZoomPos` | Query zoom positions |
| `XSDK_SetLensZoomPos` | Set zoom position |
| `XSDK_GetLensZoomPos` | Get zoom position |
| `XSDK_CapAperture` | Query aperture values |
| `XSDK_SetAperture` | Set aperture (f-stop) |
| `XSDK_GetAperture` | Get aperture |

</details>

### White Balance Control (6 functions)

| Function | Description |
|----------|-------------|
| `XSDK_CapWBMode` | Query white balance modes |
| `XSDK_SetWBMode` | Set white balance mode |
| `XSDK_GetWBMode` | Get white balance mode |
| `XSDK_CapWBColorTemp` | Query color temp range (when WB=ColorTemp) |
| `XSDK_SetWBColorTemp` | Set color temperature |
| `XSDK_GetWBColorTemp` | Get color temperature |

### Media Recording Control (3 functions)

| Function | Description |
|----------|-------------|
| `XSDK_CapMediaRecord` | Query media recording options |
| `XSDK_SetMediaRecord` | Control where images are saved (camera SD card, computer, both) |
| `XSDK_GetMediaRecord` | Get current media recording setting |

**Important for tethering:** Set to save images to computer memory (not SD card).

### Operation Mode Control (2 functions)

| Function | Description |
|----------|-------------|
| `XSDK_CapForceMode` | Query supported operation modes |
| `XSDK_SetForceMode` | Force camera into SHOOTING MODE |

### Backup and Restore (2 functions)

| Function | Description |
|----------|-------------|
| `XSDK_SetBackupSettings` | Restore camera settings from backup |
| `XSDK_GetBackupSettings` | Backup all camera settings |

### Movie Control (18 functions)

Similar to exposure control but for video mode. Skip for MVP (still photo only).

### Model Dependent Function Interface (3 functions)

| Function | Description |
|----------|-------------|
| `XSDK_CapProp` | Query supported values for model-specific function |
| `XSDK_SetProp` | Set model-specific property |
| `XSDK_GetProp` | Get model-specific property |

**Use these to access:**
- Battery level (if not in GetDeviceInfo)
- Focus control
- Advanced camera settings

---

## MODEL DEPENDENT APIs (Optional)

These are called via `XSDK_CapProp`, `XSDK_SetProp`, `XSDK_GetProp` with property IDs.

**Focus Control** (most extensive optional API):
- Focus mode (AF-S, AF-C, MF)
- AF mode (single point, zone, wide/tracking)
- Focus area selection
- Face detection
- Eye AF
- Subject detection

**Note:** X-T3 support varies - check with `XSDK_CapProp`.

---

## Essential Functions for fujimatic MVP

### Story C-1: Connect/Disconnect/Battery

**Minimum required:**
```c
1. XSDK_Init()                    // Initialize SDK
2. XSDK_Detect()                  // Find cameras
3. XSDK_OpenEx()                  // Connect to X-T3
4. XSDK_GetDeviceInfo()           // Get camera info (includes battery?)
   OR
   XSDK_GetProp(BATTERY_PROP_ID)  // Get battery via model-dependent API
5. XSDK_Close()                   // Disconnect
6. XSDK_Exit()                    // Cleanup
```

### Story C-2: Capture (trigger shutter)

**Add these:**
```c
7. XSDK_SetPriorityMode(PC_PRIORITY)  // Let PC control camera
8. XSDK_SetShutterSpeed(speed)        // Set shutter (e.g., 1 second)
9. XSDK_Release()                     // Trigger shutter
10. XSDK_GetReleaseStatus()           // Wait for completion
```

### Story C-3: Download RAF Files

**Add these:**
```c
11. XSDK_SetMediaRecord(TO_COMPUTER)  // Don't save to SD card
12. XSDK_ReadImageInfo()              // Check buffer
13. XSDK_ReadImage()                  // Download RAF file
```

---

## Function Naming Pattern

All functions follow consistent patterns:

| Pattern | Purpose | Example |
|---------|---------|---------|
| `XSDK_Cap*` | Query available options | `XSDK_CapShutterSpeed` → get list of valid speeds |
| `XSDK_Set*` | Set a value | `XSDK_SetShutterSpeed(1000)` → set to 1 second |
| `XSDK_Get*` | Read current value | `XSDK_GetShutterSpeed()` → read current setting |

**Always call `Cap` before `Set`** to ensure the value is supported!

---

## Next Steps

1. **Read Initialize APIs** (4-1-1-Initialize.pdf) - Details on XSDK_Init()
2. **Read Enumeration APIs** (4-1-2-Enumeration.pdf) - Details on XSDK_Detect()
3. **Read Session Management APIs** (4-1-3-SessionManagement.pdf) - Details on XSDK_OpenEx()
4. **Read Device Information APIs** (4-1-5-DeviceInformation.pdf) - Find battery level
5. **Read Release Control APIs** (4-1-7-ReleaseControl.pdf) - Details on capture
6. **Read Image Acquisition APIs** (4-1-8-ImageAcquisition.pdf) - Details on download

## Summary for Implementation

**For our C wrapper (`fm_wrapper.c`):**
- `fm_init()` → calls `XSDK_Init()`
- `fm_connect()` → calls `XSDK_Detect()` + `XSDK_OpenEx()`
- `fm_disconnect()` → calls `XSDK_Close()`
- `fm_get_battery()` → calls `XSDK_GetDeviceInfo()` or `XSDK_GetProp()`
- `fm_set_shutter()` → calls `XSDK_SetShutterSpeed()`
- `fm_capture()` → calls `XSDK_Release()`
- `fm_download_last()` → calls `XSDK_ReadImage()`

We already have the Go bindings (`pkg/sdk`) that call these C functions!
