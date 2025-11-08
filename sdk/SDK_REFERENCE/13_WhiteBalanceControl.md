# 13. White Balance Control

## Overview
These functions allow programmatic control over the camera's white balance setting, which dictates color temperature compensation.

**State Requirement:** **S3 (Session)**.

---

## Key Functions

### 1. White Balance Mode Control
- **`XSDK_CapWBMode()`**: Queries supported White Balance modes (Auto, Daylight, Shade, Tungsten, Daylight Fluorescent, Custom Color Temp).
- **`XSDK_SetWBMode(mode)`**: Sets the operating mode.
- **`XSDK_GetWBMode()`**: Reads the current mode.

**MVP Note:** For automated capture, setting a specific fixed temperature or using a known supported preset (like Daylight) is safer than Auto.

### 2. Color Temperature Control (When Mode is Set to Custom Temp)
If the supported mode allows setting a specific color temperature (Kelvin), these functions are used:

- **`XSDK_CapWBColorTemp()`**: Queries the valid range and step size for absolute color temperature settings (e.g., min 2800K, max 10000K, step 100).
- **`XSDK_SetWBColorTemp(temp)`**: Sets the color temperature value (e.g., 5600 for daylight).
- **`XSDK_GetWBColorTemp()`**: Reads the current Kelvin setting.

### Usage Pattern
1. Check if WB is controllable via SDK using `XSDK_CapMode()`.
2. Query the temperature range using `XSDK_CapWBColorTemp()` if custom temperature is desired.
3. Set mode:
    ```c
    // Set WB to a known manual temperature setting
    XSDK_SetWBMode(hCamera, XSDK_WB_MODE_COLOR_TEMP);
    
    // Set temperature (assuming 5600K is supported)
    XSDK_SetWBColorTemp(hCamera, 5600);
    ```

---

## Best Practices for Astrophotography

- **Avoid Auto/Presets:** Auto features can react to environmental changes (e.g., a nearby light turning on), leading to inconsistent color calibration across frames required for good plate-solving/stacking.
- **Use Constant Value:** Select a fixed, known Kelvin value (e.g., 3200K for Tungsten or 5600K daylight) and use `XSDK_SetWBColorTemp` to lock it in.
