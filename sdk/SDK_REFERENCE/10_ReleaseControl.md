# 10. Release Control (Shutter Trigger)

## Overview
These functions control the physical action of the camera shutter mechanism, including autofocus (AF) and exposure lock (AE) operations where applicable.

**State Requirement:** **S3 (Session)** and **PC Priority** mode required.

---

## Release Control Functions

The SDK offers two parallel sets of functions depending on the established Priority Mode (see 09_CameraOperationMode.md). For fujimatic, we rely on PC Priority.

### PC Priority Functions (Required for Automated Shooting)

These functions operate when `XSDK_SetPriorityMode` has been set to `XSDK_PRIORITY_MODE_PC`.

#### 1. XSDK_CapRelease
- **Purpose:** Queries the camera's capabilities regarding supported release functions (AF, AE Lock, Shutter Trigger).
- **Usage:** Call this *before* attempting `XSDK_Release` to ensure the camera supports the required sequence (e.g., checking if AF is required before shutter).

#### 2. XSDK_Release (The Capture Command)
- **Purpose:** Triggers the capture sequence: Autofocus → Auto Exposure Lock → Shutter Firing → Image Acquisition.
- **Signature (Inferred/Contextual):** `XSDK_APIENTRY XSDK_Release(XSDK_HANDLE hCamera, long lOperation);` (Operation specifies AF/AE behavior).
- **Usage:**
    ```c
    // Assuming PC Priority is set
    // lOperation typically specifies a combination of AF/AE stages to run concurrently
    result = XSDK_Release(hCamera, XSDK_RELEASE_OPERATION_AF_AE_SHUTTER); 
    ```

#### 3. XSDK_GetReleaseStatus
- **Purpose:** Checks the current status of an ongoing release operation (`XSDK_Release`).
- **Usage:** Must be polled after calling `XSDK_Release` until the status indicates completion or error. Crucial for synchronization before attempting download.
- **Polling Pattern:** Poll rapidly until status is NOT `XSDK_RELEASE_STATUS_RUNNING`.

### Camera Priority Functions (Not Used for MVP)
- `XSDK_CapReleaseEx`, `XSDK_ReleaseEx`, `XSDK_GetReleaseStatusEx`: These are used when the camera dial is set to Camera Priority. They are generally avoided for fully automated tethered control.

---

## Synchronization for Plate-Solving (MVP Requirement)

The process must be completely synchronous:

1. **Set Priority:** Ensure PC Priority is active.
2. **Set Parameters:** Set desired shutter speed (`XSDK_SetShutterSpeed`).
3. **Trigger:** Call `XSDK_Release(hCamera, ...)`
4. **Wait for Capture:** Poll `XSDK_GetReleaseStatus()` until complete. This is the **wait time for the physical exposure**.
5. **Download:** Immediately proceed to Image Acquisition (`XSDK_ReadImage`).

### Pitfall Warning: Release Status vs. Download Status
- `XSDK_GetReleaseStatus` indicates the *shutter* part is done (image is now in buffer).
- It **DOES NOT** wait for the image to be transferred or buffered internally.
- You must wait for Release Status to complete **before** calling `XSDK_ReadImageInfo()` or `XSDK_ReadImage()`.
