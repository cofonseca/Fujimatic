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

	// GetFocusMode returns the current focus mode
	GetFocusMode() (int, error)

	// SetFocusMode sets the camera focus mode
	SetFocusMode(mode int) error

	// GetSupportedFocusModes returns the list of focus modes supported by the attached lens
	GetSupportedFocusModes() ([]int, error)

	// AdjustFocus makes a manual focus adjustment using relative position steps
	// direction: "near" (closer) or "far" (farther)
	// steps: number of focus steps to move (positive integer)
	// Useful for astrophotography where autofocus can't lock on dim stars
	AdjustFocus(direction string, steps int) error

	// TriggerAutoFocus triggers a single-shot autofocus operation
	// Only works when camera is in automatic focus mode (AF-S or AF-C)
	TriggerAutoFocus() error

	// Capture triggers a photo capture
	Capture() error

	// DownloadLast downloads the last captured image to the specified location
	DownloadLast(outputDir, filename string) error

	// Live view methods
	// StartLiveView starts live view streaming
	StartLiveView() error

	// StopLiveView stops live view streaming
	StopLiveView() error

	// GetLiveViewFrame retrieves a single JPEG frame from live view
	GetLiveViewFrame() ([]byte, error)

	// IsLiveViewActive checks if live view is currently running
	IsLiveViewActive() (bool, error)

	// SetLiveViewSize sets the live view image size (0=S, 1=M, 2=L)
	SetLiveViewSize(size int) error
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
