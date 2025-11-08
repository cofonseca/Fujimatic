# Camera Enumeration

## Overview

Camera enumeration functions detect and list connected cameras via USB or network interfaces.

**State Requirements:**
- `XSDK_Detect`: Can be used in States S1 and S2
- `XSDK_Append`: Can be used in State S2

**Platform Notes:**
- Android OS has different implementation patterns (see Alternative Codes sections in original docs)
- macOS 15 on M1/M2: Allow at least 3000ms between `XSDK_Init()` and `XSDK_Detect()`

---

## XSDK_Detect

**Purpose:** Enumerates available cameras and generates a connected camera list.

### Function Signature

```c
XSDK_APIENTRY XSDK_Detect(
    long lInterface,
    LPSTR pInterface,
    LPSTR pDeviceName,
    long* plCount
);
```

### Parameters

| Parameter | Direction | Description |
|-----------|-----------|-------------|
| `lInterface` | IN | Interface type (use BIT-OR to combine):<br>`XSDK_DSC_IF_USB` - USB<br>`XSDK_DSC_IF_WIFI_LOCAL` - Network (local segment)<br>`XSDK_DSC_IF_WIFI_IP` - Network (specific IP) |
| `pInterface` | IN | IP address specification (only when `XSDK_DSC_IF_WIFI_IP` is set):<br>- Comma-separated: `"192.168.100.32, 192.168.100.33"`<br>- Range: `"192.168.100"` (means .1 to .254)<br>- Range: `"192.168.100.21-192.168.100.30"`<br>**Note:** SDK 1.4.0.0+ only supports single IP address<br>Set to NULL if not using IP detection |
| `pDeviceName` | IN | Target device model name (e.g., `"GFX100S"`).<br>Set to NULL to enumerate all devices. |
| `plCount` | OUT | Number of devices detected |

### Return Values

- `XSDK_COMPLETE`: Success
- `XSDK_ERROR`: Error

### Usage Examples

```c
// Detect all USB cameras
long count;
result = XSDK_Detect(XSDK_DSC_IF_USB, NULL, NULL, &count);
printf("Found %ld USB cameras\n", count);

// Detect specific model via USB
result = XSDK_Detect(XSDK_DSC_IF_USB, NULL, "X-T3", &count);

// Detect cameras on local network
result = XSDK_Detect(XSDK_DSC_IF_WIFI_LOCAL, NULL, NULL, &count);

// Detect camera at specific IP
result = XSDK_Detect(XSDK_DSC_IF_WIFI_IP, "192.168.1.100", NULL, &count);

// Detect via USB AND network
long interface = XSDK_DSC_IF_USB | XSDK_DSC_IF_WIFI_LOCAL;
result = XSDK_Detect(interface, NULL, NULL, &count);
```

### Camera Identification

After detection, cameras are referenced by index: `"ENUM:0"`, `"ENUM:1"`, etc.

```c
// Detect cameras
long count;
XSDK_Detect(XSDK_DSC_IF_USB, NULL, NULL, &count);

// Open first detected camera
XSDK_HANDLE hCamera;
long cameraMode;
XSDK_OpenEx("ENUM:0", &hCamera, &cameraMode, NULL);
```

---

## XSDK_Append

**Purpose:** Updates the connected camera list without clearing existing entries.

**Note:** Not supported on Android OS (only supports one camera connection).

### Function Signature

```c
XSDK_APIENTRY XSDK_Append(
    long lInterface,
    LPSTR pInterface,
    LPSTR pDeviceName,
    long* plCount,
    XSDK_DeviceStatus* pDeviceList
);
```

### Parameters

| Parameter | Direction | Description |
|-----------|-----------|-------------|
| `lInterface` | IN | Same as `XSDK_Detect` |
| `pInterface` | IN | Same as `XSDK_Detect` |
| `pDeviceName` | IN | Same as `XSDK_Detect` |
| `plCount` | IN/OUT | **IN:** Allocated array size<br>**OUT:** Number of devices in list |
| `pDeviceList` | OUT | Array of device status structures.<br>If NULL, returns maximum field count in `plCount` |

### Device Status Structure

```c
typedef struct {
    char strProduct[256];      // Model name
    char strSerialNo[256];     // Serial number (USB only)
    char strIPAddress[256];    // IPv4 address (network only)
    char strFramework[256];    // "USB" / "ETHER" / "IP"
    bool bValid;              // true: valid, false: invalid
} XSDK_DeviceStatus;
```

### Usage Pattern

```c
// First call to get count
long count;
XSDK_Append(XSDK_DSC_IF_USB, NULL, NULL, &count, NULL);

// Allocate array
XSDK_DeviceStatus* devices = malloc(sizeof(XSDK_DeviceStatus) * count);

// Second call to get device list
XSDK_Append(XSDK_DSC_IF_USB, NULL, NULL, &count, devices);

// Iterate devices
for (int i = 0; i < count; i++) {
    if (devices[i].bValid) {
        printf("Device %d: %s (%s)\n", i,
               devices[i].strProduct,
               devices[i].strFramework);
    }
}

free(devices);
```

---

## Append vs. Detect Behavior

### Camera Reconnection Scenarios

**Scenario 1: Same camera disconnects and reconnects**
- Uses same `lDevIndex` when reconnected
- Maintains index consistency

**Scenario 2: Different camera connects after disconnection**
- Gets new `lDevIndex`
- Previous index remains invalid until reused

**Scenario 3: Using XSDK_Detect after disconnection**
- Regenerates entire camera list
- Previous `lDevIndex` values NOT guaranteed

### Detecting Disconnections

Monitor disconnections by checking error codes:
- `XSDK_ERRCODE_COMMUNICATION` - Communication lost
- `XSDK_ERRCODE_TIMEOUT` - Camera not responding

```c
// Check if camera disconnected
result = XSDK_GetSomeProperty(hCamera, &value);
if (result == XSDK_ERROR) {
    long apiCode, errCode;
    XSDK_GetErrorNumber(hCamera, &apiCode, &errCode);

    if (errCode == XSDK_ERRCODE_COMMUNICATION ||
        errCode == XSDK_ERRCODE_TIMEOUT) {
        // Camera disconnected - try to detect again
        XSDK_Append(interface, NULL, NULL, &count, deviceList);
    }
}
```

---

## Best Practices

1. **Initial Detection:** Use `XSDK_Detect()` at startup
2. **Hotplug Detection:** Use `XSDK_Append()` to update list without clearing
3. **Multiple Cameras:** Use `XSDK_WriteDeviceName()` to assign unique names for easier identification
4. **Error Handling:** Always check return values and use `XSDK_GetErrorNumber()` for details
5. **macOS M1/M2:** Wait 3000ms after `XSDK_Init()` before calling `XSDK_Detect()`

---

## Android OS Alternative Implementation

For Android OS, standard `XSDK_Detect()` is not supported. Use Android USB API with `XSDK_SetUSBDeviceHandle()` instead. See Session Management documentation for Android-specific patterns.
