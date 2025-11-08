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
	captureCount int
	mu           sync.Mutex
}

// NewFakeCamera creates a new fake camera instance
func NewFakeCamera() *FakeCamera {
	return &FakeCamera{
		connected:    false,
		batteryLevel: 100,
		shutterSpeed: 1,
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

// SetShutter sets the shutter speed in seconds
func (f *FakeCamera) SetShutter(seconds int) error {
	f.mu.Lock()
	defer f.mu.Unlock()

	if !f.connected {
		return fmt.Errorf("camera not connected")
	}

	if seconds < 0 {
		return fmt.Errorf("shutter speed must be non-negative")
	}

	f.shutterSpeed = seconds
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
