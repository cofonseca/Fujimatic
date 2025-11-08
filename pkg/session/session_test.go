package session

import (
	"encoding/json"
	"os"
	"path/filepath"
	"testing"

	"github.com/cfonseca/fujimatic/pkg/hal"
)

func TestNew(t *testing.T) {
	camera := hal.NewFakeCamera()

	tests := []struct {
		name        string
		projectName string
		outputDir   string
		camera      hal.Camera
		wantErr     bool
	}{
		{
			name:        "valid session",
			projectName: "test_project",
			outputDir:   "/tmp/captures",
			camera:      camera,
			wantErr:     false,
		},
		{
			name:        "empty project name",
			projectName: "",
			outputDir:   "/tmp/captures",
			camera:      camera,
			wantErr:     true,
		},
		{
			name:        "empty output dir",
			projectName: "test_project",
			outputDir:   "",
			camera:      camera,
			wantErr:     true,
		},
		{
			name:        "nil camera",
			projectName: "test_project",
			outputDir:   "/tmp/captures",
			camera:      nil,
			wantErr:     true,
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			session, err := New(tt.projectName, tt.outputDir, tt.camera)
			if (err != nil) != tt.wantErr {
				t.Errorf("New() error = %v, wantErr %v", err, tt.wantErr)
				return
			}

			if !tt.wantErr {
				if session == nil {
					t.Error("New() returned nil session")
					return
				}
				if session.ProjectName != tt.projectName {
					t.Errorf("ProjectName = %v, want %v", session.ProjectName, tt.projectName)
				}
				if session.OutputDir != tt.outputDir {
					t.Errorf("OutputDir = %v, want %v", session.OutputDir, tt.outputDir)
				}
				if session.SequenceNumber != 1 {
					t.Errorf("SequenceNumber = %v, want 1", session.SequenceNumber)
				}
			}
		})
	}
}

func TestGetNextFilename(t *testing.T) {
	camera := hal.NewFakeCamera()

	tests := []struct {
		name           string
		projectName    string
		sequenceNumber int
		wantFilename   string
	}{
		{
			name:           "sequence 1",
			projectName:    "orion_m42",
			sequenceNumber: 1,
			wantFilename:   "orion_m42_0001.RAF",
		},
		{
			name:           "sequence 42",
			projectName:    "test_project",
			sequenceNumber: 42,
			wantFilename:   "test_project_0042.RAF",
		},
		{
			name:           "sequence 9999",
			projectName:    "andromeda",
			sequenceNumber: 9999,
			wantFilename:   "andromeda_9999.RAF",
		},
		{
			name:           "sequence 10000",
			projectName:    "large_seq",
			sequenceNumber: 10000,
			wantFilename:   "large_seq_10000.RAF",
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			s := &Session{
				ProjectName:    tt.projectName,
				OutputDir:      "/tmp/test",
				SequenceNumber: tt.sequenceNumber,
				camera:         camera,
			}

			filename := s.GetNextFilename()
			if filename != tt.wantFilename {
				t.Errorf("GetNextFilename() = %v, want %v", filename, tt.wantFilename)
			}
		})
	}
}

func TestGetNextFilePath(t *testing.T) {
	camera := hal.NewFakeCamera()

	s := &Session{
		ProjectName:    "test_project",
		OutputDir:      "/tmp/captures",
		SequenceNumber: 5,
		camera:         camera,
	}

	expected := filepath.Join("/tmp/captures", "test_project_0005.RAF")
	result := s.GetNextFilePath()

	if result != expected {
		t.Errorf("GetNextFilePath() = %v, want %v", result, expected)
	}
}

func TestCapture(t *testing.T) {
	// Create temporary directory for test
	tempDir := t.TempDir()

	camera := hal.NewFakeCamera()

	// Connect camera
	if err := camera.Connect(); err != nil {
		t.Fatalf("failed to connect camera: %v", err)
	}

	s := &Session{
		ProjectName:    "test_capture",
		OutputDir:      tempDir,
		SequenceNumber: 1,
		camera:         camera,
	}

	// First capture
	if err := s.Capture(); err != nil {
		t.Errorf("Capture() error = %v", err)
	}

	if s.SequenceNumber != 2 {
		t.Errorf("SequenceNumber = %v, want 2", s.SequenceNumber)
	}

	// Second capture
	if err := s.Capture(); err != nil {
		t.Errorf("Capture() error = %v", err)
	}

	if s.SequenceNumber != 3 {
		t.Errorf("SequenceNumber = %v, want 3", s.SequenceNumber)
	}

	// Verify battery drained (FakeCamera drains 1% per capture)
	battery, err := camera.GetBattery()
	if err != nil {
		t.Errorf("GetBattery() error = %v", err)
	}

	expectedBattery := 98 // Started at 100%, 2 captures = 2%
	if battery != expectedBattery {
		t.Errorf("Battery = %v%%, want %v%%", battery, expectedBattery)
	}
}

func TestCaptureNotConnected(t *testing.T) {
	tempDir := t.TempDir()

	camera := hal.NewFakeCamera()
	// Don't connect camera

	s := &Session{
		ProjectName:    "test_capture",
		OutputDir:      tempDir,
		SequenceNumber: 1,
		camera:         camera,
	}

	err := s.Capture()
	if err == nil {
		t.Error("Capture() expected error when camera not connected, got nil")
	}
}

func TestCaptureWithFileCollision(t *testing.T) {
	tempDir := t.TempDir()

	camera := hal.NewFakeCamera()
	if err := camera.Connect(); err != nil {
		t.Fatalf("failed to connect camera: %v", err)
	}

	s := &Session{
		ProjectName:    "collision_test",
		OutputDir:      tempDir,
		SequenceNumber: 1,
		camera:         camera,
	}

	// Create existing file at sequence 1
	existingFile := filepath.Join(tempDir, "collision_test_0001.RAF")
	if err := os.WriteFile(existingFile, []byte("existing"), 0644); err != nil {
		t.Fatalf("failed to create existing file: %v", err)
	}

	// Capture should skip sequence 1 and use sequence 2
	if err := s.Capture(); err != nil {
		t.Errorf("Capture() error = %v", err)
	}

	if s.SequenceNumber != 3 {
		t.Errorf("SequenceNumber = %v, want 3 (skipped 1, used 2, incremented to 3)", s.SequenceNumber)
	}

	// Verify file was not created at sequence 1
	existingContent, err := os.ReadFile(existingFile)
	if err != nil {
		t.Errorf("failed to read existing file: %v", err)
	}
	if string(existingContent) != "existing" {
		t.Error("existing file was overwritten")
	}
}

func TestSaveAndLoad(t *testing.T) {
	camera := hal.NewFakeCamera()

	// Create a session
	originalSession := &Session{
		ProjectName:    "test_save_load",
		OutputDir:      "/tmp/test_captures",
		SequenceNumber: 42,
		camera:         camera,
	}

	// Save the session
	if err := originalSession.Save(); err != nil {
		t.Fatalf("Save() error = %v", err)
	}

	// Load the session
	loadedSession, err := Load(camera)
	if err != nil {
		t.Fatalf("Load() error = %v", err)
	}

	// Verify fields match
	if loadedSession.ProjectName != originalSession.ProjectName {
		t.Errorf("ProjectName = %v, want %v", loadedSession.ProjectName, originalSession.ProjectName)
	}

	if loadedSession.OutputDir != originalSession.OutputDir {
		t.Errorf("OutputDir = %v, want %v", loadedSession.OutputDir, originalSession.OutputDir)
	}

	if loadedSession.SequenceNumber != originalSession.SequenceNumber {
		t.Errorf("SequenceNumber = %v, want %v", loadedSession.SequenceNumber, originalSession.SequenceNumber)
	}

	// Clean up
	configDir, err := getConfigDir()
	if err != nil {
		t.Fatalf("failed to get config dir: %v", err)
	}
	sessionPath := filepath.Join(configDir, "session.json")
	os.Remove(sessionPath)
}

func TestLoadNonExistent(t *testing.T) {
	camera := hal.NewFakeCamera()

	// Ensure session file doesn't exist
	configDir, err := getConfigDir()
	if err != nil {
		t.Fatalf("failed to get config dir: %v", err)
	}
	sessionPath := filepath.Join(configDir, "session.json")
	os.Remove(sessionPath)

	// Try to load
	_, err = Load(camera)
	if err == nil {
		t.Error("Load() expected error for non-existent session, got nil")
	}
}

func TestLoadWithNilCamera(t *testing.T) {
	_, err := Load(nil)
	if err == nil {
		t.Error("Load() expected error for nil camera, got nil")
	}
}

func TestSaveCreatesConfigDir(t *testing.T) {
	camera := hal.NewFakeCamera()

	s := &Session{
		ProjectName:    "test_create_dir",
		OutputDir:      "/tmp/test",
		SequenceNumber: 1,
		camera:         camera,
	}

	// Get config dir path
	configDir, err := getConfigDir()
	if err != nil {
		t.Fatalf("failed to get config dir: %v", err)
	}

	// Remove config dir if it exists
	os.RemoveAll(configDir)

	// Save should create the directory
	if err := s.Save(); err != nil {
		t.Errorf("Save() error = %v", err)
	}

	// Verify directory exists
	if _, err := os.Stat(configDir); os.IsNotExist(err) {
		t.Error("Save() did not create config directory")
	}

	// Clean up
	sessionPath := filepath.Join(configDir, "session.json")
	os.Remove(sessionPath)
}

func TestJSONFormat(t *testing.T) {
	camera := hal.NewFakeCamera()

	s := &Session{
		ProjectName:    "json_test",
		OutputDir:      "/tmp/json_test",
		SequenceNumber: 10,
		camera:         camera,
	}

	// Save
	if err := s.Save(); err != nil {
		t.Fatalf("Save() error = %v", err)
	}

	// Read raw JSON
	configDir, err := getConfigDir()
	if err != nil {
		t.Fatalf("failed to get config dir: %v", err)
	}
	sessionPath := filepath.Join(configDir, "session.json")

	data, err := os.ReadFile(sessionPath)
	if err != nil {
		t.Fatalf("failed to read session file: %v", err)
	}

	// Parse JSON
	var parsed map[string]interface{}
	if err := json.Unmarshal(data, &parsed); err != nil {
		t.Fatalf("failed to parse JSON: %v", err)
	}

	// Verify expected fields
	if parsed["project_name"] != "json_test" {
		t.Errorf("project_name = %v, want json_test", parsed["project_name"])
	}

	if parsed["output_dir"] != "/tmp/json_test" {
		t.Errorf("output_dir = %v, want /tmp/json_test", parsed["output_dir"])
	}

	// sequence_number is parsed as float64 by json.Unmarshal into interface{}
	if seqNum, ok := parsed["sequence_number"].(float64); !ok || int(seqNum) != 10 {
		t.Errorf("sequence_number = %v, want 10", parsed["sequence_number"])
	}

	// Clean up
	os.Remove(sessionPath)
}
