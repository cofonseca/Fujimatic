# Fujimatic Installation Guide

Complete setup instructions for installing Fujimatic on a new Windows computer.

## Table of Contents

- [Overview](#overview)
- [System Requirements](#system-requirements)
- [Installation Steps](#installation-steps)
- [Quick Start: Migrating from Another Computer](#quick-start-migrating-from-another-computer)
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
- **MinGW Runtime DLLs** (7 files - required for libraw):
  - `libgcc_s_seh-1.dll` - GCC runtime
  - `libgomp-1.dll` - OpenMP support
  - `libstdc++-6.dll` - C++ standard library
  - `libwinpthread-1.dll` - Threading support
  - `libjpeg-8.dll` - JPEG support for LibRaw
  - `liblcms2-2.dll` - Color management
  - `zlib1.dll` - Compression library

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
- **Architecture:** 64-bit Windows only (x64) - will NOT work on 32-bit or ARM Windows
- **Runtime Dependencies:**
  - Microsoft Visual C++ Redistributable (usually already installed on Windows 10/11)
  - If missing, download from: https://aka.ms/vs/17/release/vc_redist.x64.exe
  - Windows will prompt you if this is needed

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
├── libgcc_s_seh-1.dll
├── libgomp-1.dll
├── libstdc++-6.dll
├── libwinpthread-1.dll
├── libjpeg-8.dll
├── liblcms2-2.dll
├── zlib1.dll
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
   ├── libgcc_s_seh-1.dll    ✓ (MinGW runtime)
   ├── libgomp-1.dll         ✓ (MinGW runtime)
   ├── libstdc++-6.dll       ✓ (MinGW runtime)
   ├── libwinpthread-1.dll   ✓ (MinGW runtime)
   ├── libjpeg-8.dll         ✓ (MinGW runtime)
   ├── liblcms2-2.dll        ✓ (MinGW runtime)
   ├── zlib1.dll             ✓ (MinGW runtime)
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

## Quick Start: Migrating from Another Computer

**If you're copying Fujimatic from an existing installation** (e.g., from another laptop where it's already working), follow this streamlined process instead of the full installation steps above.

### Prerequisites Check

- ✓ Both computers running **64-bit Windows 10/11** (will NOT work on 32-bit or ARM)
- ✓ You have the **complete fujimatic directory** with all DLLs from the working installation
- ✓ The directory includes both Fujimatic DLLs **and** Fujifilm SDK DLLs

### Migration Steps (3 Required, 1 Optional)

#### Step 1: Copy the Directory

Copy the **entire fujimatic folder** to your new computer:

```
From old computer:  C:\Program Files\fujimatic\
To new computer:    C:\Program Files\fujimatic\  (or any location you prefer)
```

**Verify all files copied:**
```
fujimatic.exe          ✓
fmwrapper.dll          ✓
libraw_wrapper.dll     ✓
libraw-23.dll          ✓
libgcc_s_seh-1.dll     ✓ (MinGW runtime - CRITICAL)
libgomp-1.dll          ✓ (MinGW runtime - CRITICAL)
libstdc++-6.dll        ✓ (MinGW runtime - CRITICAL)
libwinpthread-1.dll    ✓ (MinGW runtime - CRITICAL)
libjpeg-8.dll          ✓ (MinGW runtime)
liblcms2-2.dll         ✓ (MinGW runtime)
zlib1.dll              ✓ (MinGW runtime)
XAPI.dll               ✓ (Fujifilm SDK)
FF*.dll                ✓ (Fujifilm SDK - multiple files)
FTLPTP.dll             ✓ (Fujifilm SDK)
FTLPTPIP.dll           ✓ (Fujifilm SDK)
```

**IMPORTANT:** Missing any of the MinGW DLLs will cause fujimatic to exit immediately with no error message or a "libgcc_s_seh-1.dll not found" error.

#### Step 2: Set FUJI_SDK_PATH (REQUIRED)

**This is CRITICAL** - Fujimatic won't work without this environment variable.

1. Press `Windows + R`, type `sysdm.cpl`, press Enter
2. Click **"Advanced"** tab → **"Environment Variables..."**
3. Under **"User variables"**, click **"New..."**
4. Set:
   - **Variable name:** `FUJI_SDK_PATH`
   - **Variable value:** `C:\Program Files\fujimatic` (or wherever you copied it)
5. Click **OK** to save
6. **CRITICAL:** Restart any open Command Prompt/PowerShell windows

**Verify:**
```cmd
echo %FUJI_SDK_PATH%
```
Should output your fujimatic directory path.

#### Step 3: Restart Command Prompt

Environment variables only take effect in **new** terminal windows:
- Close all Command Prompt and PowerShell windows
- Open a fresh Command Prompt or PowerShell

#### Step 4: Add to PATH (Optional)

Only needed if you want to run `fujimatic` from any directory (instead of having to `cd` to the fujimatic folder first).

Follow the same process as [Step 5](#step-5-add-to-path-optional-but-recommended) in the full installation guide above.

### Quick Verification

Test that everything works:

```cmd
cd "C:\Program Files\fujimatic"
fujimatic.exe --version
```

**Expected output:**
```
Fujimatic v1.0.0
Fujifilm Camera Control CLI
```

If you see this, **you're done!** Connect your camera and test with the [Verification](#verification) steps below.

### What Doesn't Transfer (Must Configure Manually)

When copying between computers, these settings do **NOT** transfer automatically:

- ✗ `FUJI_SDK_PATH` environment variable → **You must set this manually**
- ✗ `PATH` environment variable → Optional, for convenience only
- ✗ Application shortcuts → Create new ones if desired

### What Transfers Automatically

These components copy with the directory:

- ✓ All executable code (fujimatic.exe)
- ✓ All DLL dependencies (Fujimatic + SDK)
- ✓ All functionality (no reinstallation needed)

### Common Migration Issues

#### Problem: "libgcc_s_seh-1.dll was not found" or fujimatic exits immediately with no error

**Cause:** MinGW runtime DLLs missing from fujimatic directory.

**Solution:**
1. Go back to your working laptop
2. Verify these 7 DLLs are in the fujimatic directory:
   - `libgcc_s_seh-1.dll`
   - `libgomp-1.dll`
   - `libstdc++-6.dll`
   - `libwinpthread-1.dll`
   - `libjpeg-8.dll`
   - `liblcms2-2.dll`
   - `zlib1.dll`
3. If missing, they should be in `C:\msys64\mingw64\bin\` - copy them to your fujimatic directory
4. Copy the complete directory (with all DLLs) to your new laptop again

#### Problem: "VCRUNTIME140.dll was not found" or "MSVCP140.dll was not found"

**Cause:** New computer missing Visual C++ Runtime libraries.

**Solution:**
1. Download [Microsoft Visual C++ Redistributable](https://aka.ms/vs/17/release/vc_redist.x64.exe) (x64 version)
2. Run the installer
3. Restart your computer
4. Try fujimatic again

#### Problem: Windows Defender blocks fujimatic.exe or DLLs

**Cause:** Executables copied from another computer are flagged as "untrusted" by Windows security.

**Solution Option 1 - Unblock files:**
1. Right-click the fujimatic directory → **Properties**
2. Check **"Unblock"** at the bottom (if present) → **Apply**
3. Click **OK**

**Solution Option 2 - Add exclusion:**
1. Open **Windows Security** → **Virus & threat protection**
2. Click **"Manage settings"** under Virus & threat protection settings
3. Scroll to **"Exclusions"** → **"Add or remove exclusions"**
4. Click **"Add an exclusion"** → **"Folder"**
5. Browse to and select your fujimatic directory
6. Click **"Select Folder"**

#### Problem: "fujimatic.exe is not a valid Win32 application"

**Cause:** Trying to run 64-bit fujimatic on 32-bit Windows.

**Solution:**
- Fujimatic **requires 64-bit Windows** (x64)
- Check your Windows version: Settings → System → About → "System type"
- If you have 32-bit Windows, you'll need to upgrade to 64-bit Windows or use a different computer

#### Problem: "Failed to initialize SDK" after copying

**Cause:** `FUJI_SDK_PATH` not set or pointing to wrong location.

**Solution:**
1. Verify environment variable:
   ```cmd
   echo %FUJI_SDK_PATH%
   ```
2. Should output your fujimatic directory path (e.g., `C:\Program Files\fujimatic`)
3. If blank or wrong, go back to Step 2 above
4. **CRITICAL:** Restart Command Prompt after setting environment variables

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

### Problem: Fujimatic exits immediately with no error message

**Cause:** Missing DLL dependencies (usually MinGW runtime DLLs).

**Solution:**
1. Try running from Command Prompt (not PowerShell) to see error messages:
   ```cmd
   cd "C:\Program Files\fujimatic"
   fujimatic.exe
   ```
2. If you see "libgcc_s_seh-1.dll was not found":
   - Your installation is missing MinGW runtime DLLs
   - Download a complete release package with all DLLs included
   - Or see the troubleshooting in the "Quick Start: Migrating" section above

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

### Problem: "VCRUNTIME140.dll was not found" or missing DLL errors

**Causes:**
- Microsoft Visual C++ Runtime libraries not installed
- System missing required DLL dependencies

**Solution:**
1. Download and install [Microsoft Visual C++ Redistributable](https://aka.ms/vs/17/release/vc_redist.x64.exe) (latest x64 version)
2. Restart your computer
3. Try running fujimatic again

### Problem: "Failed to initialize SDK"

**Causes:**
- SDK DLLs missing or wrong version (must be 64-bit)
- `FUJI_SDK_PATH` not set or pointing to wrong directory
- Incompatible SDK version
- Missing Visual C++ Runtime (see above)

**Solution:**
1. Verify all SDK DLLs are in place (see Step 3)
2. Check that FUJI_SDK_PATH is set correctly: `echo %FUJI_SDK_PATH%`
3. Install Visual C++ Redistributable (see above)
4. Try running as Administrator (right-click → "Run as administrator")
5. Check Windows Event Viewer for DLL loading errors

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
│       ├── libgcc_s_seh-1.dll       ← MinGW runtime (required)
│       ├── libgomp-1.dll            ← MinGW runtime (required)
│       ├── libstdc++-6.dll          ← MinGW runtime (required)
│       ├── libwinpthread-1.dll      ← MinGW runtime (required)
│       ├── libjpeg-8.dll            ← MinGW runtime (required)
│       ├── liblcms2-2.dll           ← MinGW runtime (required)
│       ├── zlib1.dll                ← MinGW runtime (required)
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

*Last updated: 2025-01-17*
*For Fujimatic version: 1.0.0+*
