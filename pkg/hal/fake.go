package hal

import (
	"fmt"
	"sync"
	"time"
)

// FakeCamera provides a fake implementation of Camera for testing
type FakeCamera struct {
	connected    bool
	batteryLevel int
	shutterSpeed int
	iso          int
	exposureMode int
	captureCount int
	mu           sync.Mutex
}

// NewFakeCamera creates a new fake camera instance
func NewFakeCamera() *FakeCamera {
	return &FakeCamera{
		connected:    false,
		batteryLevel: 100,
		shutterSpeed: 1,
		iso:          800, // Default ISO
		exposureMode: 0x0006, // Default to Program mode
		captureCount: 0,
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
