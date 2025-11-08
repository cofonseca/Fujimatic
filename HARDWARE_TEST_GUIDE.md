# Hardware Testing Guide - Story C-1

## Summary of Completed Work

✅ **SDK Integration Complete**
- All 7 C wrapper functions implemented with real Fujifilm SDK calls
- fmwrapper.dll successfully built (242KB) in MSYS2 MinGW terminal
- Battery conversion logic: SDK codes → 0-100 percentage
- Image download workflow fully implemented
- Error handling with descriptive messages

## Prerequisites

### 1. Hardware Setup
- [ ] X-T3 camera fully charged or on AC power
- [ ] USB cable (USB 3.0/3.1 recommended)
- [ ] Camera powered ON
- [ ] Camera in correct USB mode (check manual - may need MTP/PTP/PC Auto Save mode)

### 2. Environment Setup

**Set SDK Path (in PowerShell):**
```powershell
setx FUJI_SDK_PATH "C:\Users\cfonseca\Documents\fujimatic\sdk\REDISTRIBUTABLES\Windows\64bit"
```

**Note:** You'll need to close and reopen terminal for environment variable to take effect.

## Building the Application

The Go application must be built in the **MSYS2 MinGW 64-bit** terminal (same as the DLL):

```bash
# Open MSYS2 MinGW 64-bit terminal
cd /c/Users/cfonseca/Documents/fujimatic

# Build the application
go build -o bin/fujimatic.exe ./cmd/fujimatic

# Verify build succeeded
ls -lh bin/fujimatic.exe
```

**Why MSYS2 terminal?** The cgo build requires the same GCC environment used to compile fmwrapper.dll.

## Testing Sequence

### Test 1: SDK Initialization
```bash
# In MSYS2 MinGW terminal:
./bin/fujimatic.exe
```

**Expected output:**
```
Initializing Fujifilm SDK from: C:\Users\cfonseca\Documents\fujimatic\sdk\REDISTRIBUTABLES\Windows\64bit
Fujifilm SDK initialized successfully
SDK initialized successfully
Fujimatic Camera Control CLI
Type 'help' for available commands

>
```

**If it fails:**
- Check FUJI_SDK_PATH is set: `echo $FUJI_SDK_PATH`
- Verify XAPI.dll exists in that path
- Check for error messages about missing DLLs

### Test 2: Camera Connection
```bash
> connect
```

**Expected output (camera connected):**
```
Connecting to camera...
Connected to camera: X-T3 (S/N: 12345678)
Camera connected successfully
```

**Expected output (camera NOT connected):**
```
Connecting to camera...
Error: failed to connect: camera connection failed with code: -4
(fm_connect: No cameras detected)
```

**Troubleshooting:**
- Ensure camera is powered ON
- Check USB cable is connected
- Try different USB ports
- Check camera USB mode setting in menu
- Camera LCD should show "PC connection" or similar when connected

### Test 3: Status Check
```bash
> status
```

**Expected output:**
```
Status:

  Camera: Connected
  Battery: 85%

  Session: None
```

### Test 4: Battery Reading
```bash
> battery
```

**Expected output:**
```
Battery: 85%
```

**Verification:**
- Compare with camera LCD battery indicator
- Should be within ±5% accuracy
- If shows 50%, check stderr for unknown battery code

### Test 5: Disconnect
```bash
> disconnect
```

**Expected output:**
```
Disconnecting from camera...
Camera disconnected successfully
```

**Verification:**
- Camera LCD should return to normal operation
- No error messages
- No hanging or crashes

### Test 6: Multiple Connect/Disconnect Cycles
Run this sequence 3-5 times to check for memory leaks:
```bash
> connect
> battery
> disconnect
> connect
> battery
> disconnect
```

**Watch for:**
- Consistent battery readings
- No increase in memory usage (check Task Manager)
- No error messages about already connected/not connected
- Clean disconnects every time

### Test 7: Exit
```bash
> exit
```

**Expected output:**
```
Disconnecting camera...
Camera disconnected successfully
Exiting...
```

## Success Criteria Checklist

- [ ] SDK initializes without errors
- [ ] Camera detected via USB enumeration
- [ ] Connection establishes successfully
- [ ] Battery percentage matches camera LCD (±5%)
- [ ] Status command shows correct camera state
- [ ] Disconnect releases camera cleanly
- [ ] Multiple connect/disconnect cycles work without issues
- [ ] No memory leaks observed
- [ ] No crashes or hangs
- [ ] Error messages are helpful and descriptive

## Known Limitations (MVP Scope)

- Only connects to first detected camera (multi-camera not supported)
- Shutter speed and capture not tested in C-1 (deferred to C-2)
- Image download not tested in C-1 (deferred to C-2)
- Battery reading may return approximate values based on SDK codes

## Next Steps After C-1 Completion

Once all tests pass, move to **Story C-2: Full Capture & Download**:
- Test SetShutter with various exposure times
- Test Capture trigger
- Test DownloadLast with .RAF file creation
- Verify complete synchronous capture+download workflow

## Common Issues

### Issue: "No cameras detected"
**Solutions:**
- Power on camera
- Check USB cable connection
- Try different USB port (preferably USB 3.0)
- Check camera USB mode setting (may need to enable PC connection mode)
- Restart camera

### Issue: "Failed to load XAPI.dll"
**Solutions:**
- Verify FUJI_SDK_PATH environment variable is set
- Check XAPI.dll exists in sdk/REDISTRIBUTABLES/Windows/64bit/
- Ensure all SDK DLL dependencies are present (FF*API.dll files)

### Issue: "Battery returns 50%"
**Solutions:**
- This means unknown battery code from SDK
- Check stderr for actual SDK battery code value
- May need to add code to battery conversion function

### Issue: Connection hangs
**Solutions:**
- Camera may be in wrong mode
- Try disconnecting and reconnecting USB
- Power cycle camera
- Check SDK manual for required camera mode

### Issue: cgo build fails
**Solutions:**
- Ensure building in MSYS2 MinGW 64-bit terminal
- Check GCC is available: `gcc --version`
- Verify CGO_ENABLED=1: `go env CGO_ENABLED`
- Make sure MSYS2 MinGW bin directory is in PATH

## Documentation Updates After Testing

Once testing is complete, update:
- [ ] C1_PROGRESS.md - Mark hardware test criteria as complete
- [ ] STORIES.md - Mark C-1 as ✅ COMPLETED
- [ ] Note any quirks or SDK-specific behaviors discovered
- [ ] Document actual battery reading accuracy
- [ ] Record any camera mode requirements
