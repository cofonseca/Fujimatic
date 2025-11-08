# Session Management

## Overview

Session management functions establish and terminate connections between the computer and camera.

**State Requirements:**
- `XSDK_OpenEx` / `XSDK_SetUSBDeviceHandle`: Can be used in State S2
- `XSDK_Close`: Can be used in State S3
- `XSDK_PowerOFF`: Can be used in State S3

---

## XSDK_OpenEx

**Purpose:** Establishes a session between the camera and computer.

**Note:** Not supported on Android OS - use `XSDK_SetUSBDeviceHandle()` instead.

### Function Signature

```c
XSDK_APIENTRY XSDK_OpenEx(
    LPCSTR pDevice,
    XSDK_HANDLE* phCamera,
    long* plCameraMode,
    void* pOption
);
```

### Parameters

| Parameter | Direction | Description |
|-----------|-----------|-------------|
| `pDevice` | IN | Camera identifier:<br>**By Index:** `"ENUM:<number>"` where number is 0 to (count-1)<br>**By IP:** `"IPv4:<ip address>"` (e.g., `"IPv4:192.168.0.1"`) |
| `phCamera` | OUT | Camera handle for subsequent API calls |
| `plCameraMode` | OUT | Bitmap of supported features:<br>`XSDK_DSC_MODE_TETHER` - Tethered shooting supported<br>`XSDK_DSC_MODE_RAW` - USB RAW DEV mode<br>`XSDK_DSC_MODE_BR` - Backup/Restore supported<br>`XSDK_DSC_MODE_WEBCAM` - Webcam mode (limited tethering) |
| `pOption` | IN | Reserved - set to NULL |

### Return Values

- `XSDK_COMPLETE`: Success
- `XSDK_ERROR`: Error

### Usage Examples

```c
// Open first detected camera
XSDK_HANDLE hCamera;
long cameraMode;
result = XSDK_OpenEx("ENUM:0", &hCamera, &cameraMode, NULL);

if (result == XSDK_COMPLETE) {
    // Check capabilities
    if (cameraMode & XSDK_DSC_MODE_TETHER) {
        printf("Tethered shooting supported\n");
    }
    if (cameraMode & XSDK_DSC_MODE_BR) {
        printf("Backup/Restore supported\n");
    }
}

// Open camera by IP address
result = XSDK_OpenEx("IPv4:192.168.1.100", &hCamera, &cameraMode, NULL);

// Open second camera
XSDK_HANDLE hCamera2;
result = XSDK_OpenEx("ENUM:1", &hCamera2, &cameraMode, NULL);
```

---

## XSDK_SetUSBDeviceHandle (Android Only)

**Purpose:** Opens camera connection on Android OS using USB device handle.

**Note:** Only supported on Android OS SDK. Called from alternative code for `XSDK_OpenEx`.

### Function Signature

```c
XSDK_APIENTRY XSDK_SetUSBDeviceHandle(
    long lFileDescriptor,
    unsigned char* pRawDescriptors,
    XSDK_HANDLE* phCamera
);
```

### Parameters

| Parameter | Direction | Description |
|-----------|-----------|-------------|
| `lFileDescriptor` | IN | Native file descriptor from `UsbDeviceConnection.getFileDescriptor()` |
| `pRawDescriptors` | IN | Raw USB descriptors from `UsbDeviceConnection.getRawDescriptors()` |
| `phCamera` | OUT | Camera handle when function completes |

### Return Values

- `XSDK_COMPLETE`: Success
- `XSDK_ERROR`: Error

### Android Implementation Pattern (Kotlin)

```kotlin
// In MainActivity
private fun openCamera(): Long {
    errordetails = AndroidSDKErrorNumber.ERRCODE_NOERR

    try {
        if (usbDeviceConnection != null) {
            errordetails = AndroidSDKErrorNumber.ERRCODE_SEQUENCE
            return AndroidSDKResult.ERROR
        }

        if (detectedDevice == null) {
            errordetails = AndroidSDKErrorNumber.ERRCODE_SEQUENCE
            return AndroidSDKResult.ERROR
        }

        val requiredDevice = detectedDevice!!

        if (!usbManager.hasPermission(requiredDevice)) {
            errordetails = AndroidSDKErrorNumber.ERRCODE_PERMISSION
            return AndroidSDKResult.ERROR
        }

        usbDeviceConnection = usbManager.openDevice(requiredDevice)
        val fd = usbDeviceConnection!!.fileDescriptor
        val descriptors = usbDeviceConnection!!.rawDescriptors

        return CameraControl.setUSBDeviceHandle(fd.toLong(), descriptors, hCamera)
    } catch (e: Exception) {
        errordetails = AndroidSDKErrorNumber.ERRCODE_UNKNOWN
        return AndroidSDKResult.ERROR
    }
}
```

### Android Error Codes

```kotlin
object AndroidSDKErrorNumber {
    const val ERRCODE_NOERR = 0x00000000L
    const val ERRCODE_SEQUENCE = 0x00001001L      // Wrong call sequence
    const val ERRCODE_PERMISSION = 0x00001002L    // Permission denied
    const val ERRCODE_UNKNOWN = 0x00009100L       // Unknown error
}
```

---

## XSDK_Close

**Purpose:** Closes the session between camera and computer.

### Function Signature

```c
XSDK_APIENTRY XSDK_Close(XSDK_HANDLE hCamera);
```

### Parameters

| Parameter | Direction | Description |
|-----------|-----------|-------------|
| `hCamera` | IN | Camera handle from `XSDK_OpenEx` |

### Return Values

- `XSDK_COMPLETE`: Success
- `XSDK_ERROR`: Error

### Important Notice

**CRITICAL:** Wait at least 600ms after calling `XSDK_Close()` before calling `XSDK_Exit()`.

```c
// Proper shutdown sequence
XSDK_Close(hCamera);
Sleep(600);  // REQUIRED DELAY
XSDK_Exit();
```

### Usage Example

```c
// Close connection
result = XSDK_Close(hCamera);
if (result != XSDK_COMPLETE) {
    // Handle error
}

// Wait before finalizing SDK
#ifdef _WIN32
    Sleep(600);
#else
    usleep(600000);
#endif

// Now safe to exit SDK
XSDK_Exit();
```

---

## XSDK_PowerOFF

**Purpose:** Closes the session and shuts down the camera.

### Function Signature

```c
XSDK_APIENTRY XSDK_PowerOFF(XSDK_HANDLE hCamera);
```

### Parameters

| Parameter | Direction | Description |
|-----------|-----------|-------------|
| `hCamera` | IN | Camera handle from `XSDK_OpenEx` |

### Return Values

- `XSDK_COMPLETE`: Success
- `XSDK_ERROR`: Error

### Behavior

During shutdown:
1. Camera settings saved to non-volatile memory
2. Counter logs saved
3. Camera powers off

To use camera again:
- **Option 1:** Turn camera off, wait a few moments, turn on again
- **Option 2:** Press and hold shutter button (or remote release) for over 2 seconds

### Usage Example

```c
// Graceful shutdown with camera power off
result = XSDK_PowerOFF(hCamera);

if (result == XSDK_COMPLETE) {
    printf("Camera powered off successfully\n");
    printf("Turn camera back on manually to reconnect\n");
}

// Still need to wait before SDK exit
Sleep(600);
XSDK_Exit();
```

---

## Session Lifecycle

```
State S2 (Cameras Detected)
  |
  | XSDK_OpenEx() or XSDK_SetUSBDeviceHandle() [Android]
  v
State S3 (Camera Connected)
  |
  | ... perform camera operations ...
  |
  +-- XSDK_Close() ----------> Back to S2 (camera still on)
  |
  +-- XSDK_PowerOFF() --------> Back to S2 (camera powered off)

After close:
  Sleep(600ms) - REQUIRED
  |
  v
XSDK_Exit() -> State S0
```

---

## Multi-Camera Support

```c
// Open multiple cameras
XSDK_HANDLE hCamera1, hCamera2;
long mode1, mode2;

XSDK_OpenEx("ENUM:0", &hCamera1, &mode1, NULL);
XSDK_OpenEx("ENUM:1", &hCamera2, &mode2, NULL);

// Use cameras independently
XSDK_StartShutterRelease(hCamera1);
XSDK_StartShutterRelease(hCamera2);

// Close both
XSDK_Close(hCamera1);
XSDK_Close(hCamera2);
Sleep(600);
XSDK_Exit();
```

---

## Error Handling

```c
XSDK_HANDLE hCamera;
long cameraMode;

result = XSDK_OpenEx("ENUM:0", &hCamera, &cameraMode, NULL);

if (result != XSDK_COMPLETE) {
    long apiCode, errCode;
    XSDK_GetErrorNumber(NULL, &apiCode, &errCode);

    switch (errCode) {
        case XSDK_ERRCODE_COMMUNICATION:
            printf("Communication error - check connection\n");
            break;
        case XSDK_ERRCODE_TIMEOUT:
            printf("Timeout - camera not responding\n");
            break;
        case XSDK_ERRCODE_BUSY:
            printf("Camera busy\n");
            break;
        default:
            printf("Error code: 0x%08lX\n", errCode);
    }
}
```

---

## Best Practices

1. **Always check return values** from open/close operations
2. **Wait 600ms** after `XSDK_Close()` before `XSDK_Exit()`
3. **Use XSDK_PowerOFF()** when you want camera to shut down (saves battery)
4. **Handle Android differently** - use `XSDK_SetUSBDeviceHandle()` with USB permission flow
5. **Check camera mode bitmap** after opening to verify supported features
6. **Close cameras before exit** - don't leave sessions open
