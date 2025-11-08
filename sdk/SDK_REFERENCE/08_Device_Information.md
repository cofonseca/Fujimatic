# Device Information

## Overview

Device information functions retrieve camera model, firmware, serial numbers, and lens details.

**State Requirement:** All functions can be used in State S3 (camera connected).

---

## XSDK_GetDeviceInfo

**Purpose:** Gets basic information about the connected camera.

### Function Signature

```c
XSDK_APIENTRY XSDK_GetDeviceInfo(
    XSDK_HANDLE hCamera,
    XSDK_DeviceInformation* pDevInfo
);
```

### Device Information Structure

```c
typedef struct {
    char strVendor[256];          // Camera vendor (e.g., "FUJIFILM")
    char strManufacturer[256];    // Camera manufacturer (e.g., "FUJIFILM")
    char strProduct[256];         // Model name (e.g., "GFX100S", "X-T3")
    char strFirmware[256];        // Firmware version (e.g., "1.00")
    char strDeviceType[256];      // Device type (reserved)
    char strSerialNo[256];        // Camera serial number
    char strFramework[256];       // Interface type ("USB" or "Wi-Fi")
    BYTE bDeviceId;               // Device ID (reserved)
    char strDeviceName[32];       // Custom name set with XSDK_WriteDeviceName
    char strYNo[32];              // Reserved
} XSDK_DeviceInformation;
```

### Parameters

| Parameter | Direction | Description |
|-----------|-----------|-------------|
| `hCamera` | IN | Camera handle from `XSDK_OpenEx` |
| `pDevInfo` | OUT | Pointer to device information structure |

### Return Values

- `XSDK_COMPLETE`: Success
- `XSDK_ERROR`: Error

### Usage Example

```c
XSDK_DeviceInformation devInfo;
result = XSDK_GetDeviceInfo(hCamera, &devInfo);

if (result == XSDK_COMPLETE) {
    printf("Vendor: %s\n", devInfo.strVendor);
    printf("Model: %s\n", devInfo.strProduct);
    printf("Serial Number: %s\n", devInfo.strSerialNo);
    printf("Firmware: %s\n", devInfo.strFirmware);
    printf("Interface: %s\n", devInfo.strFramework);

    if (strlen(devInfo.strDeviceName) > 0) {
        printf("Custom Name: %s\n", devInfo.strDeviceName);
    }
}
```

### Notes

- Some information may not be returned for certain models
- Serial number typically available only for USB connections
- Framework shows connection type: "USB", "ETHER" (Ethernet), or "IP" (Wi-Fi)

---

## XSDK_GetDeviceInfoEx

**Purpose:** Gets device information plus list of supported APIs.

### Function Signature

```c
XSDK_APIENTRY XSDK_GetDeviceInfoEx(
    XSDK_HANDLE hCamera,
    XSDK_DeviceInformation* pDevInfo,
    long* plNumAPICode,
    long* plAPICode
);
```

### Parameters

| Parameter | Direction | Description |
|-----------|-----------|-------------|
| `hCamera` | IN | Camera handle |
| `pDevInfo` | OUT | Device information structure (same as `XSDK_GetDeviceInfo`) |
| `plNumAPICode` | OUT | Number of supported API codes |
| `plAPICode` | OUT | Array of supported API codes. If NULL, only returns count. Allocate `sizeof(long) * (*plNumAPICode)` bytes before calling. |

### Return Values

- `XSDK_COMPLETE`: Success
- `XSDK_ERROR`: Error

### Usage Pattern

```c
XSDK_DeviceInformation devInfo;
long numAPIs;
long* apiCodes;

// First call to get count
result = XSDK_GetDeviceInfoEx(hCamera, &devInfo, &numAPIs, NULL);

if (result == XSDK_COMPLETE) {
    printf("Model: %s\n", devInfo.strProduct);
    printf("Supported APIs: %ld\n", numAPIs);

    // Allocate array for API codes
    apiCodes = (long*)malloc(sizeof(long) * numAPIs);

    // Second call to get API codes
    result = XSDK_GetDeviceInfoEx(hCamera, &devInfo, &numAPIs, apiCodes);

    if (result == XSDK_COMPLETE) {
        printf("API Codes:\n");
        for (int i = 0; i < numAPIs; i++) {
            printf("  0x%08lX\n", apiCodes[i]);
        }
    }

    free(apiCodes);
}
```

### Checking Feature Support

```c
bool IsAPISupported(XSDK_HANDLE hCamera, long targetAPI) {
    XSDK_DeviceInformation devInfo;
    long numAPIs;
    long* apiCodes;
    bool supported = false;

    // Get count
    if (XSDK_GetDeviceInfoEx(hCamera, &devInfo, &numAPIs, NULL) != XSDK_COMPLETE) {
        return false;
    }

    // Get API list
    apiCodes = (long*)malloc(sizeof(long) * numAPIs);
    if (XSDK_GetDeviceInfoEx(hCamera, &devInfo, &numAPIs, apiCodes) == XSDK_COMPLETE) {
        // Search for target API
        for (int i = 0; i < numAPIs; i++) {
            if (apiCodes[i] == targetAPI) {
                supported = true;
                break;
            }
        }
    }

    free(apiCodes);
    return supported;
}
```

---

## XSDK_WriteDeviceName

**Purpose:** Assigns a custom unique name to the camera.

The name is stored in camera's non-volatile memory and useful for identifying specific cameras when multiple units are connected.

### Function Signature

```c
XSDK_APIENTRY XSDK_WriteDeviceName(
    XSDK_HANDLE hCamera,
    LPCSTR pDeviceName
);
```

### Parameters

| Parameter | Direction | Description |
|-----------|-----------|-------------|
| `hCamera` | IN | Camera handle |
| `pDeviceName` | IN | Custom name (max 32 characters including NULL terminator) |

### Return Values

- `XSDK_COMPLETE`: Success
- `XSDK_ERROR`: Error

### Usage Example

```c
// Assign unique names to multiple cameras
XSDK_HANDLE hCamera1, hCamera2;

// Open cameras
XSDK_OpenEx("ENUM:0", &hCamera1, &mode, NULL);
XSDK_OpenEx("ENUM:1", &hCamera2, &mode, NULL);

// Assign custom names
XSDK_WriteDeviceName(hCamera1, "Camera-Left");
XSDK_WriteDeviceName(hCamera2, "Camera-Right");

// Later, retrieve names
XSDK_DeviceInformation info1, info2;
XSDK_GetDeviceInfo(hCamera1, &info1);
XSDK_GetDeviceInfo(hCamera2, &info2);

printf("Camera 1: %s\n", info1.strDeviceName);  // "Camera-Left"
printf("Camera 2: %s\n", info2.strDeviceName);  // "Camera-Right"
```

### Multi-Camera Identification Pattern

```c
// Setup phase - assign names
void SetupCameras() {
    XSDK_Detect(XSDK_DSC_IF_USB, NULL, NULL, &count);

    for (int i = 0; i < count; i++) {
        char enumStr[32];
        sprintf(enumStr, "ENUM:%d", i);

        XSDK_HANDLE hCam;
        long mode;
        XSDK_OpenEx(enumStr, &hCam, &mode, NULL);

        // Assign based on serial number or position
        char name[32];
        sprintf(name, "Studio-Camera-%d", i + 1);
        XSDK_WriteDeviceName(hCam, name);

        XSDK_Close(hCam);
    }
}

// Usage phase - find by name
XSDK_HANDLE FindCameraByName(const char* targetName) {
    long count;
    XSDK_Detect(XSDK_DSC_IF_USB, NULL, NULL, &count);

    for (int i = 0; i < count; i++) {
        char enumStr[32];
        sprintf(enumStr, "ENUM:%d", i);

        XSDK_HANDLE hCam;
        long mode;
        XSDK_OpenEx(enumStr, &hCam, &mode, NULL);

        XSDK_DeviceInformation info;
        XSDK_GetDeviceInfo(hCam, &info);

        if (strcmp(info.strDeviceName, targetName) == 0) {
            return hCam;  // Found it
        }

        XSDK_Close(hCam);
    }

    return NULL;  // Not found
}
```

---

## XSDK_GetFirmwareVersion

**Purpose:** Gets camera firmware version as a string.

### Function Signature

```c
XSDK_APIENTRY XSDK_GetFirmwareVersion(
    XSDK_HANDLE hCamera,
    LPSTR pFirmwareVersion
);
```

### Parameters

| Parameter | Direction | Description |
|-----------|-----------|-------------|
| `hCamera` | IN | Camera handle |
| `pFirmwareVersion` | OUT | Firmware version string. Allocate 256-byte buffer. |

### Return Values

- `XSDK_COMPLETE`: Success
- `XSDK_ERROR`: Error

### Usage Example

```c
char firmwareVersion[256];
result = XSDK_GetFirmwareVersion(hCamera, firmwareVersion);

if (result == XSDK_COMPLETE) {
    printf("Camera Firmware: %s\n", firmwareVersion);
    // Example output: "1.00", "2.10", etc.
}
```

---

## XSDK_GetLensInfo

**Purpose:** Gets information about the lens attached to the camera.

### Function Signature

```c
XSDK_APIENTRY XSDK_GetLensInfo(
    XSDK_HANDLE hCamera,
    XSDK_LensInformation* pLensInfo
);
```

### Lens Information Structure

```c
typedef struct {
    char strModel[20];              // Reserved
    char strProductName[100];       // Lens model name
    char strSerialNo[20];           // Lens serial number
    long lISCapability;             // Image stabilization: 1=Present, 0=Not present
    long lMFCapability;             // Manual focus switch: 1=Present, 0=Not present
    long lZoomPosCapability;        // SetZoomPos enabled: 1=Yes, 0=No
} XSDK_LensInformation;
```

### Parameters

| Parameter | Direction | Description |
|-----------|-----------|-------------|
| `hCamera` | IN | Camera handle |
| `pLensInfo` | OUT | Lens information structure |

### Return Values

- `XSDK_COMPLETE`: Success
- `XSDK_ERROR`: Error

### Usage Example

```c
XSDK_LensInformation lensInfo;
result = XSDK_GetLensInfo(hCamera, &lensInfo);

if (result == XSDK_COMPLETE) {
    printf("Lens: %s\n", lensInfo.strProductName);

    if (strlen(lensInfo.strSerialNo) > 0) {
        printf("Serial: %s\n", lensInfo.strSerialNo);
    }

    printf("Image Stabilization: %s\n",
           lensInfo.lISCapability ? "Yes" : "No");
    printf("Manual Focus Switch: %s\n",
           lensInfo.lMFCapability ? "Yes" : "No");
    printf("Zoom Control: %s\n",
           lensInfo.lZoomPosCapability ? "Supported" : "Not supported");
}
```

### Special Cases

**Built-in Lens (Fixed Lens Cameras):**
- `strModel[0]` and `strSerialNo[0]` are NULL
- `strProductName` contains lens info (e.g., "FUJINON LENS 4.0x f=6.4-25.6mm 1:1.8-4.9")

**Leica M Mount with Adapter:**
- `strModel[0]` and `strSerialNo[0]` are NULL
- `strProductName` shows adapter setting (e.g., "28mm")

### Lens Detection Pattern

```c
void PrintLensType(XSDK_HANDLE hCamera) {
    XSDK_LensInformation lensInfo;

    if (XSDK_GetLensInfo(hCamera, &lensInfo) != XSDK_COMPLETE) {
        printf("Failed to get lens info\n");
        return;
    }

    if (strlen(lensInfo.strSerialNo) == 0) {
        // No serial number
        if (strstr(lensInfo.strProductName, "FUJINON") != NULL) {
            printf("Built-in lens: %s\n", lensInfo.strProductName);
        } else {
            printf("Manual lens or adapter: %s\n", lensInfo.strProductName);
        }
    } else {
        // Has serial number - interchangeable lens
        printf("Lens: %s (S/N: %s)\n",
               lensInfo.strProductName,
               lensInfo.strSerialNo);
    }
}
```

---

## XSDK_GetLensVersion

**Purpose:** Gets firmware version of the attached lens.

### Function Signature

```c
XSDK_APIENTRY XSDK_GetLensVersion(
    XSDK_HANDLE hCamera,
    LPSTR pLensVersion
);
```

### Parameters

| Parameter | Direction | Description |
|-----------|-----------|-------------|
| `hCamera` | IN | Camera handle |
| `pLensVersion` | OUT | Lens firmware version. Allocate 256-byte buffer. |

### Return Values

- `XSDK_COMPLETE`: Success
- `XSDK_ERROR`: Error

### Usage Example

```c
char lensVersion[256];
result = XSDK_GetLensVersion(hCamera, lensVersion);

if (result == XSDK_COMPLETE) {
    if (strlen(lensVersion) > 0) {
        printf("Lens Firmware: %s\n", lensVersion);
    } else {
        printf("Lens firmware version not available\n");
    }
}
```

---

## Complete System Information Report

```c
void PrintSystemInfo(XSDK_HANDLE hCamera) {
    printf("=== Camera System Information ===\n\n");

    // SDK Version
    char sdkVersion[256];
    if (XSDK_GetVersionString(sdkVersion) == XSDK_COMPLETE) {
        printf("SDK Version: %s\n", sdkVersion);
    }

    // Device Info
    XSDK_DeviceInformation devInfo;
    if (XSDK_GetDeviceInfo(hCamera, &devInfo) == XSDK_COMPLETE) {
        printf("\nCamera:\n");
        printf("  Model: %s\n", devInfo.strProduct);
        printf("  Serial Number: %s\n", devInfo.strSerialNo);
        printf("  Interface: %s\n", devInfo.strFramework);
        if (strlen(devInfo.strDeviceName) > 0) {
            printf("  Custom Name: %s\n", devInfo.strDeviceName);
        }
    }

    // Firmware Version
    char fwVersion[256];
    if (XSDK_GetFirmwareVersion(hCamera, fwVersion) == XSDK_COMPLETE) {
        printf("  Firmware: %s\n", fwVersion);
    }

    // Lens Info
    XSDK_LensInformation lensInfo;
    if (XSDK_GetLensInfo(hCamera, &lensInfo) == XSDK_COMPLETE) {
        printf("\nLens:\n");
        printf("  Model: %s\n", lensInfo.strProductName);
        if (strlen(lensInfo.strSerialNo) > 0) {
            printf("  Serial Number: %s\n", lensInfo.strSerialNo);
        }
        printf("  Image Stabilization: %s\n",
               lensInfo.lISCapability ? "Yes" : "No");
        printf("  Manual Focus: %s\n",
               lensInfo.lMFCapability ? "Yes" : "No");
    }

    // Lens Firmware
    char lensVersion[256];
    if (XSDK_GetLensVersion(hCamera, lensVersion) == XSDK_COMPLETE) {
        if (strlen(lensVersion) > 0) {
            printf("  Firmware: %s\n", lensVersion);
        }
    }

    printf("\n");
}
```

---

## Best Practices

1. **Cache device information** - Don't query repeatedly if info doesn't change
2. **Use custom names** for multi-camera setups
3. **Check lens capabilities** before attempting lens control operations
4. **Handle missing information** gracefully (some fields may be empty)
5. **Log system info** at startup for debugging purposes
