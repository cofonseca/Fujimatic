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

// GetShutter returns the current shutter speed in seconds
func (c *Camera) GetShutter() (int, error) {
	if !c.connected {
		return 0, fmt.Errorf("camera not connected")
	}

	var seconds C.int
	result := C.fm_get_shutter(&seconds)

	if result == 0 {
		return int(seconds), nil
	}
	return 0, fmt.Errorf("failed to get shutter speed, code: %d", result)
}

// SetShutter sets the shutter speed in seconds
func (c *Camera) SetShutter(seconds int) error {
	if !c.connected {
		return fmt.Errorf("camera not connected")
	}

	if seconds < 0 {
		return fmt.Errorf("shutter speed must be non-negative")
	}

	result := C.fm_set_shutter(C.int(seconds))
	if result == 0 {
		return nil
	}
	return fmt.Errorf("failed to set shutter speed, code: %d", result)
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