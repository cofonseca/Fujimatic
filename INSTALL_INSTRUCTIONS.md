# Fujimatic Installation Guide

Complete setup instructions for installing Fujimatic on a new Windows computer.

## Table of Contents

- [Overview](#overview)
- [System Requirements](#system-requirements)
- [Installation Steps](#installation-steps)
- [Camera Setup](#camera-setup)
- [Verification](#verification)
- [Troubleshooting](#troubleshooting)
- [Optional Features](#optional-features)

---

## Overview

Fujimatic is a CLI/TUI application for tethered control of Fujifilm cameras (tested with X-T3). This guide will help you set up Fujimatic from a GitHub release package.

**What's Included in the Release:**
- `fujimatic.exe` - Main application
- `fmwrapper.dll` - Fujifilm SDK wrapper library
- `libraw_wrapper.dll` - RAF conversion library (optional feature)
- `libraw-23.dll` - LibRaw dependency (optional feature)

**What You Need to Obtain Separately:**
- Fujifilm Camera Control SDK (proprietary - see below)
- USB cable compatible with your camera

---

## System Requirements

### Hardware
- **Computer:** Windows 10/11 (64-bit)
- **Camera:** Fujifilm X-T3 (other X-series cameras may work but are untested)
- **USB Cable:** USB 3.0 cable (USB-C to USB-A or USB-C to USB-C depending on your camera/computer)
- **Storage:** At least 10 GB free space for captured RAF files

### Software Prerequisites
- **Operating System:** Windows 10 version 1809 or later (Windows 11 recommended)
- **No additional runtime required** - fujimatic.exe is a standalone binary

---

## Installation Steps

### Step 1: Download Fujimatic Release

1. Go to the [Fujimatic Releases page](https://github.com/cfonseca/fujimatic/releases)
2. Download the latest release ZIP file: `fujimatic-windows-x64-vX.X.X.zip`
3. Extract the ZIP to a permanent location (e.g., `C:\Program Files\fujimatic\`)

**Recommended directory structure:**
```
C:\Program Files\fujimatic\
├── fujimatic.exe
├── fmwrapper.dll
├── libraw_wrapper.dll
├── libraw-23.dll
└── (SDK DLLs will go here - see Step 2)
```

### Step 2: Obtain Fujifilm SDK

**IMPORTANT:** The Fujifilm Camera Control SDK is **proprietary software** and cannot be redistributed with Fujimatic.

**How to Get the SDK:**

1. **Contact Fujifilm:** Email Fujifilm technical support requesting access to the "Camera Control SDK" for developers
   - Support email: (varies by region - check Fujifilm's developer website)
   - Explain you're using it for personal/educational astrophotography purposes

2. **Download from Fujifilm Developer Portal:** If you have developer access
   - Log in to the Fujifilm Developer Portal
   - Download the "Camera Control SDK for Windows" package
   - Accept the SDK license agreement

**What You'll Receive:**
- SDK package containing DLL files and documentation
- License agreement (read carefully - personal use typically allowed)

### Step 3: Install SDK DLLs

After obtaining the SDK package:

1. **Locate the redistributable DLLs** in the SDK package:
   ```
   SDK_REDISTRIBUTABLES/
   └── Windows/
       └── 64bit/
           ├── XAPI.dll
           ├── FF0000API.dll
           ├── FF0001API.dll
           ├── FF0002API.dll
           ├── FF0003API.dll
           ├── (... more FF*.dll files)
           ├── FTLPTP.dll
           └── FTLPTPIP.dll
   ```

2. **Copy ALL DLL files** to your Fujimatic installation directory:
   ```
   Copy from: SDK_REDISTRIBUTABLES/Windows/64bit/*.dll
   Copy to:   C:\Program Files\fujimatic\
   ```

3. **Verify all files are present:**
   ```
   C:\Program Files\fujimatic\
   ├── fujimatic.exe         ✓
   ├── fmwrapper.dll         ✓
   ├── libraw_wrapper.dll    ✓
   ├── libraw-23.dll         ✓
   ├── XAPI.dll              ✓ (from SDK)
   ├── FF0000API.dll         ✓ (from SDK)
   ├── FF0001API.dll         ✓ (from SDK)
   ├── (... more FF*.dll)    ✓ (from SDK)
   ├── FTLPTP.dll            ✓ (from SDK)
   └── FTLPTPIP.dll          ✓ (from SDK)
   ```

### Step 4: Configure Environment Variables

Fujimatic needs to know where the SDK DLLs are located at runtime.

**Windows 10/11:**

1. Press `Windows + R`, type `sysdm.cpl`, press Enter
2. Click the **"Advanced"** tab
3. Click **"Environment Variables..."**
4. Under **"User variables"**, click **"New..."**
5. Set:
   - **Variable name:** `FUJI_SDK_PATH`
   - **Variable value:** `C:\Program Files\fujimatic`
6. Click **OK** to save
7. **IMPORTANT:** Restart any open Command Prompt or PowerShell windows for the change to take effect

**Verify Environment Variable:**
```cmd
echo %FUJI_SDK_PATH%
```
Should output: `C:\Program Files\fujimatic`

### Step 5: Add to PATH (Optional but Recommended)

This allows you to run `fujimatic` from any directory.

1. In the **Environment Variables** window (from Step 4)
2. Under **"User variables"**, find and select **"Path"**
3. Click **"Edit..."**
4. Click **"New"**
5. Enter: `C:\Program Files\fujimatic`
6. Click **OK** on all windows
7. Restart Command Prompt

**Verify:**
```cmd
fujimatic --version
```

---

## Camera Setup

### Step 1: Camera Configuration

Configure your Fujifilm X-T3 for tethered operation:

1. **Power on** your camera
2. Navigate to **MENU → CONNECTION SETTING → USB MODE**
3. Set USB mode to: **"USB TETHER SHOOTING AUTO"** or **"USB CARD READER"**
   - Try both if one doesn't work
4. Set **"USB POWER SUPPLY"** to **"OFF"** (prevents battery charging interference)

### Step 2: Connect Camera to Computer

1. Use a **USB 3.0 cable** (USB-C to USB-A or USB-C to USB-C)
2. Connect camera to computer
3. Turn camera **ON**
4. Windows should detect the camera (you may see a notification)

**IMPORTANT:** Do **NOT** install Fujifilm's official tethering software (X Acquire) if you want to use Fujimatic. They will conflict.

### Step 3: Verify Camera Detection

Windows should recognize the camera as a USB device:

1. Open **Device Manager** (Windows + X → Device Manager)
2. Look for **"Imaging devices"** or **"Cameras"**
3. You should see your camera listed (e.g., "FUJIFILM X-T3")

If the camera doesn't appear, try:
- Different USB port (prefer USB 3.0 ports - often blue)
- Different USB cable
- Restarting the camera

---

## Verification

### Test 1: CLI Connection Test

Open Command Prompt or PowerShell:

```cmd
cd "C:\Program Files\fujimatic"
fujimatic.exe --version
```

**Expected output:**
```
Fujimatic v1.0.0
Fujifilm Camera Control CLI
```

### Test 2: Camera Connection Test

```cmd
fujimatic.exe
```

At the prompt, type:
```
connect
```

**Expected output:**
```
✓ SDK initialized successfully
✓ Connected to camera: FUJIFILM X-T3
✓ Battery: 85%
```

If successful, type `exit` to quit.

### Test 3: Test Capture (Quick)

```cmd
fujimatic.exe
```

At the prompt:
```
connect
session start test_captures ./test_output
capture
exit
```

**Expected:**
- Camera shutter fires
- Image downloads to `./test_output/test_captures_0001.RAF`
- No error messages

**SUCCESS!** ✓ Fujimatic is working correctly.

---

## Troubleshooting

### Problem: "fujimatic.exe is not recognized"

**Solution:**
- Make sure you're in the correct directory: `cd "C:\Program Files\fujimatic"`
- Or add Fujimatic to PATH (see Step 5 above)

### Problem: "XAPI.dll not found" or "Cannot find SDK DLLs"

**Solution:**
1. Verify XAPI.dll exists in the Fujimatic directory
2. Check `FUJI_SDK_PATH` environment variable is set correctly:
   ```cmd
   echo %FUJI_SDK_PATH%
   ```
3. Restart Command Prompt after setting environment variables

### Problem: "Failed to initialize SDK"

**Causes:**
- SDK DLLs missing or wrong version (must be 64-bit)
- `FUJI_SDK_PATH` not set or pointing to wrong directory
- Incompatible SDK version

**Solution:**
1. Verify all SDK DLLs are in place (see Step 3)
2. Try running as Administrator (right-click → "Run as administrator")
3. Check Windows Event Viewer for DLL loading errors

### Problem: "Camera not found" or "Failed to connect"

**Causes:**
- Camera not powered on
- Wrong USB mode selected on camera
- USB cable issue
- Camera driver conflict

**Solution:**
1. Ensure camera is ON and in USB Tether mode
2. Try unplugging/replugging the USB cable
3. Try a different USB port (prefer rear panel USB 3.0 ports)
4. Restart the camera
5. Check Device Manager for yellow warning icons under "Imaging devices"
6. Uninstall any Fujifilm official tethering software (X Acquire)

### Problem: "Camera not ready" errors during capture

**Solution:**
- Wait 5-10 seconds after connecting before first capture
- If using live view, stop it before starting intervalometer
- Increase intervalometer delay to 5+ seconds between captures

### Problem: RAF files won't open

**Causes:**
- Incomplete downloads
- Disk full
- File corruption

**Solution:**
- Check free disk space (RAF files are ~55-60 MB each)
- Verify file size matches expected (~56 MB for X-T3)
- Try a different capture directory on a different drive

---

## Optional Features

### RAF to FITS/TIFF Conversion

If you want to convert RAF files to FITS (astronomy) or TIFF formats:

**Prerequisites (already included in release):**
- `libraw_wrapper.dll` ✓
- `libraw-23.dll` ✓

**Usage:**
```cmd
fujimatic.exe
```

At the prompt:
```
connect
session start astro ./captures
conversion set fits
capture
```

The RAF file will be automatically converted to FITS format after download.

**Formats supported:**
- `fits` - Flexible Image Transport System (astronomy standard)
- `tiff` - Tagged Image File Format (compressed, 16-bit per channel)
- `none` - No conversion (default)

### Web UI Mode (Live View)

Start Fujimatic in server mode for browser-based control:

```cmd
fujimatic.exe server --port 8080
```

Then open in your browser:
```
http://localhost:8080
```

**Features:**
- Live view streaming
- Intervalometer with progress display
- Capture previews (thumbnails extracted from RAF files)
- Settings control (ISO, shutter speed, focus mode)

---

## Directory Structure Reference

**Recommended directory layout:**

```
C:\
├── Program Files\
│   └── fujimatic\                    ← Installation directory
│       ├── fujimatic.exe            ← Main executable
│       ├── fmwrapper.dll            ← Fujifilm SDK wrapper
│       ├── libraw_wrapper.dll       ← RAF conversion wrapper
│       ├── libraw-23.dll            ← LibRaw library
│       ├── XAPI.dll                 ← Fujifilm SDK (from SDK package)
│       ├── FF*.dll                  ← Fujifilm SDK (from SDK package)
│       ├── FTLPTP.dll               ← Fujifilm SDK (from SDK package)
│       └── FTLPTPIP.dll             ← Fujifilm SDK (from SDK package)
│
└── Users\
    └── YourName\
        └── Documents\
            └── Astrophotography\    ← Your capture directories
                ├── M42_2025-01-15\  ← Session 1
                ├── M31_2025-01-20\  ← Session 2
                └── ...
```

---

## Next Steps

- **Read the README.md** for usage examples and CLI commands
- **Read HARDWARE_TESTING_PLAN.md** for detailed camera testing procedures
- **Check STORIES.md** for feature status and roadmap
- **Join discussions** on GitHub for tips and troubleshooting

---

## Support

- **GitHub Issues:** https://github.com/cfonseca/fujimatic/issues
- **Documentation:** Check the `/docs` folder in the repository
- **SDK Issues:** Contact Fujifilm developer support directly

---

## License Notes

- **Fujimatic:** Open source (see LICENSE file)
- **Fujifilm SDK:** Proprietary - subject to Fujifilm's SDK license agreement
- **LibRaw:** LGPL v2.1 / CDDL (dual license)

**IMPORTANT:** The Fujifilm SDK is proprietary software. You must obtain it separately and comply with Fujifilm's licensing terms. Fujimatic does not include or redistribute any Fujifilm SDK components.

---

*Last updated: 2025-11-17*
*For Fujimatic version: 1.0.0+*
