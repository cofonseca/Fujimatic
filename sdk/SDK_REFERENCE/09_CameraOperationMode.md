# 9. Camera Operation Mode Control

## Overview
Operation mode functions control how the camera responds to external commands (PC Priority vs Camera Priority) and the shooting characteristics (Drive Mode, Camera Mode).

**State Requirement:** Typically **S3 (Session)** required after opening the connection.

---

## Priority Mode Control (PC vs. Camera Control)

This determines who has control over basic camera state changes. For tethered shooting, **PC Priority must be set**.

### Key Functions
- `XSDK_CapPriorityMode()`: Query supported priority modes.
- `XSDK_SetPriorityMode(mode)`: Switch control mode.
- `XSDK_GetPriorityMode()`: Read current mode.

### Priority Modes
- **PC Priority (`XSDK_PRIORITY_MODE_PC`)**: Camera waits for computer commands (`XSDK_Release`, `XSDK_SetShutterSpeed`). This is required for reliable automation.
- **Camera Priority (`XSDK_PRIORITY_MODE_CAMERA`)**: Camera ignores tethered commands and relies on physical dials/buttons.

### Setting PC Priority (Critical for MVP)
```c
// Must be done after session open (S2->S3)
result = XSDK_SetPriorityMode(hCamera, XSDK_PRIORITY_MODE_PC);

if (result != XSDK_COMPLETE) {
    // Handle error - camera might not support PC control
}
```

---

## Camera Mode Control (P/A/S/M)

Controls the exposure mode set on the main camera dial.

### Key Functions
- `XSDK_CapMode()`: Query supported modes (Program, Aperture, Shutterspeed, Manual, Bulb via `XSDK_DSC_MODE_BV`)
- `XSDK_SetMode(mode)`: Set camera mode (e.g., `XSDK_MODE_M` for Manual)
- `XSDK_GetMode()`: Read current mode

### Usage Note for fujimatic
To control exposure settings programmatically, the camera should generally be set to **Manual Mode (`XSDK_MODE_M`)** or **Aperture Priority (`XSDK_MODE_A`)** if controlling Shutter Speed via API is desired, or **Shutter Priority (`XSDK_MODE_S`)** if controlling Aperture via API is desired (though aperture API access is often model-dependent). For full control, M mode is preferred.

---

## Drive Mode Control

Controls burst settings (Single Shot, Continuous High, Continuous Low, Self-Timer).

### Key Functions
- `XSDK_CapDriveMode()`: Query supported drive modes.
- `XSDK_SetDriveMode(mode)`: Set drive mode.
- `XSDK_GetDriveMode()`: Read current mode.

### Drive Mode Constants
- `XSDK_DRIVE_MODE_SINGLE`: Single exposure
- `XSDK_DRIVE_MODE_CONT_H`: Continuous High Speed
- `XSDK_DRIVE_MODE_CONT_L`: Continuous Low Speed
- `XSDK_DRIVE_MODE_SELF_TIMER_10S`: Self-timer

### Crucial for Intervalometer (Story C-1)
For reliable intervalometer operation, **Drive Mode must typically be set to Single Shot (`XSDK_DRIVE_MODE_SINGLE`)** before starting the loop. Continuous modes may interfere with download timing and buffer management during high-speed sequences.
