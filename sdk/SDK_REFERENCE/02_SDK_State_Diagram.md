# Fujifilm SDK State Diagram

## Overview
The SDK operates through a well-defined state machine with 4 main states. Understanding this flow is **critical** for proper SDK usage.

## The Four States

| State | Name | Description | How to Enter |
|-------|------|-------------|--------------|
| **S0** | Loaded | SDK library is loaded into memory | Call `LoadLibrary()` (Windows) or `dlopen()` (Linux) |
| **S1** | Initialized | SDK initialized and ready to use | Call `XSDK_Init()` from S0 |
| **S2** | Detected | Available cameras listed in SDK's internal table | Call `XSDK_Detect()` from S1 |
| **S3** | Session | A specific camera can be controlled | Call `XSDK_OpenEx()` from S2 |

## State Transitions

```
S0 (Loaded)
    ↓ XSDK_Init()
S1 (Initialized)
    ↓ XSDK_Detect()
S2 (Detected)
    ↓ XSDK_OpenEx()
S3 (Session) ← Camera can now be controlled
    ↓ XSDK_Close()
S2 (Detected)
    ↓ XSDK_Exit()
S1 (Initialized)
    ↓ XSDK_Exit()
S0 (Loaded)
```

## Multiple Camera Support

From **S2 (Detected)** state, you can open sessions to multiple cameras simultaneously:

```
S2 (Detected)
    ├─ XSDK_OpenEx(Camera1) → S3 Session for Camera1
    ├─ XSDK_OpenEx(Camera2) → S3 Session for Camera2
    ├─ XSDK_OpenEx(Camera3) → S3 Session for Camera3
    └─ XSDK_OpenEx(Camera4) → S3 Session for Camera4
```

**Maximum:** 4 simultaneous camera sessions

## Session Invalidation

A camera session (S3) can become **invalid** if:
- Camera powers off
- Camera is disconnected (USB unplugged)
- User calls `XSDK_Close()`

When session becomes invalid, it returns to session pool but camera is no longer controllable.

## Typical Workflow

### Minimal Connect/Disconnect Flow
```
1. LoadLibrary("XAPI.dll")          → S0 Loaded
2. XSDK_Init()                      → S1 Initialized
3. XSDK_Detect()                    → S2 Detected
4. XSDK_OpenEx(camera_index)        → S3 Session
   [Control camera operations here]
5. XSDK_Close()                     → Back to S2
6. XSDK_Exit()                      → S1
7. FreeLibrary()                    → S0
```

### Production Application Flow
```
Application Start:
1. LoadLibrary("XAPI.dll")          → S0
2. XSDK_Init()                      → S1

Main Loop:
3. XSDK_Detect()                    → S2 (refresh camera list)
4. XSDK_OpenEx(camera)              → S3 (open connection)
5. [Capture operations]
6. XSDK_Close()                     → S2 (close when done)

Application Shutdown:
7. XSDK_Exit()                      → S1
8. FreeLibrary()                    → S0
```

## Key Functions by State

### S0 → S1: Initialization
- **Function:** `XSDK_Init()`
- **Purpose:** Initialize SDK, prepare for camera detection
- **Call once:** At application startup

### S1 → S2: Detection
- **Function:** `XSDK_Detect()`
- **Purpose:** Scan for connected cameras, build internal list
- **Can call multiple times:** To refresh available camera list

### S2 → S3: Open Session
- **Function:** `XSDK_OpenEx(camera_handle)`
- **Purpose:** Establish session with specific camera
- **Returns:** Session handle for camera operations
- **Can call multiple times:** Up to 4 cameras simultaneously

### S3 → S2: Close Session
- **Function:** `XSDK_Close(session_handle)`
- **Purpose:** Release camera session cleanly
- **Call when:** Done with camera or before app shutdown

### S1 → S0: Exit
- **Function:** `XSDK_Exit()`
- **Purpose:** Cleanup SDK resources
- **Call once:** At application shutdown

## Common Mistakes to Avoid

❌ **Don't skip states:**
```c
// BAD - can't go directly from S0 to S3
LoadLibrary();
XSDK_OpenEx();  // WILL FAIL - SDK not initialized!
```

❌ **Don't forget to detect:**
```c
// BAD - can't open without detecting first
XSDK_Init();
XSDK_OpenEx();  // WILL FAIL - no cameras detected!
```

❌ **Don't leave sessions open:**
```c
// BAD - leaks resources
XSDK_OpenEx();
// ... operations ...
XSDK_Exit();  // WILL FAIL - session still open!
```

✅ **Correct pattern:**
```c
LoadLibrary();
XSDK_Init();
XSDK_Detect();
handle = XSDK_OpenEx();
// ... operations ...
XSDK_Close(handle);
XSDK_Exit();
FreeLibrary();
```

## State Requirements for Camera Operations

**All camera control functions require S3 (Session) state:**
- Capture image
- Get battery level
- Set shutter speed
- Download images
- Adjust settings

**Attempting operations in wrong state will fail!**

## Recovery from Session Loss

If camera disconnects or powers off during S3:

```c
// Session becomes invalid
// Option 1: Detect and reconnect
XSDK_Detect();           // Refresh camera list
handle = XSDK_OpenEx();  // Reconnect if camera is back

// Option 2: Close and cleanup
XSDK_Close(old_handle);  // Clean up invalid session
XSDK_Detect();           // Re-scan for cameras
```

## Summary for fujimatic Implementation

For our X-T3 integration, we need to:

1. **Startup (main.go):**
   - Load XAPI.dll
   - Call XSDK_Init() → S1

2. **Connect Command:**
   - Call XSDK_Detect() → S2
   - Call XSDK_OpenEx() → S3
   - Store session handle

3. **Operations (capture, battery, etc.):**
   - Use session handle from S3
   - Must be in S3 state

4. **Disconnect Command:**
   - Call XSDK_Close() → S2

5. **Shutdown:**
   - Call XSDK_Exit() → S1
   - Unload library → S0

**Next:** Review API Overview to see actual function signatures and parameters.
