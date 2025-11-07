#!/bin/bash
echo "Building mock fm_wrapper.dll for testing..."

# Create a mock library for testing
cat > libs/windows/fmwrapper.def << 'EOF'
EXPORTS
fm_init
fm_connect
fm_disconnect
fm_get_battery
fm_set_shutter
fm_capture
fm_download_last
EOF

echo "Mock DLL definition created"
echo "For actual testing, you would need the real Fujifilm SDK and proper C compiler"