package hal

import (
	"fmt"
)

// Camera interface abstracts all camera operations
type Camera interface {
	// Connect establishes a connection to the camera
	Connect() error

	// Disconnect closes the connection to the camera
	Disconnect() error

	// IsConnected returns true if the camera is currently connected
	IsConnected() bool

	// GetBattery returns the current battery percentage
	GetBattery() (int, error)

	// GetShutter returns the current shutter speed in microseconds
	GetShutter() (int, error)

	// SetShutter sets the shutter speed in microseconds
	SetShutter(microseconds int) error

	// ListShutterSpeeds returns all available shutter speeds for diagnostic purposes
	ListShutterSpeeds() error

	// SetExposureMode sets the camera to Manual exposure mode
	SetExposureMode(mode int) error

	// GetExposureMode returns the current camera exposure mode
	GetExposureMode() (int, error)

	// GetISO returns the current ISO sensitivity value
	GetISO() (int, error)

	// SetISO sets the ISO sensitivity value
	SetISO(iso int) error

	// GetSupportedShutterSpeeds returns the list of supported shutter speeds in microseconds
	GetSupportedShutterSpeeds() ([]int, error)

	// Capture triggers a photo capture
	Capture() error

	// DownloadLast downloads the last captured image to the specified location
	DownloadLast(outputDir, filename string) error
}

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

// HAL interface for camera system initialization
type HAL interface {
	// Initialize initializes the camera system
	Initialize(sdkPath string) (InitResult, error)

	// NewCamera creates a new camera instance
	NewCamera() Camera
}

// InitializeSDK initializes the Fujifilm SDK
func InitializeSDK(sdkPath string) (InitResult, error) {
	if sdkPath == "" {
		return InitError, fmt.Errorf("SDK path cannot be empty")
	}

	// This will be implemented to use the real SDK or fake SDK
	// based on the current configuration
	return InitSuccess, nil
}
