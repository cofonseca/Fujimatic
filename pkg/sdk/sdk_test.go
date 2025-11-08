//go:build cgo
// +build cgo

package sdk

import (
	"testing"
)

func TestSDK(t *testing.T) {
	// Test SDK initialization with dummy path
	result, err := Init("dummy/sdk/path")
	if err != nil {
		t.Errorf("Init failed: %v", err)
	}
	if result != InitSuccess {
		t.Errorf("Expected InitSuccess, got %v", result)
	}
	t.Logf("SDK Init result: %v", result)
}

func TestConnect(t *testing.T) {
	// Test connection (will use mock C functions)
	camera, err := Connect()
	if err != nil {
		t.Errorf("Connect failed: %v", err)
	}
	if camera == nil {
		t.Error("Connect returned nil camera")
	}
	if !camera.IsConnected() {
		t.Error("Camera should be connected after Connect()")
	}
	t.Log("Camera connection successful")
}

func TestCameraOperations(t *testing.T) {
	// Setup: connect to camera
	camera, err := Connect()
	if err != nil {
		t.Fatalf("Failed to connect: %v", err)
	}
	defer camera.Disconnect()

	// Test GetBattery
	percent, err := camera.GetBattery()
	if err != nil {
		t.Errorf("GetBattery failed: %v", err)
	}
	if percent != 100 {
		t.Errorf("Expected 100%% battery, got %d%%", percent)
	}
	t.Logf("Battery level: %d%%", percent)

	// Test SetShutter
	err = camera.SetShutter(30)
	if err != nil {
		t.Errorf("SetShutter failed: %v", err)
	}
	t.Log("Shutter set to 30 seconds")

	// Test Capture
	err = camera.Capture()
	if err != nil {
		t.Errorf("Capture failed: %v", err)
	}
	t.Log("Capture triggered")

	// Test DownloadLast
	err = camera.DownloadLast("/tmp", "test.raf")
	if err != nil {
		t.Errorf("DownloadLast failed: %v", err)
	}
	t.Log("Download completed")
}

func TestErrorHandling(t *testing.T) {
	// Test SDK init with empty path
	_, err := Init("")
	if err == nil {
		t.Error("Init should fail with empty path")
	}

	// Test operations on unconnected camera
	camera := &Camera{connected: false}
	
	_, err = camera.GetBattery()
	if err == nil {
		t.Error("GetBattery should fail on disconnected camera")
	}

	err = camera.SetShutter(10)
	if err == nil {
		t.Error("SetShutter should fail on disconnected camera")
	}

	err = camera.Capture()
	if err == nil {
		t.Error("Capture should fail on disconnected camera")
	}

	err = camera.DownloadLast("/tmp", "test.raf")
	if err == nil {
		t.Error("DownloadLast should fail on disconnected camera")
	}

	// Test negative shutter speed
	_, connectErr := Connect()
	if connectErr != nil {
		t.Fatalf("Setup failed: %v", connectErr)
	}
	defer camera.Disconnect()

	err = camera.SetShutter(-1)
	if err == nil {
		t.Error("SetShutter should fail with negative value")
	}

	// Test empty parameters
	err = camera.DownloadLast("", "test.raf")
	if err == nil {
		t.Error("DownloadLast should fail with empty directory")
	}

	err = camera.DownloadLast("/tmp", "")
	if err == nil {
		t.Error("DownloadLast should fail with empty filename")
	}
}