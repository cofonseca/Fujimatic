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

	// Intervalometer state
	IntervalActive      bool          `json:"interval_active"`
	IntervalTotalFrames int           `json:"interval_total_frames"` // 0 = infinite
	IntervalCurrentFrame int          `json:"interval_current_frame"`
	IntervalDelay       time.Duration `json:"interval_delay"`        // Delay between frames
	IntervalPaused      bool          `json:"interval_paused"`
	IntervalStartTime   time.Time     `json:"interval_start_time"`
	LastShutterSpeed    int           `json:"last_shutter_speed"`    // In microseconds, for integration time calculation
	IntervalAsyncMode   bool          `json:"interval_async_mode"`   // Track if using async pipeline

	// Async pipeline state (not persisted to JSON)
	captureQueue chan int // Buffered channel for sequence numbers waiting to be downloaded
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

// GetIntegrationTime calculates total integration time (frames × shutter speed)
// Returns integration time in seconds
func (s *Session) GetIntegrationTime() float64 {
	if s.IntervalCurrentFrame == 0 || s.LastShutterSpeed == 0 {
		return 0.0
	}
	// Convert microseconds to seconds
	shutterSeconds := float64(s.LastShutterSpeed) / 1_000_000.0
	return float64(s.IntervalCurrentFrame) * shutterSeconds
}

// IsIntervalActive returns true if an intervalometer sequence is active
func (s *Session) IsIntervalActive() bool {
	return s.IntervalActive && !s.IntervalPaused
}

// ResetInterval clears all intervalometer state (for stop command)
func (s *Session) ResetInterval() {
	s.IntervalActive = false
	s.IntervalTotalFrames = 0
	s.IntervalCurrentFrame = 0
	s.IntervalDelay = 0
	s.IntervalPaused = false
	s.IntervalStartTime = time.Time{}
}

// StartInterval initializes the intervalometer state
func (s *Session) StartInterval(totalFrames int, delay time.Duration, shutterSpeed int, asyncMode bool) {
	s.IntervalActive = true
	s.IntervalTotalFrames = totalFrames
	s.IntervalCurrentFrame = 0
	s.IntervalDelay = delay
	s.IntervalPaused = false
	s.IntervalStartTime = time.Now()
	s.LastShutterSpeed = shutterSpeed
	s.IntervalAsyncMode = asyncMode
}

// PauseInterval pauses the intervalometer
func (s *Session) PauseInterval() {
	s.IntervalPaused = true
}

// ResumeInterval resumes the intervalometer
func (s *Session) ResumeInterval() {
	s.IntervalPaused = false
}

// StartAsyncPipeline initializes the capture queue channel for async pipeline mode
// bufferSize controls how many captures can queue ahead of downloads (2-3 recommended)
func (s *Session) StartAsyncPipeline(bufferSize int) {
	if bufferSize < 1 {
		bufferSize = 2 // Default: allow 2 captures ahead
	}
	s.captureQueue = make(chan int, bufferSize)
}

// StopAsyncPipeline closes the capture queue channel if it's still open
// Safe to call multiple times - recovers from panic if already closed
func (s *Session) StopAsyncPipeline() {
	if s.captureQueue != nil {
		defer func() {
			// Recover from panic if channel is already closed
			recover()
		}()
		close(s.captureQueue)
		s.captureQueue = nil
	}
}

// GetCaptureQueue returns the capture queue channel for async pipeline coordination
func (s *Session) GetCaptureQueue() chan int {
	return s.captureQueue
}

// TriggerCapture triggers the camera shutter only (no download)
// Returns the sequence number that was captured for later download
func (s *Session) TriggerCapture() (int, error) {
	if !s.camera.IsConnected() {
		return 0, fmt.Errorf("camera not connected")
	}

	// Ensure output directory exists
	if err := os.MkdirAll(s.OutputDir, 0755); err != nil {
		return 0, fmt.Errorf("failed to create output directory: %w", err)
	}

	// Check for file collision and skip to next available sequence number
	filePath := s.GetNextFilePath()
	for fileExists(filePath) {
		s.SequenceNumber++
		filePath = s.GetNextFilePath()
	}

	// Allocate sequence number BEFORE capture (maintains chronological order)
	capturedSeqNum := s.SequenceNumber
	s.SequenceNumber++

	// Trigger camera capture only
	if err := s.camera.Capture(); err != nil {
		return 0, fmt.Errorf("capture failed: %w", err)
	}

	return capturedSeqNum, nil
}

// DownloadCaptured downloads a previously captured image by sequence number
func (s *Session) DownloadCaptured(seqNum int) error {
	if !s.camera.IsConnected() {
		return fmt.Errorf("camera not connected")
	}

	filename := fmt.Sprintf("%s_%04d.RAF", s.ProjectName, seqNum)

	// Download the captured image
	if err := s.camera.DownloadLast(s.OutputDir, filename); err != nil {
		return fmt.Errorf("download failed: %w", err)
	}

	return nil
}
