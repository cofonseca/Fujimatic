# 4. Initialization and Finalization

## Function: fm_init

The SDK must be initialized before any other operations can be executed. This function sets up the necessary internal structures and requires the path to the Fujifilm SDK redistributables directory.

### Function Signature (C)
```c
int fm_init(const char* sdk_path);
```

### Parameters
- `sdk_path`: A null-terminated C string pointing to the directory containing the platform-specific SDK files (e.g., `XSDK.DAT`).

### Return Value
- `0`: Success.
- Non-zero: Error code, consult SDK documentation for details.

### Required Environment Configuration
Successful initialization hinges on the correct setting of the `FUJI_SDK_PATH` environment variable prior to application launch, as this path is often passed implicitly or used by the underlying C SDK.

## Function: fm_finalize (Inferred)

A corresponding finalization function is standard practice to clean up resources allocated during `fm_init`.

### Function Signature (Inferred C)
```c
int fm_finalize();
```

### Behavior
- Releases internal SDK resources.
- Should be called before the application exits or before re-initializing the SDK.
