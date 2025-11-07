@echo off
echo Building fm_wrapper.dll ...
cl /LD sdk-c-wrapper\fm_wrapper.c /I sdk-c-wrapper /Fe:libs\windows\fmwrapper.dll
echo Done.
