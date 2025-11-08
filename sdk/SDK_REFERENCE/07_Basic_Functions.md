# Basic Functions

## Overview

Basic utility functions provide error reporting, version information, and SDK diagnostics.

**State Requirements:**
- All basic functions can be used in all states (S0, S1, S2, S3)

---

## XSDK_GetErrorNumber

**Purpose:** Gets the detailed result of the last called function.

### Function Signature

```c
XSDK_APIENTRY XSDK_GetErrorNumber(
    XSDK_HANDLE hCamera,
    long* plAPICode,
    long* plERRCode
);
```

### Parameters

| Parameter | Direction | Description |
|-----------|-----------|-------------|
| `hCamera` | IN | Camera handle (required in State S3, can be NULL otherwise) |
| `plAPICode` | OUT | Code of the last called API function |
| `plERRCode` | OUT | Detailed error code (see Error Codes section) |

### Return Values

- `XSDK_COMPLETE`: Success
- `XSDK_ERROR`: Error

### Usage Example

```c
// After an API call fails
result = XSDK_StartShutterRelease(hCamera);

if (result != XSDK_COMPLETE) {
    long apiCode, errCode;
    XSDK_GetErrorNumber(hCamera, &apiCode, &errCode);

    printf("API Code: 0x%08lX\n", apiCode);
    printf("Error Code: 0x%08lX\n", errCode);

    // Check specific error
    switch (errCode) {
        case XSDK_ERRCODE_COMMUNICATION:
            printf("Communication error\n");
            break;
        case XSDK_ERRCODE_TIMEOUT:
            printf("Operation timed out\n");
            break;
        case XSDK_ERRCODE_BUSY:
            printf("Camera is busy\n");
            break;
        case XSDK_ERRCODE_RUNNING_OTHER_FUNCTION:
            printf("Another operation is running\n");
            // Use XSDK_GetErrorDetails for more info
            break;
    }
}
```

---

## XSDK_GetErrorDetails

**Purpose:** Gets details about busy errors when another operation is running.

Use this when `XSDK_GetErrorNumber()` returns `XSDK_ERRCODE_RUNNING_OTHER_FUNCTION`.

### Function Signature

```c
XSDK_APIENTRY XSDK_GetErrorDetails(
    XSDK_HANDLE hCamera,
    long* plAPICode,
    unsigned long* pulERRCode
);
```

### Parameters

| Parameter | Direction | Description |
|-----------|-----------|-------------|
| `hCamera` | IN | Camera handle (required in State S3, can be NULL otherwise) |
| `plAPICode` | OUT | Code of the last called API |
| `pulERRCode` | OUT | Bitmap indicating which function is currently running |

### Error Detail Flags

| Flag | Description |
|------|-------------|
| `XSDK_ERROR_DETAIL_AEL` | AE (Auto Exposure) is locked |
| `XSDK_ERROR_DETAIL_AFL` | AF (Auto Focus) is locked |
| `XSDK_ERROR_DETAIL_INSTANTAF` | INSTANT AF in operation |
| `XSDK_ERROR_DETAIL_AFON` | AF for AF ON in operation |
| `XSDK_ERROR_DETAIL_SHOOTING` | Shooting in progress |
| `XSDK_ERROR_DETAIL_SHOOTINGCOUNTDOWN` | SELF-TIMER in operation |
| `XSDK_ERROR_DETAIL_RECORDING` | Movie recording in progress |
| `XSDK_ERROR_DETAIL_LIVEVIEW` | Live view is active |
| `XSDK_ERROR_DETAIL_UNTRANSFERRED_IMAGE` | Pictures remain in camera volatile memory |

### Return Values

- `XSDK_COMPLETE`: Success
- `XSDK_ERROR`: Error

### Usage Example

```c
// After getting XSDK_ERRCODE_RUNNING_OTHER_FUNCTION
long apiCode, errCode;
XSDK_GetErrorNumber(hCamera, &apiCode, &errCode);

if (errCode == XSDK_ERRCODE_RUNNING_OTHER_FUNCTION) {
    unsigned long detailCode;
    XSDK_GetErrorDetails(hCamera, &apiCode, &detailCode);

    // Check what's running
    if (detailCode & XSDK_ERROR_DETAIL_SHOOTING) {
        printf("Shooting in progress - wait for completion\n");
    }
    if (detailCode & XSDK_ERROR_DETAIL_RECORDING) {
        printf("Movie recording active\n");
    }
    if (detailCode & XSDK_ERROR_DETAIL_LIVEVIEW) {
        printf("Live view is running\n");
    }
    if (detailCode & XSDK_ERROR_DETAIL_UNTRANSFERRED_IMAGE) {
        printf("Images pending download\n");
    }
}
```

### Handling Busy States

```c
// Wait for camera to become ready
bool WaitForCameraReady(XSDK_HANDLE hCamera, int timeoutMs) {
    int elapsed = 0;
    const int pollInterval = 100;  // Check every 100ms

    while (elapsed < timeoutMs) {
        long apiCode, errCode;
        unsigned long detailCode;

        // Try to get camera status
        result = XSDK_GetSomeProperty(hCamera, &value);

        if (result == XSDK_COMPLETE) {
            return true;  // Camera ready
        }

        XSDK_GetErrorNumber(hCamera, &apiCode, &errCode);

        if (errCode == XSDK_ERRCODE_RUNNING_OTHER_FUNCTION) {
            XSDK_GetErrorDetails(hCamera, &apiCode, &detailCode);

            // If shooting, wait longer
            if (detailCode & XSDK_ERROR_DETAIL_SHOOTING) {
                Sleep(pollInterval);
                elapsed += pollInterval;
                continue;
            }
        } else {
            // Some other error - not a busy state
            return false;
        }

        Sleep(pollInterval);
        elapsed += pollInterval;
    }

    return false;  // Timeout
}
```

---

## XSDK_GetVersionString

**Purpose:** Gets SDK version number as a string.

### Function Signature

```c
XSDK_APIENTRY XSDK_GetVersionString(LPSTR pVersionString);
```

### Parameters

| Parameter | Direction | Description |
|-----------|-----------|-------------|
| `pVersionString` | OUT | SDK version string. Allocate 256-byte buffer before calling. |

### Return Values

- `XSDK_COMPLETE`: Success
- `XSDK_ERROR`: Error

### Usage Example

```c
char versionString[256];
result = XSDK_GetVersionString(versionString);

if (result == XSDK_COMPLETE) {
    printf("SDK Version: %s\n", versionString);
    // Example output: "1.33.0.0"
}
```

### Version Check Pattern

```c
bool CheckSDKVersion(const char* minimumVersion) {
    char sdkVersion[256];

    if (XSDK_GetVersionString(sdkVersion) != XSDK_COMPLETE) {
        return false;
    }

    printf("SDK Version: %s\n", sdkVersion);
    printf("Minimum Required: %s\n", minimumVersion);

    // Parse and compare versions (implement version comparison logic)
    // Return true if sdkVersion >= minimumVersion

    return true;
}
```

---

## Error Handling Best Practices

### 1. Always Check Return Values

```c
// BAD - No error checking
XSDK_StartShutterRelease(hCamera);

// GOOD - Check and handle errors
result = XSDK_StartShutterRelease(hCamera);
if (result != XSDK_COMPLETE) {
    HandleError(hCamera, "StartShutterRelease");
}
```

### 2. Create Error Handler Function

```c
void HandleError(XSDK_HANDLE hCamera, const char* functionName) {
    long apiCode, errCode;

    XSDK_GetErrorNumber(hCamera, &apiCode, &errCode);

    fprintf(stderr, "Error in %s\n", functionName);
    fprintf(stderr, "API Code: 0x%08lX\n", apiCode);
    fprintf(stderr, "Error Code: 0x%08lX\n", errCode);

    // Provide user-friendly messages
    switch (errCode) {
        case XSDK_ERRCODE_NOERR:
            fprintf(stderr, "No error\n");
            break;
        case XSDK_ERRCODE_COMMUNICATION:
            fprintf(stderr, "Communication error - check connection\n");
            break;
        case XSDK_ERRCODE_TIMEOUT:
            fprintf(stderr, "Timeout - operation took too long\n");
            break;
        case XSDK_ERRCODE_BUSY:
            fprintf(stderr, "Camera busy\n");
            break;
        case XSDK_ERRCODE_RUNNING_OTHER_FUNCTION:
            HandleBusyState(hCamera);
            break;
        default:
            fprintf(stderr, "Unknown error\n");
    }
}

void HandleBusyState(XSDK_HANDLE hCamera) {
    unsigned long detailCode;
    long apiCode;

    XSDK_GetErrorDetails(hCamera, &apiCode, &detailCode);

    fprintf(stderr, "Camera busy: ");

    if (detailCode & XSDK_ERROR_DETAIL_SHOOTING) {
        fprintf(stderr, "Shooting ");
    }
    if (detailCode & XSDK_ERROR_DETAIL_RECORDING) {
        fprintf(stderr, "Recording ");
    }
    if (detailCode & XSDK_ERROR_DETAIL_LIVEVIEW) {
        fprintf(stderr, "LiveView ");
    }
    if (detailCode & XSDK_ERROR_DETAIL_UNTRANSFERRED_IMAGE) {
        fprintf(stderr, "PendingImages ");
    }

    fprintf(stderr, "\n");
}
```

### 3. Monitor Connection Status

```c
bool IsConnected(XSDK_HANDLE hCamera) {
    // Try a lightweight operation
    char version[256];
    result = XSDK_GetFirmwareVersion(hCamera, version);

    if (result == XSDK_COMPLETE) {
        return true;
    }

    long apiCode, errCode;
    XSDK_GetErrorNumber(hCamera, &apiCode, &errCode);

    if (errCode == XSDK_ERRCODE_COMMUNICATION ||
        errCode == XSDK_ERRCODE_TIMEOUT) {
        return false;  // Disconnected
    }

    return true;  // Still connected, just busy or other error
}
```

---

## Common Error Codes

| Error Code | Value | Description |
|------------|-------|-------------|
| `XSDK_ERRCODE_NOERR` | 0x00000000 | No error |
| `XSDK_ERRCODE_COMMUNICATION` | - | Communication error (disconnected) |
| `XSDK_ERRCODE_TIMEOUT` | - | Operation timed out |
| `XSDK_ERRCODE_BUSY` | - | Camera is busy |
| `XSDK_ERRCODE_RUNNING_OTHER_FUNCTION` | - | Another operation in progress |
| `XSDK_ERRCODE_SEQUENCE` | 0x00001001 | Wrong function call sequence |
| `XSDK_ERRCODE_PERMISSION` | 0x00001002 | Permission denied (Android) |
| `XSDK_ERRCODE_UNKNOWN` | 0x00009100 | Unknown error (Android) |

**Note:** Refer to Error Codes section (Chapter 5) in the SDK manual for complete error code list.

---

## Diagnostic Logging Pattern

```c
typedef struct {
    FILE* logFile;
    bool enableLogging;
} Logger;

Logger logger = {NULL, false};

void InitLogger(const char* filename) {
    logger.logFile = fopen(filename, "w");
    logger.enableLogging = (logger.logFile != NULL);

    if (logger.enableLogging) {
        char version[256];
        XSDK_GetVersionString(version);
        fprintf(logger.logFile, "SDK Version: %s\n", version);
        fprintf(logger.logFile, "===================\n");
        fflush(logger.logFile);
    }
}

void LogSDKCall(const char* funcName, long result, XSDK_HANDLE hCamera) {
    if (!logger.enableLogging) return;

    time_t now = time(NULL);
    fprintf(logger.logFile, "[%s] %s: ", ctime(&now), funcName);

    if (result == XSDK_COMPLETE) {
        fprintf(logger.logFile, "SUCCESS\n");
    } else {
        long apiCode, errCode;
        XSDK_GetErrorNumber(hCamera, &apiCode, &errCode);
        fprintf(logger.logFile, "FAILED (API=0x%08lX, ERR=0x%08lX)\n",
                apiCode, errCode);
    }

    fflush(logger.logFile);
}

void CloseLogger() {
    if (logger.logFile) {
        fclose(logger.logFile);
        logger.logFile = NULL;
    }
    logger.enableLogging = false;
}
```

Usage:
```c
InitLogger("sdk_debug.log");

result = XSDK_Init(NULL);
LogSDKCall("XSDK_Init", result, NULL);

result = XSDK_StartShutterRelease(hCamera);
LogSDKCall("XSDK_StartShutterRelease", result, hCamera);

CloseLogger();
```
