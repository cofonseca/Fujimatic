#include "fm_wrapper.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#ifdef _WIN32
    #include <windows.h>
    #define PATH_SEP '\\'
#else
    #include <dlfcn.h>
    #define PATH_SEP '/'
#endif

// Include Fujifilm SDK headers
#include "../sdk/HEADERS/XAPI.H"
#include "../sdk/HEADERS/XAPIOpt.H"
#include "../sdk/HEADERS/X-T3.h"

// Global state
static LIB_HANDLE g_hLib = NULL;
static XSDK_HANDLE g_hCamera = NULL;
static int verbose_logging = 0;
static int g_liveview_active = 0; // Track live view state (X-T3 doesn't have GetLiveViewStatus)

// Battery level conversion helper
static int convert_battery_to_percent(long battery_code) {
    switch (battery_code) {
        case SDK_POWERCAPACITY_EMPTY:   return 0;
        case SDK_POWERCAPACITY_END:     return 5;
        case SDK_POWERCAPACITY_PREEND:  return 10;
        case SDK_POWERCAPACITY_PREEND5: return 15;
        case SDK_POWERCAPACITY_20:      return 20;
        case SDK_POWERCAPACITY_40:      return 40;
        case SDK_POWERCAPACITY_HALF:    return 50;
        case SDK_POWERCAPACITY_60:      return 60;
        case SDK_POWERCAPACITY_80:      return 80;
        case SDK_POWERCAPACITY_FULL:    return 95;
        case SDK_POWERCAPACITY_HIGH:    return 100;
        case SDK_POWERCAPACITY_100:     return 100;
        case SDK_POWERCAPACITY_DC:      return 100; // AC powered
        case SDK_POWERCAPACITY_DC_CHARGE: return 100; // Charging
        default:                        return 50; // Unknown
    }
}

int fm_init(const char* sdk_path) {
    // NOTE: sdk_path is ignored - we load XAPI.dll from current directory
    // The SDK expects all DLLs (XAPI.dll, FF*.dll, etc.) to be in the same directory
    // This matches how the official Fujifilm samples work

    (void)sdk_path; // Suppress unused parameter warning

#ifdef _WIN32
    // Load XAPI.dll from current directory (where all SDK DLLs should be)
    if (verbose_logging) {
        printf("Loading XAPI.dll from current directory...\n");
    }
    g_hLib = LoadLibraryA("XAPI.dll");
    if (g_hLib == NULL) {
        DWORD error = GetLastError();
        fprintf(stderr, "fm_init: Failed to load XAPI.dll from current directory (Error: %lu)\n", error);
        fprintf(stderr, "  Make sure XAPI.dll and all FF*.dll files are in the same directory as the executable\n");
        return -2;
    }
    if (verbose_logging) {
        printf("XAPI.dll loaded successfully\n");
    }
#else
    // On Linux, load libFXAPI.so from current directory
    if (verbose_logging) {
        printf("Loading libFXAPI.so from current directory...\n");
    }
    g_hLib = dlopen("./libFXAPI.so", RTLD_NOW);
    if (g_hLib == NULL) {
        fprintf(stderr, "fm_init: Failed to load libFXAPI.so: %s\n", dlerror());
        fprintf(stderr, "  Make sure libFXAPI.so is in the current directory\n");
        return -2;
    }
    if (verbose_logging) {
        printf("libFXAPI.so loaded successfully\n");
    }
#endif

    // Initialize the SDK
    if (verbose_logging) {
        printf("Calling XSDK_Init...\n");
    }
    long result = XSDK_Init(g_hLib);
    if (result != 0) {
        fprintf(stderr, "fm_init: XSDK_Init failed with code: %ld\n", result);
#ifdef _WIN32
        FreeLibrary(g_hLib);
#else
        dlclose(g_hLib);
#endif
        g_hLib = NULL;
        return -3;
    }

    if (verbose_logging) {
        printf("Fujifilm SDK initialized successfully\n");
    }
    return 0;
}

int fm_connect() {
    if (g_hLib == NULL) {
        fprintf(stderr, "fm_connect: SDK not initialized\n");
        return -1;
    }

    if (g_hCamera != NULL) {
        fprintf(stderr, "fm_connect: Camera already connected\n");
        return -2;
    }

    // Use the same approach as ReleaseButton sample:
    // 1. Call XSDK_Detect with USB interface
    // 2. Open with "ENUM:0" device string

    #define XSDK_DSC_IF_USB 0x00000001

    if (verbose_logging) {
        printf("Detecting cameras on USB interface...\n");
    }
    long num_cameras = 0;
    long result = XSDK_Detect(XSDK_DSC_IF_USB, "", NULL, &num_cameras);

    if (verbose_logging) {
        printf("XSDK_Detect result: %ld, count: %ld\n", result, num_cameras);
    }

    if (result != 0) {
        fprintf(stderr, "fm_connect: XSDK_Detect failed with code: %ld\n", result);
        return -3;
    }

    if (num_cameras == 0) {
        fprintf(stderr, "fm_connect: No cameras detected\n");
        fprintf(stderr, "  Possible issues:\n");
        fprintf(stderr, "  - Camera not powered on\n");
        fprintf(stderr, "  - USB cable not connected\n");
        fprintf(stderr, "  - Camera in wrong USB connection mode (try USB TETHER AUTO)\n");
        return -4;
    }

    if (verbose_logging) {
        printf("Found %ld camera(s) - opening first camera with ENUM:0\n", num_cameras);
    }

    // Open first enumerated camera using ENUM:0 device string (matches ReleaseButton sample)
    long capability_bitmap = 0;
    result = XSDK_OpenEx("ENUM:0", &g_hCamera, &capability_bitmap, NULL);

    if (result != 0 || g_hCamera == NULL) {
        fprintf(stderr, "fm_connect: XSDK_OpenEx failed with code: %ld\n", result);
        return -5;
    }

    // CRITICAL: Set camera to PC priority mode (matches ReleaseButton sample)
    // Without this, most SDK operations will fail or crash!
    #define XSDK_PRIORITY_PC 0x0002
    #define XSDK_MEDIAREC_RAWJPEG 0x0001

    if (verbose_logging) {
        printf("Setting camera to PC priority mode...\n");
    }
    result = XSDK_SetPriorityMode(g_hCamera, XSDK_PRIORITY_PC);
    if (result != 0) {
        fprintf(stderr, "fm_connect: XSDK_SetPriorityMode failed with code: %ld\n", result);
        XSDK_Close(g_hCamera);
        g_hCamera = NULL;
        return -6;
    }

    // CRITICAL: Give camera time to fully transition to PC mode
    // Some cameras need 100-200ms to be ready for subsequent settings
    if (verbose_logging) {
        printf("Waiting for camera to settle into PC control mode...\n");
    }
#ifdef _WIN32
    Sleep(150);  // 150ms delay
#else
    usleep(150000);
#endif

    if (verbose_logging) {
        printf("Setting media recording to RAW+JPEG...\n");
    }
    result = XSDK_SetMediaRecord(g_hCamera, XSDK_MEDIAREC_RAWJPEG);
    if (result != 0) {
        fprintf(stderr, "fm_connect: XSDK_SetMediaRecord failed with code: %ld\n", result);
        fprintf(stderr, "  Continuing anyway - RAW capture will still work\n");
        // Don't fail the connection - RAW capture is the priority
        // Most important: we're in PC mode and can capture
    }

    // Get device info for display
    XSDK_DeviceInformation dev_info;
    memset(&dev_info, 0, sizeof(dev_info));
    if (XSDK_GetDeviceInfo(g_hCamera, &dev_info) == 0) {
        printf("Connected to camera: %s (S/N: %s, F/W: %s)\n",
               dev_info.strProduct,
               dev_info.strSerialNo,
               dev_info.strFirmware);
    } else {
        printf("Connected to camera successfully\n");
    }

    printf("Camera ready for PC control\n");
    return 0;
}

int fm_disconnect() {
    if (g_hCamera == NULL) {
        fprintf(stderr, "fm_disconnect: Camera not connected\n");
        return -1;
    }

    // CRITICAL: Restore camera to CAMERA priority mode before closing
    // Without this, the camera stays in PC mode and reconnection fails
    #define XSDK_PRIORITY_CAMERA 0x0001

    if (verbose_logging) {
        printf("Restoring camera to CAMERA priority mode...\n");
    }
    long result = XSDK_SetPriorityMode(g_hCamera, XSDK_PRIORITY_CAMERA);
    if (result != 0) {
        fprintf(stderr, "fm_disconnect: Warning - failed to restore camera priority mode (code: %ld)\n", result);
        // Continue with close anyway
    }

    result = XSDK_Close(g_hCamera);
    if (result != 0) {
        fprintf(stderr, "fm_disconnect: XSDK_Close failed with code: %ld\n", result);
        g_hCamera = NULL;  // Clear handle even if close failed
        return -2;
    }

    g_hCamera = NULL;
    g_liveview_active = 0; // Reset live view state
    printf("Camera disconnected successfully\n");
    return 0;
}

int fm_get_battery(int* percent) {
    if (g_hCamera == NULL) {
        fprintf(stderr, "fm_get_battery: Camera not connected\n");
        return -1;
    }

    if (percent == NULL) {
        fprintf(stderr, "fm_get_battery: percent pointer is NULL\n");
        return -2;
    }

    // Get battery info using XSDK_GetProp
    // X-T3 CheckBatteryInfo API requires 6 parameters (all output pointers)
    // Based on SDK pattern: variadic parameters are output pointers for Get* functions
    long param1 = 0, param2 = 0, param3 = 0, param4 = 0, param5 = 0, param6 = 0;

    long result = XSDK_GetProp(g_hCamera, API_CODE_CheckBatteryInfo, 6,
                               &param1, &param2, &param3, &param4, &param5, &param6);

    if (result != 0) {
        fprintf(stderr, "fm_get_battery: XSDK_GetProp failed with code: %ld\n", result);
        return -3;
    }

    // First parameter should be battery code
    // Log all parameters for debugging
    if (verbose_logging) {
        printf("Battery info - param1=%ld, param2=%ld, param3=%ld, param4=%ld, param5=%ld, param6=%ld\n",
               param1, param2, param3, param4, param5, param6);
    }

    *percent = convert_battery_to_percent(param1);
    return 0;
}

int fm_get_shutter(int* microseconds) {
    if (g_hCamera == NULL) {
        fprintf(stderr, "fm_get_shutter: Camera not connected\n");
        return -1;
    }

    if (microseconds == NULL) {
        fprintf(stderr, "fm_get_shutter: microseconds pointer is NULL\n");
        return -2;
    }

    if (verbose_logging) {
        printf("fm_get_shutter: Starting with camera handle %p\n", (void*)g_hCamera);
    }

    // Get shutter speed from camera (SDK returns microseconds)
    long speed_microseconds = 0;
    long bulb = 0;
    if (verbose_logging) {
        printf("fm_get_shutter: Calling XSDK_GetShutterSpeed...\n");
    }
    long result = XSDK_GetShutterSpeed(g_hCamera, &speed_microseconds, &bulb);
    if (verbose_logging) {
        printf("fm_get_shutter: XSDK_GetShutterSpeed returned %ld\n", result);
        printf("fm_get_shutter: speed_microseconds = %ld, bulb = %ld\n", speed_microseconds, bulb);
    }

    if (result != 0) {
        fprintf(stderr, "fm_get_shutter: XSDK_GetShutterSpeed failed with code: %ld\n", result);
        fprintf(stderr, "  Camera handle: %p\n", (void*)g_hCamera);
        return -3;
    }

    // Return microseconds directly (SDK units)
    *microseconds = (int)speed_microseconds;
    if (verbose_logging) {
        printf("fm_get_shutter: Setting *microseconds = %d (from %ld)\n", *microseconds, speed_microseconds);
        printf("Current shutter speed: %d microseconds (%.6f seconds, bulb=%ld)\n",
               *microseconds, (double)speed_microseconds / 1000000.0, bulb);
    }
    return 0;
}

int fm_set_shutter(int microseconds) {
    if (g_hCamera == NULL) {
        fprintf(stderr, "fm_set_shutter: Camera not connected\n");
        return -1;
    }

    if (microseconds < 125) {
        fprintf(stderr, "fm_set_shutter: Invalid shutter speed (too fast, minimum: 125 microseconds = 1/8000s)\n");
        return -2;
    }

    if (microseconds > 3600000000) { // 1 hour maximum
        fprintf(stderr, "fm_set_shutter: Invalid shutter speed (too slow, maximum: 3600000000 microseconds = 1 hour)\n");
        return -2;
    }

    // Check if camera supports shutter control
    long num_speeds = 0;
    long result = XSDK_CapShutterSpeed(g_hCamera, &num_speeds, NULL, NULL);
    if (result != 0) {
        fprintf(stderr, "fm_set_shutter: XSDK_CapShutterSpeed failed with code: %ld\n", result);
    } else if (num_speeds == 0) {
        fprintf(stderr, "fm_set_shutter: Camera reports 0 supported shutter speeds\n");
        fprintf(stderr, "  This usually means the camera is in Program AE mode\n");
        fprintf(stderr, "  Try setting Manual mode first with 'exposure manual' command\n");
        return -4;
    }

    // Set the shutter speed
    long bulb = 0; // Not using bulb mode for MVP
    result = XSDK_SetShutterSpeed(g_hCamera, microseconds, bulb);
    if (result != 0) {
        fprintf(stderr, "fm_set_shutter: XSDK_SetShutterSpeed failed with code: %ld\n", result);
        fprintf(stderr, "  Camera handle: %p\n", (void*)g_hCamera);
        fprintf(stderr, "  Shutter speed: %d microseconds\n", microseconds);
        fprintf(stderr, "  Bulb mode: %ld\n", bulb);
        return -3;
    }

    printf("Shutter speed set to %d microseconds (%.6f seconds)\n", microseconds, (double)microseconds / 1000000.0);
    return 0;
}

// Diagnostic function to get all available shutter speeds
int fm_list_shutter_speeds() {
    if (g_hCamera == NULL) {
        fprintf(stderr, "fm_list_shutter_speeds: Camera not connected\n");
        return -1;
    }

    printf("Getting available shutter speeds...\n");
    long num_speeds = 0;
    long result = XSDK_CapShutterSpeed(g_hCamera, &num_speeds, NULL, NULL);
    if (result != 0) {
        fprintf(stderr, "fm_list_shutter_speeds: XSDK_CapShutterSpeed failed with code: %ld\n", result);
        return -2;
    }

    printf("Camera supports %ld shutter speeds\n", num_speeds);

    if (num_speeds > 0) {
        long* valid_speeds = (long*)malloc(num_speeds * sizeof(long));
        long* bulb_capable = (long*)malloc(num_speeds * sizeof(long));
        if (valid_speeds != NULL && bulb_capable != NULL) {
            result = XSDK_CapShutterSpeed(g_hCamera, &num_speeds, valid_speeds, bulb_capable);
            if (result == 0) {
                printf("Valid shutter speeds (microseconds):\n");
                for (long i = 0; i < num_speeds; i++) {
                    printf("  %2ld: %7ld μs (%.6f seconds) %s\n", 
                           i + 1, valid_speeds[i], (double)valid_speeds[i] / 1000000.0,
                           bulb_capable[i] ? "[bulb capable]" : "");
                }
            } else {
                fprintf(stderr, "Failed to get shutter speed list: %ld\n", result);
            }
            free(valid_speeds);
            free(bulb_capable);
        } else {
            fprintf(stderr, "Memory allocation failed for shutter speed list\n");
        }
    }

    return 0;
}

int fm_set_exposure_mode(int mode) {
    if (g_hCamera == NULL) {
        fprintf(stderr, "fm_set_exposure_mode: Camera not connected\n");
        return -1;
    }

    // Tethered control requires Manual exposure mode for full control
    if (verbose_logging) {
        printf("Setting Manual exposure mode for tethered control...\n");
    }
    long result = XSDK_SetAEMode(g_hCamera, 0x0001); // Always Manual mode
    if (result != 0) {
        fprintf(stderr, "fm_set_exposure_mode: Failed to set Manual exposure mode: %ld\n", result);
        return -3;
    }

    if (verbose_logging) {
        printf("Manual exposure mode set successfully\n");
    }
    return 0;
}

int fm_get_exposure_mode(int* mode) {
    if (g_hCamera == NULL) {
        fprintf(stderr, "fm_get_exposure_mode: Camera not connected\n");
        return -1;
    }

    if (mode == NULL) {
        fprintf(stderr, "fm_get_exposure_mode: mode pointer is NULL\n");
        return -2;
    }

    long currentMode = 0;
    long result = XSDK_GetAEMode(g_hCamera, &currentMode);
    if (result != 0) {
        fprintf(stderr, "fm_get_exposure_mode: XSDK_GetAEMode failed with code: %ld\n", result);
        return -3;
    }

    *mode = (int)currentMode;
    if (verbose_logging) {
        printf("Current exposure mode: 0x%04X\n", *mode);
    }
    return 0;
}

int fm_get_iso(int* iso) {
    if (g_hCamera == NULL) {
        fprintf(stderr, "fm_get_iso: Camera not connected\n");
        return -1;
    }

    if (iso == NULL) {
        fprintf(stderr, "fm_get_iso: iso pointer is NULL\n");
        return -2;
    }

    // Get ISO sensitivity from camera
    long iso_value = 0;
    long result = XSDK_GetSensitivity(g_hCamera, &iso_value);
    if (result != 0) {
        fprintf(stderr, "fm_get_iso: XSDK_GetSensitivity failed with code: %ld\n", result);
        return -3;
    }

    *iso = (int)iso_value;
    if (verbose_logging) {
        printf("Current ISO: %d\n", *iso);
    }
    return 0;
}

int fm_set_iso(int iso) {
    if (g_hCamera == NULL) {
        fprintf(stderr, "fm_set_iso: Camera not connected\n");
        return -1;
    }

    if (iso < 100 || iso > 51200) {
        fprintf(stderr, "fm_set_iso: Invalid ISO value (must be 100-51200)\n");
        return -2;
    }

    long result = XSDK_SetSensitivity(g_hCamera, (long)iso);
    if (result != 0) {
        fprintf(stderr, "fm_set_iso: XSDK_SetSensitivity failed with code: %ld\n", result);
        return -3;
    }

    printf("ISO set to %d\n", iso);
    return 0;
}

int fm_capture() {
    if (g_hCamera == NULL) {
        fprintf(stderr, "fm_capture: Camera not connected\n");
        return -1;
    }

    // Trigger capture using XSDK_Release
    // Use XSDK_RELEASE_SHOOT_S1OFF mode - complete shutter sequence in one call
    // From SDK manual: "Shutter button pressed all the way down and then released"
    #define XSDK_RELEASE_SHOOT 0x0100
    #define XSDK_RELEASE_N_S1OFF 0x0004
    #define XSDK_RELEASE_SHOOT_S1OFF (XSDK_RELEASE_SHOOT | XSDK_RELEASE_N_S1OFF)
    #define XSDK_COMPLETE 0

    long shot_opt = 0;
    long af_status = 0;
    long result = 0;

    // Retry logic for camera initialization issues
    // Some cameras need extra time after connection before first capture works
    int max_retries = 3;
    int retry_delay_ms = 200;

    for (int attempt = 0; attempt < max_retries; attempt++) {
        if (verbose_logging && attempt > 0) {
            printf("Retry attempt %d/%d...\n", attempt + 1, max_retries);
        }

        if (verbose_logging) {
            printf("Triggering shutter release (mode: 0x%04X)...\n", XSDK_RELEASE_SHOOT_S1OFF);
        }
        result = XSDK_Release(g_hCamera, XSDK_RELEASE_SHOOT_S1OFF, &shot_opt, &af_status);

        if (result == XSDK_COMPLETE) {
            // Success!
            break;
        }

        // Get detailed error information
        long api_code = 0;
        long err_code = 0;
        XSDK_GetErrorNumber(g_hCamera, &api_code, &err_code);

        // Check if this is a retryable error
        // 0x1008 = Camera not fully initialized (common on first capture)
        // 0x8002 = Camera busy
        if (err_code == 0x1008 || err_code == 0x8002) {
            if (attempt < max_retries - 1) {
                if (verbose_logging) {
                    fprintf(stderr, "fm_capture: Temporary error (0x%04lX), retrying after %dms...\n",
                            err_code, retry_delay_ms);
                }
#ifdef _WIN32
                Sleep(retry_delay_ms);
#else
                usleep(retry_delay_ms * 1000);
#endif
                continue;  // Retry
            }
        }

        // Non-retryable error or max retries reached
        fprintf(stderr, "fm_capture: XSDK_Release failed\n");
        fprintf(stderr, "  Return code: %ld\n", result);
        fprintf(stderr, "  API code: %ld (0x%04lX)\n", api_code, api_code);
        fprintf(stderr, "  Error code: %ld (0x%04lX)\n", err_code, err_code);

        // Common error codes
        if (err_code == 0x1008) {
            fprintf(stderr, "  -> Camera not ready (initialization issue)\n");
        } else if (err_code == 0x8002) {
            fprintf(stderr, "  -> Camera is BUSY (try again)\n");
        } else if (err_code == 0x8003) {
            fprintf(stderr, "  -> Invalid parameter\n");
        } else if (err_code == 0x8004) {
            fprintf(stderr, "  -> Not supported in this mode\n");
        } else if (err_code == 0x8006) {
            fprintf(stderr, "  -> Camera in wrong priority mode\n");
        }

        return -2;
    }

    if (result != XSDK_COMPLETE) {
        // Should not reach here, but just in case
        return -2;
    }

    if (verbose_logging) {
        printf("Capture triggered successfully (AF status: %ld, shot_opt: %ld)\n", af_status, shot_opt);
    }

    // Wait for image to appear in buffer
    // SDK manual: "poll the camera buffer by XSDK_GetBufferCapacity() or XSDK_ReadImageInfo()"
    long shoot_frames = 0;
    long total_frames = 0;
    int attempts = 0;

    if (verbose_logging) {
        printf("Waiting for image to be written to camera buffer...\n");
    }
    while (attempts < 100) {  // Max 10 seconds
        XSDK_GetBufferCapacity(g_hCamera, &shoot_frames, &total_frames);
        if (shoot_frames > 0) {
            if (verbose_logging) {
                printf("Image available in buffer after %d ms (%ld frames)\n", attempts * 100, shoot_frames);
            }
            break;  // Image is ready
        }
#ifdef _WIN32
        Sleep(100);  // 100ms
#else
        usleep(100000);
#endif
        attempts++;
    }

    if (attempts >= 100) {
        fprintf(stderr, "fm_capture: Warning - timeout waiting for image in buffer\n");
        return -3;
    }

    return 0;
}

int fm_download_last(const char* outdir, const char* filename) {
    if (g_hCamera == NULL) {
        fprintf(stderr, "fm_download_last: Camera not connected\n");
        return -1;
    }

    if (outdir == NULL || filename == NULL) {
        fprintf(stderr, "fm_download_last: NULL parameters\n");
        return -2;
    }

    // Check buffer capacity to see if image is available
    long shoot_frames = 0;
    long total_frames = 0;
    long result = XSDK_GetBufferCapacity(g_hCamera, &shoot_frames, &total_frames);
    if (result != 0) {
        fprintf(stderr, "fm_download_last: XSDK_GetBufferCapacity failed with code: %ld\n", result);
        return -3;
    }

    if (shoot_frames == 0) {
        fprintf(stderr, "fm_download_last: No images in buffer\n");
        return -4;
    }

    // Read image information
    XSDK_ImageInformation img_info;
    memset(&img_info, 0, sizeof(img_info));
    result = XSDK_ReadImageInfo(g_hCamera, &img_info);
    if (result != 0) {
        fprintf(stderr, "fm_download_last: XSDK_ReadImageInfo failed with code: %ld\n", result);
        return -5;
    }

    // Allocate buffer for image data
    unsigned char* image_data = (unsigned char*)malloc(img_info.lDataSize);
    if (image_data == NULL) {
        fprintf(stderr, "fm_download_last: Failed to allocate %ld bytes for image\n", img_info.lDataSize);
        return -6;
    }

    // Read image data
    result = XSDK_ReadImage(g_hCamera, image_data, img_info.lDataSize);
    if (result != 0) {
        fprintf(stderr, "fm_download_last: XSDK_ReadImage failed with code: %ld\n", result);
        free(image_data);
        return -7;
    }

    // Build full file path
    char filepath[1024];
    snprintf(filepath, sizeof(filepath), "%s%c%s", outdir, PATH_SEP, filename);

    // Write to file
    FILE* fp = fopen(filepath, "wb");
    if (fp == NULL) {
        fprintf(stderr, "fm_download_last: Failed to open file %s for writing\n", filepath);
        free(image_data);
        return -8;
    }

    size_t written = fwrite(image_data, 1, img_info.lDataSize, fp);
    fclose(fp);
    free(image_data);

    if (written != (size_t)img_info.lDataSize) {
        fprintf(stderr, "fm_download_last: Incomplete write (%zu/%ld bytes)\n", written, img_info.lDataSize);
        return -9;
    }

    // Delete image from camera buffer
    XSDK_DeleteImage(g_hCamera);

    printf("Downloaded image: %s (%ld bytes)\n", filepath, img_info.lDataSize);
    return 0;
}

int fm_get_supported_shutter_count(int* count) {
    if (g_hCamera == NULL) {
        fprintf(stderr, "fm_get_supported_shutter_count: Camera not connected\n");
        return -1;
    }

    if (count == NULL) {
        fprintf(stderr, "fm_get_supported_shutter_count: count pointer is NULL\n");
        return -2;
    }

    if (verbose_logging) {
        printf("Getting supported shutter speed count...\n");
    }
    long num_speeds = 0;
    long bulb_capable = 0;
    long result = XSDK_CapShutterSpeed(g_hCamera, &num_speeds, NULL, &bulb_capable);
    if (result != 0) {
        fprintf(stderr, "fm_get_supported_shutter_count: XSDK_CapShutterSpeed failed with code: %ld\n", result);
        return -3;
    }

    *count = (int)num_speeds;
    if (verbose_logging) {
        printf("Camera supports %ld shutter speeds (bulb capable: %ld)\n", num_speeds, bulb_capable);
    }

    if (num_speeds == 0) {
        fprintf(stderr, "WARNING: No supported shutter speeds found\n");
        fprintf(stderr, "  This usually means the camera is not in Manual exposure mode\n");
        fprintf(stderr, "  Try setting Manual mode first: XSDK_SetAEMode(hCamera, 0x0001)\n");
    }

    return 0;
}

int fm_get_supported_shutter_speeds(int* count, int* speeds) {
    if (g_hCamera == NULL) {
        fprintf(stderr, "fm_get_supported_shutter_speeds: Camera not connected\n");
        return -1;
    }

    if (count == NULL || speeds == NULL) {
        fprintf(stderr, "fm_get_supported_shutter_speeds: NULL parameters\n");
        return -2;
    }

    long num_speeds = 0;
    long bulb_capable = 0;

    // First get the count
    long result = XSDK_CapShutterSpeed(g_hCamera, &num_speeds, NULL, &bulb_capable);
    if (result != 0) {
        fprintf(stderr, "fm_get_supported_shutter_speeds: XSDK_CapShutterSpeed failed with code: %ld\n", result);
        return -3;
    }

    if (num_speeds <= 0) {
        fprintf(stderr, "fm_get_supported_shutter_speeds: No supported shutter speeds found\n");
        *count = 0;
        return -4;
    }

    // Allocate array for the speeds
    long* valid_speeds = (long*)malloc(num_speeds * sizeof(long));
    if (valid_speeds == NULL) {
        fprintf(stderr, "fm_get_supported_shutter_speeds: Memory allocation failed\n");
        return -5;
    }

    // Get the actual speeds
    result = XSDK_CapShutterSpeed(g_hCamera, &num_speeds, valid_speeds, &bulb_capable);
    if (result != 0) {
        fprintf(stderr, "fm_get_supported_shutter_speeds: Failed to get shutter speed list: %ld\n", result);
        free(valid_speeds);
        return -6;
    }

    // Copy to output array
    *count = (int)num_speeds;
    for (long i = 0; i < num_speeds; i++) {
        speeds[i] = (int)valid_speeds[i];
    }

    if (verbose_logging) {
        printf("Supported shutter speeds (%ld values):\n", num_speeds);
        for (long i = 0; i < num_speeds; i++) {
            double seconds = (double)valid_speeds[i] / 1000000.0;
            printf("  %2ld: %7d μs (%.6f seconds)\n", i + 1, valid_speeds[i], seconds);
        }
    }

    free(valid_speeds);
    return 0;
}

int fm_get_focus_mode(int* mode) {
    if (g_hCamera == NULL) {
        fprintf(stderr, "fm_get_focus_mode: Camera not connected\n");
        return -1;
    }

    if (mode == NULL) {
        fprintf(stderr, "fm_get_focus_mode: mode pointer is NULL\n");
        return -2;
    }

    // Get focus mode using XSDK_GetProp
    // API: XSDK_GetProp(hCamera, API_CODE_GetFocusMode, API_PARAM_GetFocusMode, &lFocusMode)
    long focus_mode = 0;
    long result = XSDK_GetProp(g_hCamera, API_CODE_GetFocusMode, 1, &focus_mode);

    if (result != 0) {
        fprintf(stderr, "fm_get_focus_mode: XSDK_GetProp failed with code: %ld\n", result);
        return -3;
    }

    *mode = (int)focus_mode;
    if (verbose_logging) {
        const char* mode_name = "UNKNOWN";
        if (focus_mode == SDK_FOCUS_MANUAL) mode_name = "MANUAL";
        else if (focus_mode == SDK_FOCUS_AFS) mode_name = "AF-S";
        else if (focus_mode == SDK_FOCUS_AFC) mode_name = "AF-C";
        printf("Current focus mode: %s (0x%04X)\n", mode_name, *mode);
    }
    return 0;
}

int fm_set_focus_mode(int mode) {
    if (g_hCamera == NULL) {
        fprintf(stderr, "fm_set_focus_mode: Camera not connected\n");
        return -1;
    }

    // Validate mode
    if (mode != SDK_FOCUS_MANUAL && mode != SDK_FOCUS_AFS && mode != SDK_FOCUS_AFC) {
        fprintf(stderr, "fm_set_focus_mode: Invalid focus mode (must be 0x0001=MANUAL, 0x8001=AFS, or 0x8002=AFC)\n");
        return -2;
    }

    if (verbose_logging) {
        const char* mode_name = "UNKNOWN";
        if (mode == SDK_FOCUS_MANUAL) mode_name = "MANUAL";
        else if (mode == SDK_FOCUS_AFS) mode_name = "AF-S";
        else if (mode == SDK_FOCUS_AFC) mode_name = "AF-C";
        printf("Setting focus mode to %s (0x%04X)...\n", mode_name, mode);
    }

    // Set focus mode using XSDK_SetProp
    // API: XSDK_SetProp(hCamera, API_CODE_SetFocusMode, API_PARAM_SetFocusMode, lFocusMode)
    long result = XSDK_SetProp(g_hCamera, API_CODE_SetFocusMode, 1, (long)mode);

    if (result != 0) {
        fprintf(stderr, "fm_set_focus_mode: XSDK_SetProp failed with code: %ld\n", result);
        fprintf(stderr, "  Camera handle: %p\n", (void*)g_hCamera);
        fprintf(stderr, "  Focus mode: 0x%04X\n", mode);

        // Check if lens supports autofocus
        if (mode != SDK_FOCUS_MANUAL && result != 0) {
            fprintf(stderr, "  -> This may indicate the lens does not support autofocus\n");
        }
        return -3;
    }

    if (verbose_logging) {
        printf("Focus mode set successfully\n");
    }
    return 0;
}

int fm_get_supported_focus_modes(int* count, int* modes) {
    if (g_hCamera == NULL) {
        fprintf(stderr, "fm_get_supported_focus_modes: Camera not connected\n");
        return -1;
    }

    if (count == NULL) {
        fprintf(stderr, "fm_get_supported_focus_modes: count pointer is NULL\n");
        return -2;
    }

    // Query supported focus modes using XSDK_CapProp
    // API: XSDK_CapProp(hCamera, API_CODE_CapFocusMode, API_PARAM_CapFocusMode, &lNum, plFocusMode)
    long num_modes = 0;
    long supported_modes[10]; // Max possible modes
    memset(supported_modes, 0, sizeof(supported_modes));

    long result = XSDK_CapProp(g_hCamera, API_CODE_CapFocusMode, 2, &num_modes, supported_modes);

    if (result != 0) {
        fprintf(stderr, "fm_get_supported_focus_modes: XSDK_CapProp failed with code: %ld\n", result);
        return -3;
    }

    *count = (int)num_modes;

    if (verbose_logging) {
        printf("Camera supports %ld focus modes:\n", num_modes);
        for (long i = 0; i < num_modes; i++) {
            const char* mode_name = "UNKNOWN";
            if (supported_modes[i] == SDK_FOCUS_MANUAL) mode_name = "MANUAL";
            else if (supported_modes[i] == SDK_FOCUS_AFS) mode_name = "AF-S";
            else if (supported_modes[i] == SDK_FOCUS_AFC) mode_name = "AF-C";
            printf("  %ld: %s (0x%04lX)\n", i + 1, mode_name, supported_modes[i]);
        }
    }

    // Copy to output array if provided
    if (modes != NULL && num_modes > 0) {
        for (long i = 0; i < num_modes; i++) {
            modes[i] = (int)supported_modes[i];
        }
    }

    return 0;
}

// ========== Live View Functions ==========

int fm_start_liveview() {
    if (g_hCamera == NULL) {
        fprintf(stderr, "fm_start_liveview: Camera not connected\n");
        return -1;
    }

    if (g_liveview_active) {
        if (verbose_logging) {
            printf("Live view already active\n");
        }
        return 0; // Already running
    }

    if (verbose_logging) {
        printf("Starting live view...\n");
    }

    // Start live view using XSDK_SetProp
    long result = XSDK_SetProp(g_hCamera, XT3_API_CODE_StartLiveView, XT3_API_PARAM_StartLiveView);

    if (result != 0) {
        fprintf(stderr, "fm_start_liveview: XSDK_SetProp failed with code: %ld\n", result);
        fprintf(stderr, "  Note: Live view requires ReadImage buffer to be empty\n");
        fprintf(stderr, "  Make sure no images are waiting to be downloaded\n");
        return -2;
    }

    g_liveview_active = 1;

    if (verbose_logging) {
        printf("Live view started successfully\n");
    }

    return 0;
}

int fm_stop_liveview() {
    if (g_hCamera == NULL) {
        fprintf(stderr, "fm_stop_liveview: Camera not connected\n");
        return -1;
    }

    if (!g_liveview_active) {
        if (verbose_logging) {
            printf("Live view not active\n");
        }
        return 0; // Already stopped
    }

    if (verbose_logging) {
        printf("Stopping live view...\n");
    }

    // Stop live view using XSDK_SetProp
    long result = XSDK_SetProp(g_hCamera, XT3_API_CODE_StopLiveView, XT3_API_PARAM_StopLiveView);

    if (result != 0) {
        fprintf(stderr, "fm_stop_liveview: XSDK_SetProp failed with code: %ld\n", result);
        return -2;
    }

    g_liveview_active = 0;

    if (verbose_logging) {
        printf("Live view stopped successfully\n");
    }

    return 0;
}

int fm_get_liveview_frame(unsigned char** buffer, int* size) {
    if (g_hCamera == NULL) {
        fprintf(stderr, "fm_get_liveview_frame: Camera not connected\n");
        return -1;
    }

    if (buffer == NULL || size == NULL) {
        fprintf(stderr, "fm_get_liveview_frame: NULL parameters\n");
        return -2;
    }

    if (!g_liveview_active) {
        fprintf(stderr, "fm_get_liveview_frame: Live view not active\n");
        return -3;
    }

    *buffer = NULL;
    *size = 0;

    // Read live view image info
    XSDK_ImageInformation img_info;
    memset(&img_info, 0, sizeof(img_info));
    long result = XSDK_ReadImageInfo(g_hCamera, &img_info);

    if (result != 0) {
        if (verbose_logging) {
            fprintf(stderr, "fm_get_liveview_frame: XSDK_ReadImageInfo failed with code: %ld\n", result);
        }
        return -4;
    }

    if (img_info.lDataSize <= 0) {
        if (verbose_logging) {
            fprintf(stderr, "fm_get_liveview_frame: No frame data available (size: %ld)\n", img_info.lDataSize);
        }
        return -5;
    }

    // Allocate buffer for JPEG data
    unsigned char* jpeg_buffer = (unsigned char*)malloc(img_info.lDataSize);
    if (jpeg_buffer == NULL) {
        fprintf(stderr, "fm_get_liveview_frame: Failed to allocate %ld bytes\n", img_info.lDataSize);
        return -6;
    }

    // Read the JPEG frame
    result = XSDK_ReadImage(g_hCamera, jpeg_buffer, img_info.lDataSize);
    if (result != 0) {
        fprintf(stderr, "fm_get_liveview_frame: XSDK_ReadImage failed with code: %ld\n", result);
        free(jpeg_buffer);
        return -7;
    }

    // Delete the frame from buffer (live view generates continuous frames)
    XSDK_DeleteImage(g_hCamera);

    *buffer = jpeg_buffer;
    *size = (int)img_info.lDataSize;

    if (verbose_logging) {
        printf("Got live view frame: %d bytes\n", *size);
    }

    return 0;
}

void fm_free_liveview_frame(unsigned char* buffer) {
    if (buffer != NULL) {
        free(buffer);
    }
}

int fm_is_liveview_active(int* is_active) {
    if (is_active == NULL) {
        fprintf(stderr, "fm_is_liveview_active: is_active pointer is NULL\n");
        return -1;
    }

    // X-T3 doesn't have GetLiveViewStatus, so we track state manually
    *is_active = g_liveview_active;

    if (verbose_logging) {
        printf("Live view active: %s\n", g_liveview_active ? "YES" : "NO");
    }

    return 0;
}

int fm_set_liveview_size(int size_code) {
    if (g_hCamera == NULL) {
        fprintf(stderr, "fm_set_liveview_size: Camera not connected\n");
        return -1;
    }

    // Map size code to SDK constant
    long sdk_size;
    const char* size_name;

    switch (size_code) {
        case 0:
            sdk_size = XT3_LIVEVIEW_SIZE_S;
            size_name = "Small (~320px)";
            break;
        case 1:
            sdk_size = XT3_LIVEVIEW_SIZE_M;
            size_name = "Medium (~640px)";
            break;
        case 2:
            sdk_size = XT3_LIVEVIEW_SIZE_L;
            size_name = "Large (~1024px)";
            break;
        default:
            fprintf(stderr, "fm_set_liveview_size: Invalid size code (must be 0, 1, or 2)\n");
            return -2;
    }

    if (verbose_logging) {
        printf("Setting live view size to %s...\n", size_name);
    }

    // Set live view image size
    long result = XSDK_SetProp(g_hCamera, XT3_API_CODE_SetLiveViewImageSize,
                               XT3_API_PARAM_SetLiveViewImageSize, sdk_size);

    if (result != 0) {
        fprintf(stderr, "fm_set_liveview_size: XSDK_SetProp failed with code: %ld\n", result);
        return -3;
    }

    if (verbose_logging) {
        printf("Live view size set to %s successfully\n", size_name);
    }

    return 0;
}

// ========== End Live View Functions ==========

int fm_set_verbose(int enabled) {
    verbose_logging = enabled;
    if (verbose_logging) {
        printf("Verbose logging enabled\n");
    }
    return 0;
}
