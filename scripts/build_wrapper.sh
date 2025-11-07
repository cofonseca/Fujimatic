#!/bin/bash
echo "Building fm_wrapper.so ..."
gcc -shared -fPIC sdk-c-wrapper/fm_wrapper.c -o libs/linux/libfmwrapper.so
echo "Done."
