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

// GetImageQuality returns the current image quality setting
func (r *RealCamera) GetImageQuality() (int, error) {
	r.mu.Lock()
	defer r.mu.Unlock()

	if !r.connected {
		return 0, fmt.Errorf("camera not connected")
	}

	if r.sdkCamera == nil {
		return 0, fmt.Errorf("SDK camera not initialized")
	}

	quality, err := r.sdkCamera.GetImageQuality()
	return int(quality), err
}

// SetImageQuality sets the image quality mode
func (r *RealCamera) SetImageQuality(quality int) error {
	r.mu.Lock()
	defer r.mu.Unlock()

	if !r.connected {
		return fmt.Errorf("camera not connected")
	}

	if r.sdkCamera == nil {
		return fmt.Errorf("SDK camera not initialized")
	}

	return r.sdkCamera.SetImageQuality(sdk.ImageQuality(quality))
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

// AdjustFocus makes a manual focus adjustment using relative position steps
// direction: "near" (closer) or "far" (farther)
// steps: number of focus steps to move (positive integer)
func (r *RealCamera) AdjustFocus(direction string, steps int) error {
	r.mu.Lock()
	defer r.mu.Unlock()

	if !r.connected {
		return fmt.Errorf("camera not connected")
	}

	if r.sdkCamera == nil {
		return fmt.Errorf("SDK camera not initialized")
	}

	// Convert string direction to sdk.FocusDirection
	var dir sdk.FocusDirection
	switch direction {
	case "near":
		dir = sdk.FocusDirectionNear
	case "far":
		dir = sdk.FocusDirectionFar
	default:
		return fmt.Errorf("invalid focus direction: %s (must be \"near\" or \"far\")", direction)
	}

	return r.sdkCamera.AdjustFocus(dir, steps)
}

// TriggerAutoFocus triggers a single-shot autofocus operation
func (r *RealCamera) TriggerAutoFocus() error {
	r.mu.Lock()
	defer r.mu.Unlock()

	if !r.connected {
		return fmt.Errorf("camera not connected")
	}

	if r.sdkCamera == nil {
		return fmt.Errorf("SDK camera not initialized")
	}

	return r.sdkCamera.TriggerAutoFocus()
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

// CaptureBulb performs a BULB mode capture with a timed exposure
// durationSeconds: exposure duration in seconds (e.g., 90 for 1.5 minutes, 300 for 5 minutes)
// Maximum duration is 30 minutes (1800 seconds)
func (r *RealCamera) CaptureBulb(durationSeconds int) error {
	r.mu.Lock()
	defer r.mu.Unlock()

	if !r.connected {
		return fmt.Errorf("camera not connected")
	}

	if r.sdkCamera == nil {
		return fmt.Errorf("SDK camera not initialized")
	}

	return r.sdkCamera.CaptureBulb(durationSeconds)
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

// ========== Live View Methods ==========

// StartLiveView starts live view streaming
func (r *RealCamera) StartLiveView() error {
	r.mu.Lock()
	defer r.mu.Unlock()

	if !r.connected {
		return fmt.Errorf("camera not connected")
	}

	if r.sdkCamera == nil {
		return fmt.Errorf("SDK camera not initialized")
	}

	return r.sdkCamera.StartLiveView()
}

// StopLiveView stops live view streaming
func (r *RealCamera) StopLiveView() error {
	r.mu.Lock()
	defer r.mu.Unlock()

	if !r.connected {
		return fmt.Errorf("camera not connected")
	}

	if r.sdkCamera == nil {
		return fmt.Errorf("SDK camera not initialized")
	}

	return r.sdkCamera.StopLiveView()
}

// GetLiveViewFrame retrieves a single JPEG frame from live view
func (r *RealCamera) GetLiveViewFrame() ([]byte, error) {
	r.mu.Lock()
	defer r.mu.Unlock()

	if !r.connected {
		return nil, fmt.Errorf("camera not connected")
	}

	if r.sdkCamera == nil {
		return nil, fmt.Errorf("SDK camera not initialized")
	}

	frame, err := r.sdkCamera.GetLiveViewFrame()
	if err != nil {
		return nil, err
	}

	return frame.Data, nil
}

// IsLiveViewActive checks if live view is currently running
func (r *RealCamera) IsLiveViewActive() (bool, error) {
	r.mu.Lock()
	defer r.mu.Unlock()

	if !r.connected {
		return false, fmt.Errorf("camera not connected")
	}

	if r.sdkCamera == nil {
		return false, fmt.Errorf("SDK camera not initialized")
	}

	return r.sdkCamera.IsLiveViewActive()
}

// SetLiveViewSize sets the live view image size (0=S, 1=M, 2=L)
func (r *RealCamera) SetLiveViewSize(size int) error {
	r.mu.Lock()
	defer r.mu.Unlock()

	if !r.connected {
		return fmt.Errorf("camera not connected")
	}

	if r.sdkCamera == nil {
		return fmt.Errorf("SDK camera not initialized")
	}

	return r.sdkCamera.SetLiveViewSize(size)
}
