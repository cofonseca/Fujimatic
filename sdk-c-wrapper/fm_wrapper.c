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

// Global state
static LIB_HANDLE g_hLib = NULL;
static XSDK_HANDLE g_hCamera = NULL;

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
    printf("Loading XAPI.dll from current directory...\n");
    g_hLib = LoadLibraryA("XAPI.dll");
    if (g_hLib == NULL) {
        DWORD error = GetLastError();
        fprintf(stderr, "fm_init: Failed to load XAPI.dll from current directory (Error: %lu)\n", error);
        fprintf(stderr, "  Make sure XAPI.dll and all FF*.dll files are in the same directory as the executable\n");
        return -2;
    }
    printf("XAPI.dll loaded successfully\n");
#else
    // On Linux, load libFXAPI.so from current directory
    printf("Loading libFXAPI.so from current directory...\n");
    g_hLib = dlopen("./libFXAPI.so", RTLD_NOW);
    if (g_hLib == NULL) {
        fprintf(stderr, "fm_init: Failed to load libFXAPI.so: %s\n", dlerror());
        fprintf(stderr, "  Make sure libFXAPI.so is in the current directory\n");
        return -2;
    }
    printf("libFXAPI.so loaded successfully\n");
#endif

    // Initialize the SDK
    printf("Calling XSDK_Init...\n");
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

    printf("Fujifilm SDK initialized successfully\n");
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

    printf("Detecting cameras on USB interface...\n");
    long num_cameras = 0;
    long result = XSDK_Detect(XSDK_DSC_IF_USB, "", NULL, &num_cameras);

    printf("XSDK_Detect result: %ld, count: %ld\n", result, num_cameras);

    if (result != 0) {
        fprintf(stderr, "fm_connect: XSDK_Detect failed with code: %ld\n", result);
        return -3;
    }

    if (num_cameras == 0) {
        fprintf(stderr, "fm_connect: No cameras detected\n");
        fprintf(stderr, "  Possible issues:\n");
        fprintf(stderr, "  - Camera not powered on\n");
        fprintf(stderr, "  - USB cable not connected\n");
        fprintf(stderr, "  - Camera in wrong USB mode (try PC AUTO SAVE or MTP mode)\n");
        fprintf(stderr, "  - USB drivers not installed (install Fujifilm X Acquire or MyFinePix Studio)\n");
        return -4;
    }

    printf("Found %ld camera(s) - opening first camera with ENUM:0\n", num_cameras);

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

    printf("Setting camera to PC priority mode...\n");
    result = XSDK_SetPriorityMode(g_hCamera, XSDK_PRIORITY_PC);
    if (result != 0) {
        fprintf(stderr, "fm_connect: XSDK_SetPriorityMode failed with code: %ld\n", result);
        XSDK_Close(g_hCamera);
        g_hCamera = NULL;
        return -6;
    }

    printf("Setting media recording to RAW+JPEG...\n");
    result = XSDK_SetMediaRecord(g_hCamera, XSDK_MEDIAREC_RAWJPEG);
    if (result != 0) {
        fprintf(stderr, "fm_connect: XSDK_SetMediaRecord failed with code: %ld\n", result);
        XSDK_Close(g_hCamera);
        g_hCamera = NULL;
        return -7;
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

    long result = XSDK_Close(g_hCamera);
    if (result != 0) {
        fprintf(stderr, "fm_disconnect: XSDK_Close failed with code: %ld\n", result);
        return -2;
    }

    g_hCamera = NULL;
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
    printf("Battery info - param1=%ld, param2=%ld, param3=%ld, param4=%ld, param5=%ld, param6=%ld\n",
           param1, param2, param3, param4, param5, param6);

    *percent = convert_battery_to_percent(param1);
    return 0;
}

int fm_set_shutter(int seconds) {
    if (g_hCamera == NULL) {
        fprintf(stderr, "fm_set_shutter: Camera not connected\n");
        return -1;
    }

    if (seconds < 0) {
        fprintf(stderr, "fm_set_shutter: Invalid shutter speed (negative)\n");
        return -2;
    }

    // Convert seconds to microseconds
    long shutter_microseconds = (long)seconds * 1000000;
    long bulb = 0; // Not using bulb mode for MVP

    long result = XSDK_SetShutterSpeed(g_hCamera, shutter_microseconds, bulb);
    if (result != 0) {
        fprintf(stderr, "fm_set_shutter: XSDK_SetShutterSpeed failed with code: %ld\n", result);
        return -3;
    }

    printf("Shutter speed set to %d seconds\n", seconds);
    return 0;
}

int fm_capture() {
    if (g_hCamera == NULL) {
        fprintf(stderr, "fm_capture: Camera not connected\n");
        return -1;
    }

    // Trigger capture using XSDK_Release
    long shot_opt = 0;
    long af_status = 0;
    long result = XSDK_Release(g_hCamera, 0, &shot_opt, &af_status);
    if (result != 0) {
        fprintf(stderr, "fm_capture: XSDK_Release failed with code: %ld\n", result);
        return -2;
    }

    printf("Capture triggered successfully\n");
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
