package session

import (
	"encoding/json"
	"fmt"
	"os"
	"path/filepath"
	"time"

	"github.com/cfonseca/fujimatic/pkg/hal"
)

// Session manages a capture session with file naming and persistence
type Session struct {
	ProjectName    string    `json:"project_name"`
	OutputDir      string    `json:"output_dir"`
	SequenceNumber int       `json:"sequence_number"`
	CreatedAt      time.Time `json:"created_at"`
	camera         hal.Camera
}

// New creates a new capture session
func New(projectName, outputDir string, camera hal.Camera) (*Session, error) {
	if projectName == "" {
		return nil, fmt.Errorf("project name cannot be empty")
	}

	if outputDir == "" {
		return nil, fmt.Errorf("output directory cannot be empty")
	}

	if camera == nil {
		return nil, fmt.Errorf("camera cannot be nil")
	}

	return &Session{
		ProjectName:    projectName,
		OutputDir:      outputDir,
		SequenceNumber: 1,
		CreatedAt:      time.Now(),
		camera:         camera,
	}, nil
}

// GetNextFilename generates the next sequential filename
// Format: projectname_0001.RAF
func (s *Session) GetNextFilename() string {
	return fmt.Sprintf("%s_%04d.RAF", s.ProjectName, s.SequenceNumber)
}

// GetNextFilePath returns the full path for the next capture
func (s *Session) GetNextFilePath() string {
	return filepath.Join(s.OutputDir, s.GetNextFilename())
}

// Capture performs a synchronous capture and download operation
func (s *Session) Capture() error {
	if !s.camera.IsConnected() {
		return fmt.Errorf("camera not connected")
	}

	// Ensure output directory exists
	if err := os.MkdirAll(s.OutputDir, 0755); err != nil {
		return fmt.Errorf("failed to create output directory: %w", err)
	}

	// Check for file collision and skip to next available sequence number
	filePath := s.GetNextFilePath()
	for fileExists(filePath) {
		s.SequenceNumber++
		filePath = s.GetNextFilePath()
	}

	filename := s.GetNextFilename()

	// Trigger camera capture
	if err := s.camera.Capture(); err != nil {
		return fmt.Errorf("capture failed: %w", err)
	}

	// Download the captured image
	if err := s.camera.DownloadLast(s.OutputDir, filename); err != nil {
		return fmt.Errorf("download failed: %w", err)
	}

	// Increment sequence for next capture
	s.SequenceNumber++

	return nil
}

// Save persists the session state to a JSON file in the user config directory
func (s *Session) Save() error {
	configDir, err := getConfigDir()
	if err != nil {
		return fmt.Errorf("failed to get config directory: %w", err)
	}

	if err := os.MkdirAll(configDir, 0755); err != nil {
		return fmt.Errorf("failed to create config directory: %w", err)
	}

	sessionPath := filepath.Join(configDir, "session.json")

	data, err := json.MarshalIndent(s, "", "  ")
	if err != nil {
		return fmt.Errorf("failed to marshal session: %w", err)
	}

	if err := os.WriteFile(sessionPath, data, 0644); err != nil {
		return fmt.Errorf("failed to write session file: %w", err)
	}

	return nil
}

// Load restores a session from the user config directory
func Load(camera hal.Camera) (*Session, error) {
	if camera == nil {
		return nil, fmt.Errorf("camera cannot be nil")
	}

	configDir, err := getConfigDir()
	if err != nil {
		return nil, fmt.Errorf("failed to get config directory: %w", err)
	}

	sessionPath := filepath.Join(configDir, "session.json")

	data, err := os.ReadFile(sessionPath)
	if err != nil {
		return nil, fmt.Errorf("failed to read session file: %w", err)
	}

	var s Session
	if err := json.Unmarshal(data, &s); err != nil {
		return nil, fmt.Errorf("failed to unmarshal session: %w", err)
	}

	s.camera = camera

	return &s, nil
}

// getConfigDir returns the OS-specific user configuration directory
func getConfigDir() (string, error) {
	homeDir, err := os.UserHomeDir()
	if err != nil {
		return "", err
	}

	// Windows: %USERPROFILE%\.config\fujimatic
	// Linux/Mac: ~/.config/fujimatic
	return filepath.Join(homeDir, ".config", "fujimatic"), nil
}

// fileExists checks if a file exists at the given path
func fileExists(path string) bool {
	_, err := os.Stat(path)
	return err == nil
}
