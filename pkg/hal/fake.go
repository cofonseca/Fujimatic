package hal

import (
	"fmt"
	"sync"
	"time"
)

// FakeCamera provides a fake implementation of Camera for testing
type FakeCamera struct {
	connected      bool
	batteryLevel   int
	shutterSpeed   int
	iso            int
	imageQuality   int
	exposureMode   int
	focusMode      int
	captureCount   int
	liveViewActive bool
	liveViewSize   int
	mu             sync.Mutex
}

// NewFakeCamera creates a new fake camera instance
func NewFakeCamera() *FakeCamera {
	return &FakeCamera{
		connected:      false,
		batteryLevel:   100,
		shutterSpeed:   1,
		iso:            800,     // Default ISO
		imageQuality:   0x0005,  // Default to RAW+NORMAL
		exposureMode:   0x0006,  // Default to Program mode
		focusMode:      0x0001,  // Default to Manual focus
		captureCount:   0,
		liveViewActive: false,
		liveViewSize:   1, // Default to Medium size
	}
}

// Connect establishes a connection to the fake camera
func (f *FakeCamera) Connect() error {
	f.mu.Lock()
	defer f.mu.Unlock()

	if f.connected {
		return fmt.Errorf("camera already connected")
	}

	// Simulate connection delay
	time.Sleep(10 * time.Millisecond)

	f.connected = true
	return nil
}

// Disconnect closes the connection to the fake camera
func (f *FakeCamera) Disconnect() error {
	f.mu.Lock()
	defer f.mu.Unlock()

	if !f.connected {
		return fmt.Errorf("camera not connected")
	}

	// Simulate disconnection delay
	time.Sleep(5 * time.Millisecond)

	f.connected = false
	return nil
}

// IsConnected returns whether the camera is currently connected
func (f *FakeCamera) IsConnected() bool {
	f.mu.Lock()
	defer f.mu.Unlock()
	return f.connected
}

// GetBattery returns the current battery percentage
func (f *FakeCamera) GetBattery() (int, error) {
	f.mu.Lock()
	defer f.mu.Unlock()

	if !f.connected {
		return 0, fmt.Errorf("camera not connected")
	}

	return f.batteryLevel, nil
}

// GetShutter returns the current shutter speed in microseconds
func (f *FakeCamera) GetShutter() (int, error) {
	f.mu.Lock()
	defer f.mu.Unlock()

	if !f.connected {
		return 0, fmt.Errorf("camera not connected")
	}

	return f.shutterSpeed, nil
}

// SetShutter sets the shutter speed in microseconds
func (f *FakeCamera) SetShutter(microseconds int) error {
	f.mu.Lock()
	defer f.mu.Unlock()

	if !f.connected {
		return fmt.Errorf("camera not connected")
	}

	if microseconds < 125 {
		return fmt.Errorf("shutter speed too fast (minimum: 125 microseconds = 1/8000s)")
	}

	if microseconds > 3600000000 {
		return fmt.Errorf("shutter speed too slow (maximum: 3600000000 microseconds = 1 hour)")
	}

	f.shutterSpeed = microseconds
	return nil
}

// ListShutterSpeeds returns all available shutter speeds for diagnostic purposes
func (f *FakeCamera) ListShutterSpeeds() error {
	f.mu.Lock()
	defer f.mu.Unlock()

	if !f.connected {
		return fmt.Errorf("camera not connected")
	}

	// Fake camera returns a standard set of shutter speeds
	fmt.Println("Fake camera available shutter speeds (microseconds):")
	standardSpeeds := []int{125, 250, 500, 1000, 2000, 4000, 8000, 16000, 32000, 64000, 125000, 250000, 500000, 1000000, 2000000, 4000000}
	for i, speed := range standardSpeeds {
		fmt.Printf("  %2d: %7d μs (%.6f seconds)\n", i+1, speed, float64(speed)/1000000.0)
	}
	return nil
}

// SetExposureMode sets the camera to Manual exposure mode
// Only 0x0001 (Manual) is supported - other modes are ignored
func (f *FakeCamera) SetExposureMode(mode int) error {
	f.mu.Lock()
	defer f.mu.Unlock()

	if !f.connected {
		return fmt.Errorf("camera not connected")
	}

	// Only Manual mode (0x0001) is supported for tethered control
	f.exposureMode = 0x0001
	fmt.Println("Fake camera: Manual exposure mode set")
	return nil
}

// GetExposureMode returns the current camera exposure mode (always Manual for tethered control)
func (f *FakeCamera) GetExposureMode() (int, error) {
	f.mu.Lock()
	defer f.mu.Unlock()

	if !f.connected {
		return 0, fmt.Errorf("camera not connected")
	}

	// Always return Manual mode (0x0001) for tethered control
	return 0x0001, nil
}

// GetISO returns the current ISO sensitivity value
func (f *FakeCamera) GetISO() (int, error) {
	f.mu.Lock()
	defer f.mu.Unlock()

	if !f.connected {
		return 0, fmt.Errorf("camera not connected")
	}

	return f.iso, nil
}

// SetISO sets the ISO sensitivity value
func (f *FakeCamera) SetISO(iso int) error {
	f.mu.Lock()
	defer f.mu.Unlock()

	if !f.connected {
		return fmt.Errorf("camera not connected")
	}

	if iso < 100 || iso > 51200 {
		return fmt.Errorf("ISO must be between 100 and 51200")
	}

	f.iso = iso
	return nil
}

// GetImageQuality returns the current image quality setting
func (f *FakeCamera) GetImageQuality() (int, error) {
	f.mu.Lock()
	defer f.mu.Unlock()

	if !f.connected {
		return 0, fmt.Errorf("camera not connected")
	}

	return f.imageQuality, nil
}

// SetImageQuality sets the image quality mode
func (f *FakeCamera) SetImageQuality(quality int) error {
	f.mu.Lock()
	defer f.mu.Unlock()

	if !f.connected {
		return fmt.Errorf("camera not connected")
	}

	// Validate quality value (0x0001=RAW, 0x0002=FINE, 0x0003=NORMAL, 0x0004=RAW+FINE, 0x0005=RAW+NORMAL)
	if quality < 0x0001 || quality > 0x0005 {
		return fmt.Errorf("invalid image quality: 0x%04X", quality)
	}

	f.imageQuality = quality

	qualityName := "Unknown"
	switch quality {
	case 0x0001:
		qualityName = "RAW"
	case 0x0002:
		qualityName = "FINE"
	case 0x0003:
		qualityName = "NORMAL"
	case 0x0004:
		qualityName = "RAW+FINE"
	case 0x0005:
		qualityName = "RAW+NORMAL"
	}
	fmt.Printf("Fake camera: Image quality set to %s\n", qualityName)
	return nil
}

// GetSupportedShutterSpeeds returns the list of supported shutter speeds in microseconds
func (f *FakeCamera) GetSupportedShutterSpeeds() ([]int, error) {
	f.mu.Lock()
	defer f.mu.Unlock()

	if !f.connected {
		return nil, fmt.Errorf("camera not connected")
	}

	// Return common shutter speeds for fake camera testing
	return []int{
		125,    // 1/8000s
		250,    // 1/4000s
		500,    // 1/2000s
		1000,   // 1/1000s
		2000,   // 1/500s
		4000,   // 1/250s
		8000,   // 1/125s
		15625,  // 1/64s
		31250,  // 1/32s
		62500,  // 1/16s
		125000, // 1/8s
		250000, // 1/4s
		500000, // 1/2s
		1000000, // 1s
		2000000, // 2s
		4000000, // 4s
	}, nil
}

// GetFocusMode returns the current focus mode
func (f *FakeCamera) GetFocusMode() (int, error) {
	f.mu.Lock()
	defer f.mu.Unlock()

	if !f.connected {
		return 0, fmt.Errorf("camera not connected")
	}

	return f.focusMode, nil
}

// SetFocusMode sets the camera focus mode
func (f *FakeCamera) SetFocusMode(mode int) error {
	f.mu.Lock()
	defer f.mu.Unlock()

	if !f.connected {
		return fmt.Errorf("camera not connected")
	}

	// Validate mode (manual=0x0001, AFS=0x8001, AFC=0x8002)
	if mode != 0x0001 && mode != 0x8001 && mode != 0x8002 {
		return fmt.Errorf("invalid focus mode: 0x%04X", mode)
	}

	f.focusMode = mode
	modeName := "Unknown"
	switch mode {
	case 0x0001:
		modeName = "Manual"
	case 0x8001:
		modeName = "AF-S"
	case 0x8002:
		modeName = "AF-C"
	}
	fmt.Printf("Fake camera: Focus mode set to %s\n", modeName)
	return nil
}

// GetSupportedFocusModes returns the list of focus modes supported by the fake camera
func (f *FakeCamera) GetSupportedFocusModes() ([]int, error) {
	f.mu.Lock()
	defer f.mu.Unlock()

	if !f.connected {
		return nil, fmt.Errorf("camera not connected")
	}

	// Fake camera supports all three focus modes
	return []int{
		0x0001, // Manual
		0x8001, // AF-S
		0x8002, // AF-C
	}, nil
}

// AdjustFocus makes a manual focus adjustment (fake implementation)
// direction: "near" (closer) or "far" (farther)
// steps: number of focus steps to move (positive integer)
func (f *FakeCamera) AdjustFocus(direction string, steps int) error {
	f.mu.Lock()
	defer f.mu.Unlock()

	if !f.connected {
		return fmt.Errorf("camera not connected")
	}

	// Validate direction
	if direction != "near" && direction != "far" {
		return fmt.Errorf("invalid focus direction: %s (must be \"near\" or \"far\")", direction)
	}

	// Validate steps
	if steps <= 0 {
		return fmt.Errorf("invalid focus steps: %d (must be positive)", steps)
	}

	fmt.Printf("Fake camera: Adjusting focus %s by %d steps\n", direction, steps)
	return nil
}

// TriggerAutoFocus triggers a single-shot autofocus operation (fake implementation)
func (f *FakeCamera) TriggerAutoFocus() error {
	f.mu.Lock()
	defer f.mu.Unlock()

	if !f.connected {
		return fmt.Errorf("camera not connected")
	}

	// Check if in autofocus mode
	if f.focusMode == 0x0001 {
		return fmt.Errorf("autofocus trigger only works in AF-S or AF-C mode (currently in Manual)")
	}

	fmt.Println("Fake camera: Autofocus triggered")
	return nil
}

// Capture triggers a photo capture
func (f *FakeCamera) Capture() error {
	f.mu.Lock()
	defer f.mu.Unlock()

	if !f.connected {
		return fmt.Errorf("camera not connected")
	}

	// Simulate capture delay
	time.Sleep(100 * time.Millisecond)

	f.captureCount++

	// Simulate battery drain (1% per capture)
	if f.batteryLevel > 0 {
		f.batteryLevel--
	}

	return nil
}

// DownloadLast downloads the last captured image
func (f *FakeCamera) DownloadLast(outputDir, filename string) error {
	f.mu.Lock()
	defer f.mu.Unlock()

	if !f.connected {
		return fmt.Errorf("camera not connected")
	}

	if outputDir == "" || filename == "" {
		return fmt.Errorf("output directory and filename cannot be empty")
	}

	// Simulate download delay
	time.Sleep(50 * time.Millisecond)

	return nil
}

// GetCaptureCount returns the number of captures performed
func (f *FakeCamera) GetCaptureCount() int {
	f.mu.Lock()
	defer f.mu.Unlock()
	return f.captureCount
}

// SetBatteryLevel sets the battery level for testing purposes
func (f *FakeCamera) SetBatteryLevel(level int) {
	f.mu.Lock()
	defer f.mu.Unlock()

	if level >= 0 && level <= 100 {
		f.batteryLevel = level
	}
}

// ========== Live View Methods ==========

// StartLiveView starts live view streaming
func (f *FakeCamera) StartLiveView() error {
	f.mu.Lock()
	defer f.mu.Unlock()

	if !f.connected {
		return fmt.Errorf("camera not connected")
	}

	f.liveViewActive = true
	return nil
}

// StopLiveView stops live view streaming
func (f *FakeCamera) StopLiveView() error {
	f.mu.Lock()
	defer f.mu.Unlock()

	if !f.connected {
		return fmt.Errorf("camera not connected")
	}

	f.liveViewActive = false
	return nil
}

// GetLiveViewFrame returns a fake JPEG frame
// This returns a minimal valid JPEG for testing
func (f *FakeCamera) GetLiveViewFrame() ([]byte, error) {
	f.mu.Lock()
	defer f.mu.Unlock()

	if !f.connected {
		return nil, fmt.Errorf("camera not connected")
	}

	if !f.liveViewActive {
		return nil, fmt.Errorf("live view not active")
	}

	// Return a minimal valid JPEG image (1x1 pixel gray)
	// This is a real JPEG that can be displayed in browsers
	minimalJPEG := []byte{
		0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01,
		0x01, 0x01, 0x00, 0x48, 0x00, 0x48, 0x00, 0x00, 0xFF, 0xDB, 0x00, 0x43,
		0x00, 0x03, 0x02, 0x02, 0x02, 0x02, 0x02, 0x03, 0x02, 0x02, 0x02, 0x03,
		0x03, 0x03, 0x03, 0x04, 0x06, 0x04, 0x04, 0x04, 0x04, 0x04, 0x08, 0x06,
		0x06, 0x05, 0x06, 0x09, 0x08, 0x0A, 0x0A, 0x09, 0x08, 0x09, 0x09, 0x0A,
		0x0C, 0x0F, 0x0C, 0x0A, 0x0B, 0x0E, 0x0B, 0x09, 0x09, 0x0D, 0x11, 0x0D,
		0x0E, 0x0F, 0x10, 0x10, 0x11, 0x10, 0x0A, 0x0C, 0x12, 0x13, 0x12, 0x10,
		0x13, 0x0F, 0x10, 0x10, 0x10, 0xFF, 0xC9, 0x00, 0x0B, 0x08, 0x00, 0x01,
		0x00, 0x01, 0x01, 0x01, 0x11, 0x00, 0xFF, 0xCC, 0x00, 0x06, 0x00, 0x10,
		0x10, 0x05, 0xFF, 0xDA, 0x00, 0x08, 0x01, 0x01, 0x00, 0x00, 0x3F, 0x00,
		0xD2, 0xCF, 0x20, 0xFF, 0xD9,
	}

	return minimalJPEG, nil
}

// IsLiveViewActive checks if live view is currently running
func (f *FakeCamera) IsLiveViewActive() (bool, error) {
	f.mu.Lock()
	defer f.mu.Unlock()

	if !f.connected {
		return false, fmt.Errorf("camera not connected")
	}

	return f.liveViewActive, nil
}

// SetLiveViewSize sets the live view image size (0=S, 1=M, 2=L)
func (f *FakeCamera) SetLiveViewSize(size int) error {
	f.mu.Lock()
	defer f.mu.Unlock()

	if !f.connected {
		return fmt.Errorf("camera not connected")
	}

	if size < 0 || size > 2 {
		return fmt.Errorf("invalid size: must be 0 (S), 1 (M), or 2 (L)")
	}

	f.liveViewSize = size
	return nil
}
