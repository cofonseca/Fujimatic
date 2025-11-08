# Real Camera Hardware Testing Plan

**Story:** C-2: Full Capture & Download with Real Camera
**Camera:** Fujifilm X-T3
**Date:** 2025-11-08
**Tester:** User

## Prerequisites Checklist

**Hardware Setup:**
- [ ] X-T3 camera powered on and in "PC" mode
- [ ] USB-C cable connected between camera and computer
- [ ] SD card in camera with sufficient space (>100MB free)
- [ ] Camera battery level >20% (for testing)

**Software Setup:**
- [ ] Fujifilm SDK redistributables installed
- [ ] `FUJI_SDK_PATH` environment variable set correctly
- [ ] `fujimatic.exe` built and ready
- [ ] Test output directory prepared (e.g., `C:\Users\cfonseca\Pictures\fujimatic_test`)

**Expected SDK Path:**
```
FUJI_SDK_PATH=C:\Users\cfonseca\Documents\fujimatic\sdk\REDISTRIBUTABLES
```

## Test Procedure

### Phase 1: SDK Initialization Test

**Test 1.1: SDK Initialization**
```cmd
# Run fujimatic (should auto-initialize SDK)
fujimatic.exe
```

**Expected Results:**
- ✅ SDK initialization message appears
- ✅ No "SDK initialization failed" errors
- ✅ CLI shell starts successfully

---

### Phase 2: Camera Connection Test

**Test 2.1: Connect to Camera**
```
> connect
```

**Expected Results:**
- ✅ "Connecting to camera..." message
- ✅ "Camera connected successfully" message
- ✅ No error messages

**Test 2.2: Check Connection Status**
```
> status
```

**Expected Results:**
- ✅ Shows "Camera: Connected"
- ✅ Shows actual battery percentage (not 0% or 100%)
- ✅ No connection errors

**Test 2.3: Read Battery Level**
```
> battery
```

**Expected Results:**
- ✅ Shows realistic battery percentage (e.g., "Battery: 85%")
- ✅ Should match camera display

---

### Phase 3: Settings Read/Write Test

**Test 3.0: Automatic Manual Mode Setup**
```
> connect
```

**Expected Results:**
- ✅ Camera connects successfully
- ✅ "Setting Manual exposure mode for tethered control..." message
- ✅ "✅ Manual exposure mode set - full shutter/ISO control available" message
- ✅ Shutter and ISO control ready immediately

**Test 3.1: Shutter Speed Diagnostic**
```
> shutters
```

**Expected Results:**
- ✅ Lists all available shutter speeds from your X-T3
- ✅ Shows microseconds and seconds for each speed
- ✅ Critical for identifying supported values

**Test 3.2: Read Current ISO**
```
> get iso
```

**Expected Results:**
- ✅ Shows current ISO value (e.g., "Current ISO: 200")
- ✅ Reasonable value (100-12800)

**Test 3.3: Change ISO Setting**
```
> set iso 400
> get iso
```

**Expected Results:**
- ✅ "ISO set to: 400"
- ✅ "Current ISO: 400" (verify change took effect)
- ✅ Camera screen should show ISO 400

**Test 3.4: ISO Range Validation**
```
> set iso 50
```
**Expected Results:**
- ❌ Error: "ISO value out of range [100, 12800]"

**Test 3.5: Shutter Speed Read/Write**
```
> get shutter
> set shutter 0.5s
> get shutter
```

**Expected Results:**
- ✅ Shows current shutter in seconds and microseconds
- ✅ "Shutter set to: 0.500000 seconds (500000 microseconds)"
- ✅ "Current shutter: 0.500000 seconds (500000 microseconds)"
- ✅ Camera screen should show 1/2s

**Test 3.5: Shutter Range Validation**
```
> set shutter 0.0001s
```
**Expected Results:**
- ❌ Error: "shutter too fast (minimum: 1/8000 second = 0.000125 seconds)"

**Test 3.6: Shutter Format Testing**
```
> set shutter 2s
> set shutter 0.125s
> get shutter
```
**Expected Results:**
- ✅ "Shutter set to: 2.000000 seconds (2000000 microseconds)"
- ✅ "Shutter set to: 0.125000 seconds (125000 microseconds)"
- ✅ "Current shutter: 0.125000 seconds (125000 microseconds)"

**Test 3.7: Shutter Speed Diagnostic**
```
> shutters
```
**Expected Results:**
- ✅ Lists all available shutter speeds from your X-T3
- ✅ Shows both microseconds and seconds for each speed
- ✅ Helps identify which values are actually supported

**Fixed: Manual Mode is Set Automatically**
The camera is automatically set to Manual exposure mode when connecting. No manual intervention required - shutter and ISO control are available immediately after connection.

---

### Phase 4: Capture & Download Test

**Test 4.1: Single Capture**
```
> session start test_capture C:\Users\cfonseca\Pictures\fujimatic_test
> capture
```

**Expected Results:**
- ✅ Session created successfully
- ✅ "Capturing test_capture_0001.RAF..."
- ✅ "Captured and saved: [filepath]"
- ✅ No error messages
- ✅ .RAF file created in output directory

**Test 4.2: File Verification**
**Verify manually:**
- [ ] Check file exists: `C:\Users\cfonseca\Pictures\fujimatic_test\test_capture_0001.RAF`
- [ ] File size is reasonable (15-40MB for X-T3 RAW)
- [ ] File opens in image viewer without errors

**Test 4.3: Multiple Capture Sequence**
```
> capture
> capture
> capture
```

**Expected Results:**
- ✅ Sequential naming: 0001, 0002, 0003
- ✅ Each capture completes successfully
- ✅ Files saved with increasing sequence numbers

**Test 4.4: Battery Drain Check**
```
> battery
```

**Expected Results:**
- ✅ Battery level decreased by 1-2% per capture
- ✅ Still >20% (if started >20%)

---

### Phase 5: Error Handling Test

**Test 5.1: Disconnect During Operation**
```
# Physically disconnect USB cable
> capture
```

**Expected Results:**
- ❌ Error: "failed to capture" or "camera disconnected"
- [ ] Verify camera reconnection works

**Test 5.2: Operations Without Connection**
```
> disconnect
> get iso
```

**Expected Results:**
- ❌ Error: "camera not connected - use 'connect' first"

**Test 5.3: Reconnection Test**
```
> connect
> status
```

**Expected Results:**
- ✅ Reconnection works
- ✅ Camera status shows connected
- ✅ Previous settings may or may not persist

---

### Phase 6: Integration Test

**Test 6.1: Complete Workflow**
```
> connect
> get iso
> set iso 800
> set shutter 0.125s
> get iso
> get shutter
> session start integration_test C:\Users\cfonseca\Pictures\fujimatic_test
> capture
> capture
> disconnect
```

**Expected Results:**
- ✅ All settings read/written correctly
- ✅ 2 RAF files created
- ✅ Sequential naming works
- ✅ Clean disconnect

---

## Success Criteria

**For Story C-2 Completion:**

✅ **Core Functions:**
- [ ] SDK initializes without errors
- [ ] Camera connects via USB
- [ ] Battery level reads correctly
- [ ] ISO can be read and changed
- [ ] **Manual exposure mode set automatically** (Fixed: No user exposure mode selection needed)
- [ ] **Shutter can be read and changed** (Fixed: CLI seconds ↔ SDK microseconds + automatic Manual mode)
- [ ] **Shutter parsing works** (Fixed: "0.125s" format supported)
- [ ] Capture triggers actual shutter
- [ ] RAF file downloads successfully
- [ ] File size is 15-40MB (valid RAW)
- [ ] Session sequence numbering works

✅ **Error Handling:**
- [ ] Helpful error messages for common issues
- [ ] Graceful handling of disconnection
- [ ] Validation for out-of-range values

✅ **CLI Integration:**
- [ ] All new commands work: `get iso`, `set iso`, `get shutter`, `set shutter`
- [ ] Complete workflow: connect → set → capture → download
- [ ] User-friendly messages and feedback

## Troubleshooting

**If connection fails:**
1. Check camera is in "PC" mode
2. Verify USB cable is data cable (not charge-only)
3. Try different USB port
4. Check camera battery level
5. Verify `FUJI_SDK_PATH` environment variable

**If capture fails:**
1. Check SD card has space
2. Check battery level
3. Try shorter shutter speeds
4. Check camera focus mode
5. Try single-shot vs continuous mode

**If SDK fails to initialize:**
1. Verify `FUJI_SDK_PATH` points to redistributables directory
2. Check DLLs are in correct location
3. Try running as administrator
4. Verify Fujifilm Camera Control SDK is installed

## Test Results Log

| Test | Pass/Fail | Notes |
|------|-----------|-------|
| 1.1 SDK Init | | |
| 2.1 Connect (with Auto Manual) | | |
| 2.2 Status | | |
| 2.3 Battery | | |
| 3.0 Shutter List | | |
| 3.1 Get ISO | | |
| 3.2 Set ISO | | |
| 3.3 ISO Range | | |
| 3.4 Shutter Get/Set | | |
| 3.5 Shutter Range | | |
| 3.6 Shutter Format | | |
| 4.1 Single Capture | | |
| 4.2 File Verify | | |
| 4.3 Multi Capture | | |
| 4.4 Battery Drain | | |
| 5.1 Disconnect | | |
| 5.2 No Connection | | |
| 5.3 Reconnect | | |
| 6.1 Complete Workflow | | |

**Overall Result:** ✅ PASS / ❌ FAIL