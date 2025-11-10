//go:build cgo
// +build cgo

package sdk

/*
#cgo CFLAGS: -I../../sdk-c-wrapper
#cgo LDFLAGS: -L../../libs/windows -lfmwrapper

#include <stdlib.h>
#include "fm_wrapper.h"
*/
import "C"

import (
	"fmt"
	"unsafe"
)

// InitResult represents the result of SDK initialization
type InitResult int

const (
	InitSuccess InitResult = iota
	InitError
)

func (r InitResult) String() string {
	switch r {
	case InitSuccess:
		return "Success"
	case InitError:
		return "Error"
	default:
		return "Unknown"
	}
}

// Camera represents a connected camera session
type Camera struct {
	connected bool
}

// Initialize the Fujifilm SDK
func Init(sdkPath string) (InitResult, error) {
	if sdkPath == "" {
		return InitError, fmt.Errorf("SDK path cannot be empty")
	}

	// Convert Go string to C string
	cPath := C.CString(sdkPath)
	defer C.free(unsafe.Pointer(cPath))

	// Call C function
	result := C.fm_init(cPath)

	if result == 0 {
		return InitSuccess, nil
	}
	return InitError, fmt.Errorf("SDK initialization failed with code: %d", result)
}

// SetVerbose enables or disables verbose logging in the C wrapper layer
func SetVerbose(enabled bool) error {
	var cEnabled C.int
	if enabled {
		cEnabled = 1
	} else {
		cEnabled = 0
	}

	result := C.fm_set_verbose(cEnabled)
	if result == 0 {
		return nil
	}
	return fmt.Errorf("failed to set verbose mode, code: %d", result)
}

// Connect to the camera
func Connect() (*Camera, error) {
	result := C.fm_connect()
	if result == 0 {
		return &Camera{connected: true}, nil
	}
	return nil, fmt.Errorf("camera connection failed with code: %d", result)
}

// Disconnect from the camera
func (c *Camera) Disconnect() error {
	if !c.connected {
		return fmt.Errorf("camera not connected")
	}

	result := C.fm_disconnect()
	if result == 0 {
		c.connected = false
		return nil
	}
	return fmt.Errorf("disconnection failed with code: %d", result)
}

// GetBattery returns the current battery percentage
func (c *Camera) GetBattery() (int, error) {
	if !c.connected {
		return 0, fmt.Errorf("camera not connected")
	}

	var percent C.int
	result := C.fm_get_battery(&percent)
	
	if result == 0 {
		return int(percent), nil
	}
	return 0, fmt.Errorf("failed to get battery level, code: %d", result)
}

// GetShutter returns the current shutter speed in microseconds
func (c *Camera) GetShutter() (int, error) {
	if !c.connected {
		return 0, fmt.Errorf("camera not connected")
	}

	var microseconds C.int
	result := C.fm_get_shutter(&microseconds)

	if result == 0 {
		return int(microseconds), nil
	}
	return 0, fmt.Errorf("failed to get shutter speed, code: %d", result)
}

// SetShutter sets the shutter speed in microseconds
func (c *Camera) SetShutter(microseconds int) error {
	if !c.connected {
		return fmt.Errorf("camera not connected")
	}

	if microseconds < 125 {
		return fmt.Errorf("shutter speed too fast (minimum: 125 microseconds = 1/8000s)")
	}

	if microseconds > 3600000000 {
		return fmt.Errorf("shutter speed too slow (maximum: 3600000000 microseconds = 1 hour)")
	}

	result := C.fm_set_shutter(C.int(microseconds))
	if result == 0 {
		return nil
	}
	return fmt.Errorf("failed to set shutter speed, code: %d", result)
}

// ListShutterSpeeds returns all available shutter speeds for diagnostic purposes
func (c *Camera) ListShutterSpeeds() error {
	if !c.connected {
		return fmt.Errorf("camera not connected")
	}

	result := C.fm_list_shutter_speeds()
	if result == 0 {
		return nil
	}
	return fmt.Errorf("failed to list shutter speeds, code: %d", result)
}

// GetSupportedShutterSpeeds returns the list of supported shutter speeds in microseconds
func (c *Camera) GetSupportedShutterSpeeds() ([]int, error) {
	if !c.connected {
		return nil, fmt.Errorf("camera not connected")
	}

	// First get the count
	var count C.int
	result := C.fm_get_supported_shutter_count(&count)
	if result != 0 {
		return nil, fmt.Errorf("failed to get shutter speed count, code: %d", result)
	}

	if count == 0 {
		return nil, fmt.Errorf("no supported shutter speeds found - ensure camera is in Manual mode")
	}

	// Allocate array for the speeds
	speeds := make([]C.int, int(count))
	result = C.fm_get_supported_shutter_speeds(&count, &speeds[0])
	if result != 0 {
		return nil, fmt.Errorf("failed to get shutter speeds, code: %d", result)
	}

	// Convert to Go ints
	goSpeeds := make([]int, int(count))
	for i := 0; i < int(count); i++ {
		goSpeeds[i] = int(speeds[i])
	}

	return goSpeeds, nil
}

// SetExposureMode sets the camera to Manual exposure mode
// Only 0x0001 (Manual) is supported for tethered control
func (c *Camera) SetExposureMode(mode int) error {
	if !c.connected {
		return fmt.Errorf("camera not connected")
	}

	// Always set to Manual mode (0x0001) for tethered control
	result := C.fm_set_exposure_mode(C.int(0x0001))
	if result == 0 {
		return nil
	}
	return fmt.Errorf("failed to set Manual exposure mode, code: %d", result)
}

// GetExposureMode returns the current camera exposure mode
func (c *Camera) GetExposureMode() (int, error) {
	if !c.connected {
		return 0, fmt.Errorf("camera not connected")
	}

	var mode C.int
	result := C.fm_get_exposure_mode(&mode)

	if result == 0 {
		return int(mode), nil
	}
	return 0, fmt.Errorf("failed to get exposure mode, code: %d", result)
}

// GetISO returns the current ISO sensitivity value
func (c *Camera) GetISO() (int, error) {
	if !c.connected {
		return 0, fmt.Errorf("camera not connected")
	}

	var iso C.int
	result := C.fm_get_iso(&iso)

	if result == 0 {
		return int(iso), nil
	}
	return 0, fmt.Errorf("failed to get ISO, code: %d", result)
}

// SetISO sets the ISO sensitivity value
func (c *Camera) SetISO(iso int) error {
	if !c.connected {
		return fmt.Errorf("camera not connected")
	}

	if iso < 100 || iso > 51200 {
		return fmt.Errorf("ISO must be between 100 and 51200")
	}

	result := C.fm_set_iso(C.int(iso))
	if result == 0 {
		return nil
	}
	return fmt.Errorf("failed to set ISO, code: %d", result)
}

// FocusMode represents camera focus modes
type FocusMode int

const (
	FocusModeManual FocusMode = 0x0001 // Manual focus
	FocusModeAFS    FocusMode = 0x8001 // AF-S (single-shot autofocus)
	FocusModeAFC    FocusMode = 0x8002 // AF-C (continuous autofocus)
)

func (f FocusMode) String() string {
	switch f {
	case FocusModeManual:
		return "Manual"
	case FocusModeAFS:
		return "AF-S"
	case FocusModeAFC:
		return "AF-C"
	default:
		return "Unknown"
	}
}

// GetFocusMode returns the current focus mode
func (c *Camera) GetFocusMode() (FocusMode, error) {
	if !c.connected {
		return 0, fmt.Errorf("camera not connected")
	}

	var mode C.int
	result := C.fm_get_focus_mode(&mode)

	if result == 0 {
		return FocusMode(mode), nil
	}
	return 0, fmt.Errorf("failed to get focus mode, code: %d", result)
}

// SetFocusMode sets the camera focus mode
func (c *Camera) SetFocusMode(mode FocusMode) error {
	if !c.connected {
		return fmt.Errorf("camera not connected")
	}

	// Validate mode
	if mode != FocusModeManual && mode != FocusModeAFS && mode != FocusModeAFC {
		return fmt.Errorf("invalid focus mode: 0x%04X", mode)
	}

	result := C.fm_set_focus_mode(C.int(mode))
	if result == 0 {
		return nil
	}
	return fmt.Errorf("failed to set focus mode, code: %d", result)
}

// GetSupportedFocusModes returns the list of focus modes supported by the attached lens
func (c *Camera) GetSupportedFocusModes() ([]FocusMode, error) {
	if !c.connected {
		return nil, fmt.Errorf("camera not connected")
	}

	// First get the count
	var count C.int
	result := C.fm_get_supported_focus_modes(&count, nil)
	if result != 0 {
		return nil, fmt.Errorf("failed to get focus mode count, code: %d", result)
	}

	if count == 0 {
		return nil, fmt.Errorf("no supported focus modes found")
	}

	// Allocate array for the modes
	modes := make([]C.int, int(count))
	result = C.fm_get_supported_focus_modes(&count, &modes[0])
	if result != 0 {
		return nil, fmt.Errorf("failed to get focus modes, code: %d", result)
	}

	// Convert to Go FocusMode slice
	goModes := make([]FocusMode, int(count))
	for i := 0; i < int(count); i++ {
		goModes[i] = FocusMode(modes[i])
	}

	return goModes, nil
}

// Capture triggers a photo capture
func (c *Camera) Capture() error {
	if !c.connected {
		return fmt.Errorf("camera not connected")
	}

	result := C.fm_capture()
	if result == 0 {
		return nil
	}
	return fmt.Errorf("capture failed with code: %d", result)
}

// DownloadLast downloads the last captured image
func (c *Camera) DownloadLast(outputDir, filename string) error {
	if !c.connected {
		return fmt.Errorf("camera not connected")
	}

	if outputDir == "" || filename == "" {
		return fmt.Errorf("output directory and filename cannot be empty")
	}

	// Convert Go strings to C strings
	cOutputDir := C.CString(outputDir)
	cFilename := C.CString(filename)
	defer C.free(unsafe.Pointer(cOutputDir))
	defer C.free(unsafe.Pointer(cFilename))

	result := C.fm_download_last(cOutputDir, cFilename)
	if result == 0 {
		return nil
	}
	return fmt.Errorf("download failed with code: %d", result)
}

// IsConnected returns whether the camera is currently connected
func (c *Camera) IsConnected() bool {
	return c.connected
}