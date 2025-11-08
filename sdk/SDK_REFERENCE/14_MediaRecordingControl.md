# 14. Media Recording Control

## Overview
These functions control where captured still images or movies are recorded (Camera SD Card, Host Computer, or Both).

**State Requirement:** **S3 (Session)**.

---

## Key Functions

### 1. XSDK_CapMediaRecord
- **Purpose:** Queries the camera's supported storage destinations and recording modes.

### 2. XSDK_SetMediaRecord (Critical for Tethering)
- **Purpose:** Defines the destination for captured images.
- **Signature Note:** Takes a mode setting that specifies one or combination of destinations.

### Media Recording Modes (Inferred Constants)
These modes are typically set via bitwise flags:
- `XSDK_REC_TO_SDCARD`: Save to internal SD card slot.
- `XSDK_REC_TO_HOST`: Save to connected computer via USB/Network link.
- `XSDK_REC_TO_BOTH`: Save to SD card AND host computer (default in many scenarios).

### Usage Pattern for Tethering (No SD Card Backup)

For the plate-solving MVP, we require the image to land on the local filesystem immediately, meaning we should preferably send it *only* to the host, or explicitly ensure it's saved to the host directory used by `XSDK_ReadImage`. The API overview suggests saving images to the computer.

```c
// Set camera to save images ONLY to the computer
result = XSDK_SetMediaRecord(hCamera, XSDK_REC_TO_HOST); 

if (result != XSDK_COMPLETE) {
    HandleError(hCamera, "XSDK_SetMediaRecord");
    // If XSDK_REC_TO_HOST is not supported, you must use XSDK_REC_TO_BOTH 
    // and rely purely on XSDK_ReadImage to get the file, as XSDK_ReadImage 
    // downloads the file *before* the SD card write completes.
}
```

### 3. XSDK_GetMediaRecord
- **Purpose:** Reads the current recording destination setting.

---

## Interplay with Image Acquisition (11_ImageAcquisition.md)

While `XSDK_SetMediaRecord` tells the camera where to write files persistently, the **`XSDK_ReadImage` function bypasses the persistent file system** by pulling the data directly from the volatile capture buffer.

**Conclusion for MVP:**
Even if `XSDK_REC_TO_HOST` is successfully set:
1. **Always** use `XSDK_ReadImage` to ensure immediate availability for plate-solving.
2. Relying on the persistent SD card or host save via the SDK post-capture is secondary/less reliable for our synchronous needs.
