@echo off
echo Building fmwrapper.dll with GCC...

REM Set SDK path
set SDK_REDIST=sdk\REDISTRIBUTABLES\Windows\64bit

REM Check if SDK DLL exists
if not exist "%SDK_REDIST%\XAPI.dll" (
    echo Error: XAPI.dll not found in %SDK_REDIST%
    echo Please ensure the Fujifilm SDK is in the sdk directory
    exit /b 1
)

REM Create output directory if it doesn't exist
if not exist libs\windows mkdir libs\windows

REM Compile with MinGW-w64 GCC
gcc -shared ^
    -o libs\windows\fmwrapper.dll ^
    sdk-c-wrapper\fm_wrapper.c ^
    -I sdk-c-wrapper ^
    -I sdk\HEADERS ^
    -L %SDK_REDIST% ^
    -lXAPI ^
    -Wl,--out-implib,libs\windows\libfmwrapper.a ^
    -D_WIN32

if %ERRORLEVEL% NEQ 0 (
    echo Build failed!
    exit /b %ERRORLEVEL%
)

echo.
echo Build successful!
echo Output: libs\windows\fmwrapper.dll
echo Import library: libs\windows\libfmwrapper.a
echo.
echo Note: Make sure to set FUJI_SDK_PATH environment variable to:
echo %CD%\sdk\REDISTRIBUTABLES\Windows\64bit
