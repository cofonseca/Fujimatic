# Build Instructions

This document provides detailed instructions for building fujimatic on Windows with MinGW/GCC.

## Prerequisites

### Required Software

1. **Go 1.24.3+** (as specified in go.mod)
   - Download from https://golang.org/dl/
   - Verify installation: `go version`

2. **MSYS2 with MinGW64 GCC**
   - Download from https://www.msys2.org/
   - Install MSYS2 to default location: `C:\msys64`
   - After installation, open MSYS2 terminal and run:
     ```bash
     pacman -S mingw-w64-x86_64-gcc
     ```
   - Verify installation: `C:\msys64\mingw64\bin\gcc.exe --version`

3. **Fujifilm Camera Control SDK**
   - Obtain from Fujifilm (not included in repository)
   - Extract to `sdk/` directory in project root
   - Required structure:
     ```
     sdk/
     ├── HEADERS/           # SDK header files
     └── REDISTRIBUTABLES/  # Platform-specific DLLs/libraries
         └── Windows/
             └── 64bit/
                 ├── XAPI.dll
                 └── FF*.dll (various SDK DLLs)
     ```

### Environment Setup

**CRITICAL**: Before building, you MUST add MinGW to your PATH:

```bash
# For current session (in Git Bash or MSYS2):
export PATH="/c/msys64/mingw64/bin:$PATH"

# To make permanent, add to ~/.bashrc or ~/.bash_profile:
echo 'export PATH="/c/msys64/mingw64/bin:$PATH"' >> ~/.bashrc
```

**Why this is required:**
- CGO invokes GCC during the Go build process
- GCC internally calls `cc1.exe` (the C compiler backend)
- `cc1.exe` requires DLLs from the MinGW bin directory:
  - `libgcc_s_seh-1.dll`
  - `libgmp-10.dll`
  - `zlib1.dll`
  - `libwinpthread-1.dll`
  - `libzstd.dll`
- If MinGW is not in PATH, you'll get cryptic "cgo.exe: exit status 2" errors

**Verify PATH is set correctly:**
```bash
# Should show MinGW bin directory
echo $PATH | grep mingw64

# Should show GCC version without errors
gcc --version

# Test simple C compilation
echo 'int main() { return 0; }' > test.c && gcc test.c -o test.exe && ./test.exe && rm test.c test.exe
```

## Build Process

The build has two stages: C wrapper, then Go application.

### Stage 1: Build C Wrapper

The C wrapper (`fmwrapper.dll`) links against the Fujifilm SDK and provides a simplified C API.

**From Git Bash or MSYS2 terminal:**

```bash
# Ensure PATH is set
export PATH="/c/msys64/mingw64/bin:$PATH"

# Navigate to project root
cd C:/Users/cfonseca/Documents/fujimatic

# Build the wrapper
gcc -shared \
    -o libs/windows/fmwrapper.dll \
    sdk-c-wrapper/fm_wrapper.c \
    -I sdk-c-wrapper \
    -I sdk/HEADERS \
    -L sdk/REDISTRIBUTABLES/Windows/64bit \
    -lXAPI \
    -Wl,--out-implib,libs/windows/libfmwrapper.a \
    -D_WIN32
```

**Expected output:**
- `libs/windows/fmwrapper.dll` - The compiled wrapper DLL
- `libs/windows/libfmwrapper.a` - Import library for linking
- You'll see warnings about 'dllimport' attributes - these are from SDK headers and are **normal/expected**

**Common errors:**

| Error | Cause | Solution |
|-------|-------|----------|
| `gcc: command not found` | GCC not in PATH | Add MinGW to PATH (see above) |
| `XAPI.H: No such file` | SDK headers missing | Check `sdk/HEADERS/` exists |
| `cannot find -lXAPI` | SDK DLL missing | Check `sdk/REDISTRIBUTABLES/Windows/64bit/XAPI.dll` exists |

### Stage 2: Build Go Application

The Go application links against the C wrapper using CGO.

**From Git Bash or MSYS2 terminal:**

```bash
# Ensure PATH is set (CRITICAL!)
export PATH="/c/msys64/mingw64/bin:$PATH"

# Navigate to project root
cd C:/Users/cfonseca/Documents/fujimatic

# Build the application
go build -o bin/fujimatic.exe ./cmd/fujimatic

# CRITICAL: Copy wrapper DLL to bin directory
cp libs/windows/fmwrapper.dll bin/
```

**Why the DLL copy is required:**
- Windows searches for DLLs in the same directory as the .exe FIRST
- CGO's `-L` flag only affects build-time linking, not runtime loading
- Without this copy, you'll get exit code 127 (DLL not found)

**Expected output:**
- `bin/fujimatic.exe` - The compiled binary (approximately 5-6 MB)
- `bin/fmwrapper.dll` - Copy of the C wrapper (required at runtime)

**Verify the build:**
```bash
# Check binary size (should be ~5-6 MB)
ls -lh bin/fujimatic.exe

# Check DLL is present
ls -lh bin/fmwrapper.dll

# Check DLL dependencies
ldd bin/fujimatic.exe

# Test execution
./bin/fujimatic.exe --help
```

## Complete Build Script

For convenience, you can use this one-liner to rebuild everything:

```bash
# Set PATH and build both stages
export PATH="/c/msys64/mingw64/bin:$PATH" && \
cd C:/Users/cfonseca/Documents/fujimatic && \
gcc -shared -o libs/windows/fmwrapper.dll sdk-c-wrapper/fm_wrapper.c \
    -I sdk-c-wrapper -I sdk/HEADERS \
    -L sdk/REDISTRIBUTABLES/Windows/64bit -lXAPI \
    -Wl,--out-implib,libs/windows/libfmwrapper.a -D_WIN32 && \
go build -o bin/fujimatic.exe ./cmd/fujimatic && \
cp libs/windows/fmwrapper.dll bin/ && \
echo "Build complete! Test with: ./bin/fujimatic.exe --help"
```

## Troubleshooting

### Build fails with "cgo.exe: exit status 2"

**Diagnosis:**
```bash
# Check if GCC works
gcc --version

# Check if cc1.exe can find its DLLs
ldd /c/msys64/mingw64/lib/gcc/x86_64-w64-mingw32/*/cc1.exe

# If you see "not found" for any DLLs, MinGW is not in PATH
```

**Solution:**
```bash
export PATH="/c/msys64/mingw64/bin:$PATH"
```

### Executable exits immediately (exit code 127)

**Diagnosis:**
```bash
# Check what DLL is missing
ldd bin/fujimatic.exe

# If fmwrapper.dll shows "not found", it's missing from bin/
```

**Solution:**
```bash
cp libs/windows/fmwrapper.dll bin/
```

### Build succeeds but crashes at runtime

**Diagnosis:**
```bash
# Check if all SDK DLLs are accessible
ls sdk/REDISTRIBUTABLES/Windows/64bit/

# Verify FUJI_SDK_PATH is set
echo $FUJI_SDK_PATH
```

**Solution:**
```bash
# Set SDK path (required at runtime, not build time)
export FUJI_SDK_PATH="C:/Users/cfonseca/Documents/fujimatic/sdk/REDISTRIBUTABLES/Windows/64bit"

# OR make it permanent
setx FUJI_SDK_PATH "C:\Users\cfonseca\Documents\fujimatic\sdk\REDISTRIBUTABLES\Windows\64bit"
```

### "undefined reference to fm_*" errors during linking

**Cause:** C wrapper wasn't built or wasn't built successfully.

**Solution:**
```bash
# Rebuild C wrapper first
gcc -shared -o libs/windows/fmwrapper.dll sdk-c-wrapper/fm_wrapper.c \
    -I sdk-c-wrapper -I sdk/HEADERS \
    -L sdk/REDISTRIBUTABLES/Windows/64bit -lXAPI \
    -Wl,--out-implib,libs/windows/libfmwrapper.a -D_WIN32

# Verify output exists
ls -lh libs/windows/fmwrapper.dll
```

## Testing the Build

### Quick Test (No Hardware)
```bash
# Test with fake camera (no SDK/hardware required)
./bin/fujimatic.exe --fake-camera
```

### Full Test (With Camera)
```bash
# Ensure SDK path is set
export FUJI_SDK_PATH="C:/Users/cfonseca/Documents/fujimatic/sdk/REDISTRIBUTABLES/Windows/64bit"

# Run with real hardware
./bin/fujimatic.exe

# Or with verbose logging
./bin/fujimatic.exe --verbose
```

## Build Flags Reference

### C Wrapper Build Flags

| Flag | Purpose |
|------|---------|
| `-shared` | Build as shared library (DLL) |
| `-o libs/windows/fmwrapper.dll` | Output file path |
| `-I sdk-c-wrapper` | Include C wrapper headers |
| `-I sdk/HEADERS` | Include SDK headers |
| `-L sdk/REDISTRIBUTABLES/Windows/64bit` | SDK library path |
| `-lXAPI` | Link against XAPI.dll |
| `-Wl,--out-implib,libs/windows/libfmwrapper.a` | Generate import library |
| `-D_WIN32` | Define Windows platform macro |

### Go Build Flags

| Flag | Purpose |
|------|---------|
| `-o bin/fujimatic.exe` | Output file path |
| `./cmd/fujimatic` | Package to build |
| `-x` (optional) | Show verbose build output (for debugging) |
| `-v` (optional) | Show package names being built |

## CGO Configuration

CGO compiler and linker flags are defined in `pkg/sdk/sdk.go`:

```go
/*
#cgo CFLAGS: -I../../sdk-c-wrapper
#cgo LDFLAGS: -L../../libs/windows -lfmwrapper
*/
```

**Important:**
- `CFLAGS` tells CGO where to find C headers at build time
- `LDFLAGS` tells CGO where to find libraries for linking at build time
- Neither flag affects runtime DLL loading (hence the need to copy DLLs to bin/)

## Clean Build

To start fresh:

```bash
# Remove build artifacts
rm -f libs/windows/fmwrapper.dll
rm -f libs/windows/libfmwrapper.a
rm -f bin/fujimatic.exe
rm -f bin/fmwrapper.dll

# Rebuild from scratch
export PATH="/c/msys64/mingw64/bin:$PATH"
# ... then follow build steps above
```

## Development Workflow

When making changes:

1. **C wrapper changes** (`sdk-c-wrapper/fm_wrapper.c` or `.h`):
   ```bash
   # Rebuild wrapper only
   gcc -shared -o libs/windows/fmwrapper.dll sdk-c-wrapper/fm_wrapper.c \
       -I sdk-c-wrapper -I sdk/HEADERS \
       -L sdk/REDISTRIBUTABLES/Windows/64bit -lXAPI \
       -Wl,--out-implib,libs/windows/libfmwrapper.a -D_WIN32

   # Rebuild Go app
   go build -o bin/fujimatic.exe ./cmd/fujimatic

   # Copy DLL
   cp libs/windows/fmwrapper.dll bin/
   ```

2. **Go code changes** (`cmd/`, `pkg/`):
   ```bash
   # Rebuild Go app only
   go build -o bin/fujimatic.exe ./cmd/fujimatic
   ```

3. **SDK bindings changes** (`pkg/sdk/`):
   ```bash
   # Rebuild Go app (CGO will recompile bindings)
   go build -o bin/fujimatic.exe ./cmd/fujimatic
   ```

## Cross-Platform Notes

These instructions are specific to **Windows with MinGW/GCC**. For other platforms:

- **Linux (x86_64)**: Use `scripts/build_wrapper.sh` and system GCC
- **Raspberry Pi (ARM64)**: Cross-compile or build natively, see CLAUDE.md
- **Build tags**: RealCamera requires `cgo` build tag (enabled by default when CGO_ENABLED=1)

## Additional Resources

- **Full project documentation**: See `README.md`
- **Development guidelines**: See `CLAUDE.md`
- **Story tracking**: See `STORIES.md`
- **Hardware testing**: See `HARDWARE_TESTING_PLAN.md`
