//go:build cgo
// +build cgo

package hal

import (
	"fmt"
	"sync"

	"github.com/cfonseca/fujimatic/pkg/sdk"
)

// RealCamera wraps the SDK Camera to implement the HAL Camera interface
type RealCamera struct {
	sdkCamera *sdk.Camera
	connected bool
	mu        sync.Mutex
}

// NewRealCamera creates a new real camera instance
func NewRealCamera() *RealCamera {
	return &RealCamera{
		sdkCamera: nil,
		connected: false,
	}
}

// Connect establishes a connection to the real camera
func (r *RealCamera) Connect() error {
	r.mu.Lock()
	defer r.mu.Unlock()

	if r.connected {
		return fmt.Errorf("camera already connected")
	}

	sdkCamera, err := sdk.Connect()
	if err != nil {
		return fmt.Errorf("failed to connect to camera: %w", err)
	}

	r.sdkCamera = sdkCamera
	r.connected = true

	return nil
}

// Disconnect closes the connection to the real camera
func (r *RealCamera) Disconnect() error {
	r.mu.Lock()
	defer r.mu.Unlock()

	if !r.connected {
		return fmt.Errorf("camera not connected")
	}

	if r.sdkCamera == nil {
		return fmt.Errorf("SDK camera not initialized")
	}

	err := r.sdkCamera.Disconnect()
	if err == nil {
		r.connected = false
		r.sdkCamera = nil
	}

	return err
}

// IsConnected returns whether the camera is currently connected
func (r *RealCamera) IsConnected() bool {
	r.mu.Lock()
	defer r.mu.Unlock()
	return r.connected
}

// GetBattery returns the current battery percentage
func (r *RealCamera) GetBattery() (int, error) {
	r.mu.Lock()
	defer r.mu.Unlock()

	if !r.connected {
		return 0, fmt.Errorf("camera not connected")
	}

	if r.sdkCamera == nil {
		return 0, fmt.Errorf("SDK camera not initialized")
	}

	return r.sdkCamera.GetBattery()
}

// GetShutter returns the current shutter speed in microseconds
func (r *RealCamera) GetShutter() (int, error) {
	r.mu.Lock()
	defer r.mu.Unlock()

	if !r.connected {
		return 0, fmt.Errorf("camera not connected")
	}

	if r.sdkCamera == nil {
		return 0, fmt.Errorf("SDK camera not initialized")
	}

	return r.sdkCamera.GetShutter()
}

// SetShutter sets the shutter speed in microseconds
func (r *RealCamera) SetShutter(microseconds int) error {
	r.mu.Lock()
	defer r.mu.Unlock()

	if !r.connected {
		return fmt.Errorf("camera not connected")
	}

	if r.sdkCamera == nil {
		return fmt.Errorf("SDK camera not initialized")
	}

	return r.sdkCamera.SetShutter(microseconds)
}

// GetISO returns the current ISO sensitivity value
func (r *RealCamera) GetISO() (int, error) {
	r.mu.Lock()
	defer r.mu.Unlock()

	if !r.connected {
		return 0, fmt.Errorf("camera not connected")
	}

	if r.sdkCamera == nil {
		return 0, fmt.Errorf("SDK camera not initialized")
	}

	return r.sdkCamera.GetISO()
}

// ListShutterSpeeds returns all available shutter speeds for diagnostic purposes
func (r *RealCamera) ListShutterSpeeds() error {
	r.mu.Lock()
	defer r.mu.Unlock()

	if !r.connected {
		return fmt.Errorf("camera not connected")
	}

	if r.sdkCamera == nil {
		return fmt.Errorf("SDK camera not initialized")
	}

	return r.sdkCamera.ListShutterSpeeds()
}

// SetExposureMode sets the camera to Manual exposure mode
// Only 0x0001 (Manual) is supported - other modes are ignored
func (r *RealCamera) SetExposureMode(mode int) error {
	r.mu.Lock()
	defer r.mu.Unlock()

	if !r.connected {
		return fmt.Errorf("camera not connected")
	}

	if r.sdkCamera == nil {
		return fmt.Errorf("SDK camera not initialized")
	}

	// Always set to Manual mode (0x0001) for tethered control
	return r.sdkCamera.SetExposureMode(0x0001)
}

// GetExposureMode returns the current camera exposure mode
func (r *RealCamera) GetExposureMode() (int, error) {
	r.mu.Lock()
	defer r.mu.Unlock()

	if !r.connected {
		return 0, fmt.Errorf("camera not connected")
	}

	if r.sdkCamera == nil {
		return 0, fmt.Errorf("SDK camera not initialized")
	}

	return r.sdkCamera.GetExposureMode()
}

// SetISO sets the ISO sensitivity value
func (r *RealCamera) SetISO(iso int) error {
	r.mu.Lock()
	defer r.mu.Unlock()

	if !r.connected {
		return fmt.Errorf("camera not connected")
	}

	if r.sdkCamera == nil {
		return fmt.Errorf("SDK camera not initialized")
	}

	return r.sdkCamera.SetISO(iso)
}

// GetSupportedShutterSpeeds returns the list of supported shutter speeds in microseconds
func (r *RealCamera) GetSupportedShutterSpeeds() ([]int, error) {
	r.mu.Lock()
	defer r.mu.Unlock()

	if !r.connected {
		return nil, fmt.Errorf("camera not connected")
	}

	if r.sdkCamera == nil {
		return nil, fmt.Errorf("SDK camera not initialized")
	}

	return r.sdkCamera.GetSupportedShutterSpeeds()
}

// GetFocusMode returns the current focus mode
func (r *RealCamera) GetFocusMode() (int, error) {
	r.mu.Lock()
	defer r.mu.Unlock()

	if !r.connected {
		return 0, fmt.Errorf("camera not connected")
	}

	if r.sdkCamera == nil {
		return 0, fmt.Errorf("SDK camera not initialized")
	}

	mode, err := r.sdkCamera.GetFocusMode()
	return int(mode), err
}

// SetFocusMode sets the camera focus mode
func (r *RealCamera) SetFocusMode(mode int) error {
	r.mu.Lock()
	defer r.mu.Unlock()

	if !r.connected {
		return fmt.Errorf("camera not connected")
	}

	if r.sdkCamera == nil {
		return fmt.Errorf("SDK camera not initialized")
	}

	return r.sdkCamera.SetFocusMode(sdk.FocusMode(mode))
}

// GetSupportedFocusModes returns the list of focus modes supported by the attached lens
func (r *RealCamera) GetSupportedFocusModes() ([]int, error) {
	r.mu.Lock()
	defer r.mu.Unlock()

	if !r.connected {
		return nil, fmt.Errorf("camera not connected")
	}

	if r.sdkCamera == nil {
		return nil, fmt.Errorf("SDK camera not initialized")
	}

	modes, err := r.sdkCamera.GetSupportedFocusModes()
	if err != nil {
		return nil, err
	}

	// Convert sdk.FocusMode slice to int slice
	intModes := make([]int, len(modes))
	for i, mode := range modes {
		intModes[i] = int(mode)
	}

	return intModes, nil
}

// Capture triggers a photo capture
func (r *RealCamera) Capture() error {
	r.mu.Lock()
	defer r.mu.Unlock()

	if !r.connected {
		return fmt.Errorf("camera not connected")
	}

	if r.sdkCamera == nil {
		return fmt.Errorf("SDK camera not initialized")
	}

	return r.sdkCamera.Capture()
}

// DownloadLast downloads the last captured image
func (r *RealCamera) DownloadLast(outputDir, filename string) error {
	r.mu.Lock()
	defer r.mu.Unlock()

	if !r.connected {
		return fmt.Errorf("camera not connected")
	}

	if r.sdkCamera == nil {
		return fmt.Errorf("SDK camera not initialized")
	}

	return r.sdkCamera.DownloadLast(outputDir, filename)
}
