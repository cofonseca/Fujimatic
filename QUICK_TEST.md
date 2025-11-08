# Quick Hardware Test - X-T3

## Issue Resolved
✅ Missing DLL dependencies fixed - all SDK DLLs copied to bin/

## Test with Real Camera

**Prerequisites:**
- X-T3 camera powered ON
- USB cable connected
- FUJI_SDK_PATH environment variable set

**Run the test (in MSYS2 MinGW terminal):**
```bash
cd /c/Users/cfonseca/Documents/fujimatic/bin
./fujimatic.exe
```

**Expected startup:**
```
Initializing Fujifilm SDK from: C:\Users\cfonseca\Documents\fujimatic\sdk\REDISTRIBUTABLES\Windows\64bit
Fujifilm SDK initialized successfully
SDK initialized successfully
Fujimatic Camera Control CLI
Type 'help' for available commands

>
```

**Test sequence:**
```
> connect
> status
> battery
> disconnect
> exit
```

## Expected Results

### connect
**Success:**
```
Connecting to camera...
Connected to camera: X-T3 (S/N: XXXXX)
Camera connected successfully
```

**Failure (camera not detected):**
```
Connecting to camera...
Error: failed to connect: camera connection failed with code: -4
```

### status
```
Status:

  Camera: Connected
  Battery: XX%

  Session: None
```

### battery
```
Battery: XX%
```
(Should match camera LCD ±5%)

### disconnect
```
Disconnecting from camera...
Camera disconnected successfully
```

## If Camera Not Detected

1. Check camera is powered ON
2. Check USB cable is firmly connected
3. Try different USB port
4. Check camera USB mode setting (may need to enable PC mode in menu)
5. Restart camera
6. Check camera LCD for "PC connection" indicator

## Report Back

Please report:
- [ ] Did SDK initialize successfully?
- [ ] Was camera detected on connect?
- [ ] What battery percentage was shown?
- [ ] Did battery match camera LCD?
- [ ] Did disconnect work cleanly?
- [ ] Any error messages?
