# Fujifilm SDK Overview

**SDK Version:** 1.33.0.0
**Purpose:** Control FUJIFILM GFX & X Series cameras from computers via tethered shooting
**Language:** C language interfaces

## Key Capabilities

- Adjust camera shooting conditions
- Operate camera controls (shutter-release, AF lock, etc.)
- Transfer captured images from camera to computer
- **Does NOT** convert RAW files to other formats

## Platform Support

### Windows
- **OS:** Windows 7/8.1/10/11 (x86/x64)
- **CPU:** Intel CPUs
- **Interface:** USB (direct) or TCP/IP (Wi-Fi for supported models)
- **Libraries:** 32-bit and 64-bit
- **Dev Environment:** Microsoft Visual Studio 2017 (Visual C++)
- **Max Cameras:** 4 simultaneous connections
- **Main Library:** `XAPI.dll`
- **Required DLLs:** `FTLPTP.dll`, `FF0001API.dll`, and others (must be in same folder)

**Loading Pattern:**
```c
// Load library
LoadLibrary("XAPI.dll");
// Get function pointers
GetProcAddress(...);
// When done
FreeLibrary();
```

### macOS
- **OS:** macOS 10.12 through 15 (Sequoia)
- **Note:** macOS 10.15.2 NOT supported
- **Libraries:** 64-bit Universal 2 (bundle and dylib)
- **Dev Environment:** Xcode
- **Main Library:** `XAPI.bundle`
- **Required Libraries:** Place in "Resources" folder
- **Security:** Enable Hardened Runtime, check "Disable Library Validation"

### Linux
- **OS:** Ubuntu 22.04.5 LTS 64bit, RaspberryPi OS 32/64bit
- **Libraries:** 32-bit (ARMv7) / 64-bit (ARMv8)
- **Dependency:** libusb-1.0.27
- **Main Library:** `XAPI.so`
- **Loading:** Use `dlopen()`, `dlsym()`, `dlclose()`
- **Path:** Set `LD_LIBRARY_PATH` if not using absolute paths

### Android
- **OS:** Android 11-15
- **Libraries:** 64-bit
- **Dev Environment:** Android Studio
- **Max Cameras:** 1 simultaneous connection
- **Integration:** JNI wrapper pattern

## Supported Cameras

### X-T3 Support ✅
**FUJIFILM X-T3** is fully supported

### Other Supported Models
- X-T4, X-T5
- X-Pro3
- X-S10 (firmware 2.00+), X-S20
- X-H2S, X-H2
- X-M5
- GFX 50S, GFX 50R, GFX 50S II
- GFX 100, GFX 100S, GFX 100S II, GFX 100 II, GFX 100RF

**Important:** Always use latest camera firmware from https://fujifilm-x.com/support/download/firmware/cameras/

## Connection Methods

### USB Connection (All Models)

**Camera Setup:**
1. Power on camera
2. Connect via USB cable
3. Set connection mode to one of:
   - PC SHOOT AUTO
   - USB AUTO
   - USB TETHER SHOOTING AUTO

**For X-H2S specifically:**
- Navigate: NETWORK/USB SETTING MENU > SELECT CONNECTION SETTING
- Choose: "USB TETHER SHOOTING AUTO"

### Wi-Fi Connection (Most Models)

**Not supported:** GFX 50S firmware 1.00 or 1.01

**Camera Setup Steps:**
1. Configure IP address (AUTO or MANUAL)
2. Specify access point (SIMPLE SETUP or MANUAL)
3. Choose WIRELESS FIXED or WIRELESS TETHER SHOOTING FIXED
4. Note camera's IP address for application

**Camera Status LED Indicators:**
- **Blinking red:** Searching for access point
- **Blinking orange:** Awaiting tethered software connection
- **Blinking green:** Ready for tethered shooting
- **Green:** Subject in focus
- **Flashing green:** Focus/exposure warning

## Critical Limitations

⚠️ **THREAD SAFETY REQUIREMENTS:**

The SDK **PROHIBITS:**
- Multiple simultaneous calls from single process (use mutex/CriticalSection)
- Multiple simultaneous calls from multiple processes

**Must implement thread-safe locking for all SDK calls!**

## Redistributable Files (Windows 64-bit)

**Location:** `REDISTRIBUTABLES\WINDOWS\64bit\`

**Core Libraries:**
- `XAPI.dll` (1.16.0.3) - Main SDK library
- `FTLPTP.dll` (3.3.3.0) - USB protocol
- `FTLPTPIP.dll` (1.3.2.3) - Network protocol
- `XSDK.dat` - SDK data file

**Camera-Specific APIs:**
- `FF0000API.dll` through `FF0021API.dll` (22 DLLs)
- Each camera model uses specific FF####API.dll files

**All DLLs must be in same directory as XAPI.dll**

## Quick Start Summary for X-T3 + Windows

1. **Camera Setup:**
   - Power on X-T3
   - USB cable to computer
   - Set: USB AUTO or USB TETHER SHOOTING AUTO

2. **SDK Files:**
   - Place all DLLs in same folder
   - Main: `XAPI.dll`
   - Supporting: `FTLPTP.dll`, `FF0001API.dll`, etc.

3. **Code:**
   - Load: `LoadLibrary("XAPI.dll")`
   - Get functions: `GetProcAddress()`
   - Call SDK functions with thread safety
   - Unload: `FreeLibrary()`

4. **Thread Safety:**
   - Use mutex/CriticalSection for all SDK calls
   - NO simultaneous calls allowed

## Next Steps

- Review API Overview for function categories
- Check Session Management for connection workflow
- Study Basic Functions for init/connect/disconnect
- Review Release Control for shutter operations
- Check Image Acquisition for download functions
