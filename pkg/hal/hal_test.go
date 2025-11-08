package hal

import (
	"testing"
	"time"
)

func TestFakeCamera_Connect(t *testing.T) {
	camera := NewFakeCamera()

	// Test initial state
	if camera.IsConnected() {
		t.Error("new camera should not be connected")
	}

	// Test successful connection
	err := camera.Connect()
	if err != nil {
		t.Errorf("Connect() failed: %v", err)
	}

	if !camera.IsConnected() {
		t.Error("camera should be connected after Connect()")
	}

	// Test double connection
	err = camera.Connect()
	if err == nil {
		t.Error("Connect() should fail when already connected")
	}
}

func TestFakeCamera_Disconnect(t *testing.T) {
	camera := NewFakeCamera()

	// Test disconnection when not connected
	err := camera.Disconnect()
	if err == nil {
		t.Error("Disconnect() should fail when not connected")
	}

	// Test successful disconnection
	err = camera.Connect()
	if err != nil {
		t.Fatalf("Connect() failed: %v", err)
	}

	err = camera.Disconnect()
	if err != nil {
		t.Errorf("Disconnect() failed: %v", err)
	}

	if camera.IsConnected() {
		t.Error("camera should not be connected after Disconnect()")
	}
}

func TestFakeCamera_GetBattery(t *testing.T) {
	camera := NewFakeCamera()

	// Test battery level when not connected
	_, err := camera.GetBattery()
	if err == nil {
		t.Error("GetBattery() should fail when not connected")
	}

	// Test battery level when connected
	camera.Connect()
	level, err := camera.GetBattery()
	if err != nil {
		t.Errorf("GetBattery() failed: %v", err)
	}

	if level != 100 {
		t.Errorf("expected battery level 100, got %d", level)
	}
}

func TestFakeCamera_SetShutter(t *testing.T) {
	camera := NewFakeCamera()

	// Test setting shutter when not connected
	err := camera.SetShutter(125000) // 0.125 seconds
	if err == nil {
		t.Error("SetShutter() should fail when not connected")
	}

	// Test setting shutter when connected
	camera.Connect()
	err = camera.SetShutter(125000) // 0.125 seconds
	if err != nil {
		t.Errorf("SetShutter() failed: %v", err)
	}

	// Test invalid shutter speed (too fast)
	err = camera.SetShutter(100) // 100 microseconds < 125 minimum
	if err == nil {
		t.Error("SetShutter() should fail with too-fast shutter speed")
	}

	// Test invalid shutter speed (too slow)
	err = camera.SetShutter(4000000000) // 4000 seconds > 3600 maximum
	if err == nil {
		t.Error("SetShutter() should fail with too-slow shutter speed")
	}
}

func TestFakeCamera_Capture(t *testing.T) {
	camera := NewFakeCamera()

	// Test capture when not connected
	err := camera.Capture()
	if err == nil {
		t.Error("Capture() should fail when not connected")
	}

	// Test capture when connected
	camera.Connect()
	initialBattery, _ := camera.GetBattery()
	initialCaptures := camera.GetCaptureCount()

	err = camera.Capture()
	if err != nil {
		t.Errorf("Capture() failed: %v", err)
	}

	// Check capture count increased
	if camera.GetCaptureCount() != initialCaptures+1 {
		t.Error("capture count should increase after Capture()")
	}

	// Check battery decreased
	newBattery, _ := camera.GetBattery()
	if newBattery != initialBattery-1 {
		t.Errorf("battery should decrease by 1, expected %d, got %d", initialBattery-1, newBattery)
	}
}

func TestFakeCamera_DownloadLast(t *testing.T) {
	camera := NewFakeCamera()

	// Test download when not connected
	err := camera.DownloadLast("testdir", "testfile")
	if err == nil {
		t.Error("DownloadLast() should fail when not connected")
	}

	// Test download with empty parameters
	camera.Connect()
	err = camera.DownloadLast("", "testfile")
	if err == nil {
		t.Error("DownloadLast() should fail with empty output directory")
	}

	err = camera.DownloadLast("testdir", "")
	if err == nil {
		t.Error("DownloadLast() should fail with empty filename")
	}

	// Test successful download
	err = camera.DownloadLast("testdir", "testfile")
	if err != nil {
		t.Errorf("DownloadLast() failed: %v", err)
	}
}

func TestFakeCamera_BatteryDrain(t *testing.T) {
	camera := NewFakeCamera()
	camera.SetBatteryLevel(50)
	camera.Connect()

	// Perform multiple captures
	for i := 0; i < 10; i++ {
		err := camera.Capture()
		if err != nil {
			t.Fatalf("Capture() %d failed: %v", i+1, err)
		}
	}

	// Check battery level
	battery, err := camera.GetBattery()
	if err != nil {
		t.Errorf("GetBattery() failed: %v", err)
	}

	expectedBattery := 40 // 50 - 10 captures
	if battery != expectedBattery {
		t.Errorf("expected battery level %d, got %d", expectedBattery, battery)
	}
}

func TestFakeCamera_ConcurrentAccess(t *testing.T) {
	camera := NewFakeCamera()
	camera.Connect()

	// Test concurrent access
	done := make(chan bool, 5)

	for i := 0; i < 5; i++ {
		go func() {
			for j := 0; j < 10; j++ {
				camera.GetBattery()
				camera.SetShutter(j)
			}
			done <- true
		}()
	}

	// Wait for all goroutines to complete
	timeout := time.After(5 * time.Second)
	for i := 0; i < 5; i++ {
		select {
		case <-done:
			// Success
		case <-timeout:
			t.Fatal("concurrent access test timed out")
		}
	}
}
