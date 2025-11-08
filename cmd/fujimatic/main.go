package main

import (
	"bufio"
	"flag"
	"fmt"
	"os"
	"path/filepath"
	"strconv"
	"strings"

	"github.com/cfonseca/fujimatic/pkg/hal"
	"github.com/cfonseca/fujimatic/pkg/session"
)

type Shell struct {
	camera  hal.Camera
	session *session.Session
	reader  *bufio.Reader
}

func NewShell(camera hal.Camera) *Shell {
	return &Shell{
		camera: camera,
		reader: bufio.NewReader(os.Stdin),
	}
}

func main() {
	// Parse command line flags
	fakeCamera := flag.Bool("fake-camera", false, "Use fake camera for testing/debugging (default: use real camera)")
	flag.Parse()

	var camera hal.Camera

	if *fakeCamera {
		// Use fake camera for testing
		fmt.Println("Using fake camera (testing mode)")
		camera = hal.NewFakeCamera()
	} else {
		// Use real camera (production mode)
		// Initialize SDK first
		sdkPath := os.Getenv("FUJI_SDK_PATH")
		if sdkPath == "" {
			fmt.Println("Error: FUJI_SDK_PATH environment variable not set")
			fmt.Println("Please set it to the path of the Fujifilm SDK redistributables directory")
			fmt.Println("Example (Windows): setx FUJI_SDK_PATH \"C:\\Path\\To\\sdk\\REDISTRIBUTABLES\"")
			fmt.Println()
			fmt.Println("For testing without hardware, use: fujimatic --fake-camera")
			os.Exit(1)
		}

		fmt.Printf("Initializing Fujifilm SDK from: %s\n", sdkPath)
		result, err := hal.InitSDK(sdkPath)
		if err != nil {
			fmt.Printf("Error: Failed to initialize SDK: %v\n", err)
			fmt.Println()
			fmt.Println("For testing without hardware, use: fujimatic --fake-camera")
			os.Exit(1)
		}

		if result != hal.InitSuccess {
			fmt.Printf("Error: SDK initialization returned: %v\n", result)
			fmt.Println()
			fmt.Println("For testing without hardware, use: fujimatic --fake-camera")
			os.Exit(1)
		}

		fmt.Println("SDK initialized successfully")
		camera = hal.NewDefaultCamera()
	}

	shell := NewShell(camera)
	shell.Run()
}

func (s *Shell) Run() {
	fmt.Println("Fujimatic Camera Control CLI")
	fmt.Println("Type 'help' for available commands")
	fmt.Println()

	for {
		fmt.Print("> ")
		line, err := s.reader.ReadString('\n')
		if err != nil {
			fmt.Println("Error reading input:", err)
			return
		}

		line = strings.TrimSpace(line)
		if line == "" {
			continue
		}

		parts := strings.Fields(line)
		if len(parts) == 0 {
			continue
		}

		cmd := parts[0]
		args := parts[1:]

		if err := s.handleCommand(cmd, args); err != nil {
			fmt.Println("Error:", err)
		}
	}
}

func (s *Shell) handleCommand(cmd string, args []string) error {
	switch cmd {
	case "help":
		return s.cmdHelp()
	case "connect":
		return s.cmdConnect()
	case "disconnect":
		return s.cmdDisconnect()
	case "status":
		return s.cmdStatus()
	case "battery":
		return s.cmdBattery()
	case "get":
		return s.cmdGet(args)
	case "set":
		return s.cmdSet(args)
	case "shutters":
		return s.cmdListShutterSpeeds()
	case "session":
		return s.cmdSession(args)
	case "capture":
		return s.cmdCapture()
	case "exit", "quit":
		return s.cmdExit()
	default:
		fmt.Printf("Unknown command: %s (type 'help' for available commands)\n", cmd)
		return nil
	}
}

func (s *Shell) cmdHelp() error {
	fmt.Println("Available commands:")
	fmt.Println()
	fmt.Println("Camera Control:")
	fmt.Println("  connect              - Connect to camera")
	fmt.Println("  disconnect           - Disconnect from camera")
	fmt.Println("  status               - Show camera and session status")
	fmt.Println("  battery              - Show battery level")
	fmt.Println("  get <iso|shutter>    - Get current camera setting")
	fmt.Println("  set <iso|shutter> <value> - Set camera setting")
	fmt.Println("                         (shutter: 0.000125s to 3600s, e.g. '0.125s' or '2s')")
	fmt.Println("  shutters              - List available shutter speeds (diagnostic)")
	fmt.Println("                         (Camera set to Manual exposure mode automatically)")
	fmt.Println()
	fmt.Println("Session Management:")
	fmt.Println("  session start <project> <outdir> - Start new capture session")
	fmt.Println("  session save         - Save current session state")
	fmt.Println("  session load         - Load saved session state")
	fmt.Println()
	fmt.Println("Capture:")
	fmt.Println("  capture              - Capture and download image")
	fmt.Println()
	fmt.Println("Utility:")
	fmt.Println("  help                 - Show this help message")
	fmt.Println("  exit, quit           - Exit the shell")
	fmt.Println()
	return nil
}

func (s *Shell) cmdConnect() error {
	if s.camera.IsConnected() {
		fmt.Println("Camera already connected")
		return nil
	}

	fmt.Println("Connecting to camera...")
	if err := s.camera.Connect(); err != nil {
		return fmt.Errorf("failed to connect: %w", err)
	}

	fmt.Println("Camera connected successfully")

	return nil
}

func (s *Shell) cmdDisconnect() error {
	if !s.camera.IsConnected() {
		fmt.Println("Camera not connected")
		return nil
	}

	fmt.Println("Disconnecting from camera...")
	if err := s.camera.Disconnect(); err != nil {
		return fmt.Errorf("failed to disconnect: %w", err)
	}

	fmt.Println("Camera disconnected successfully")

	return nil
}

func (s *Shell) cmdStatus() error {
	fmt.Println("Status:")
	fmt.Println()

	// Camera status
	if s.camera.IsConnected() {
		fmt.Println("  Camera: Connected")

		battery, err := s.camera.GetBattery()
		if err != nil {
			fmt.Printf("  Battery: Error - %v\n", err)
		} else {
			fmt.Printf("  Battery: %d%%\n", battery)
		}
	} else {
		fmt.Println("  Camera: Not connected")
	}

	fmt.Println()

	// Session status
	if s.session != nil {
		fmt.Println("  Session: Active")
		fmt.Printf("  Project: %s\n", s.session.ProjectName)
		fmt.Printf("  Output Directory: %s\n", s.session.OutputDir)
		fmt.Printf("  Next Sequence: %04d\n", s.session.SequenceNumber)
	} else {
		fmt.Println("  Session: None")
	}

	fmt.Println()

	return nil
}

func (s *Shell) cmdBattery() error {
	if !s.camera.IsConnected() {
		return fmt.Errorf("camera not connected")
	}

	battery, err := s.camera.GetBattery()
	if err != nil {
		return fmt.Errorf("failed to get battery: %w", err)
	}

	fmt.Printf("Battery: %d%%\n", battery)

	return nil
}

func (s *Shell) cmdSession(args []string) error {
	if len(args) == 0 {
		fmt.Println("Usage:")
		fmt.Println("  session start <project> <outdir>")
		fmt.Println("  session save")
		fmt.Println("  session load")
		return nil
	}

	subCmd := args[0]

	switch subCmd {
	case "start":
		return s.cmdSessionStart(args[1:])
	case "save":
		return s.cmdSessionSave()
	case "load":
		return s.cmdSessionLoad()
	default:
		return fmt.Errorf("unknown session command: %s", subCmd)
	}
}

func (s *Shell) cmdSessionStart(args []string) error {
	if len(args) < 2 {
		return fmt.Errorf("usage: session start <project> <outdir>")
	}

	projectName := args[0]
	outputDir := args[1]

	// Expand ~ to home directory
	if strings.HasPrefix(outputDir, "~") {
		home, err := os.UserHomeDir()
		if err != nil {
			return fmt.Errorf("failed to get home directory: %w", err)
		}
		outputDir = filepath.Join(home, outputDir[1:])
	}

	newSession, err := session.New(projectName, outputDir, s.camera)
	if err != nil {
		return fmt.Errorf("failed to create session: %w", err)
	}

	s.session = newSession
	fmt.Printf("Session started: %s\n", projectName)
	fmt.Printf("Output directory: %s\n", outputDir)
	fmt.Printf("Next file: %s\n", s.session.GetNextFilename())

	return nil
}

func (s *Shell) cmdSessionSave() error {
	if s.session == nil {
		return fmt.Errorf("no active session to save")
	}

	if err := s.session.Save(); err != nil {
		return fmt.Errorf("failed to save session: %w", err)
	}

	fmt.Println("Session saved successfully")

	return nil
}

func (s *Shell) cmdSessionLoad() error {
	loadedSession, err := session.Load(s.camera)
	if err != nil {
		return fmt.Errorf("failed to load session: %w", err)
	}

	s.session = loadedSession
	fmt.Printf("Session loaded: %s\n", s.session.ProjectName)
	fmt.Printf("Output directory: %s\n", s.session.OutputDir)
	fmt.Printf("Next sequence: %04d\n", s.session.SequenceNumber)

	return nil
}

func (s *Shell) cmdCapture() error {
	if !s.camera.IsConnected() {
		return fmt.Errorf("camera not connected - use 'connect' first")
	}

	// Auto-create session if none exists
	if s.session == nil {
		fmt.Println("No active session. Creating new session...")
		fmt.Print("Project name: ")

		projectName, err := s.reader.ReadString('\n')
		if err != nil {
			return fmt.Errorf("failed to read project name: %w", err)
		}
		projectName = strings.TrimSpace(projectName)

		if projectName == "" {
			return fmt.Errorf("project name cannot be empty")
		}

		// Use default output directory (current directory + captures)
		outputDir := filepath.Join(".", "captures")

		newSession, err := session.New(projectName, outputDir, s.camera)
		if err != nil {
			return fmt.Errorf("failed to create session: %w", err)
		}

		s.session = newSession
		fmt.Printf("Session created: %s -> %s\n", projectName, outputDir)
	}

	filename := s.session.GetNextFilename()
	filepath := s.session.GetNextFilePath()
	fmt.Printf("Capturing %s...\n", filename)

	if err := s.session.Capture(); err != nil {
		return fmt.Errorf("capture failed: %w", err)
	}

	fmt.Printf("Captured and saved: %s\n", filepath)

	return nil
}

func (s *Shell) cmdGet(args []string) error {
	if !s.camera.IsConnected() {
		return fmt.Errorf("camera not connected - use 'connect' first")
	}

	if len(args) == 0 {
		return fmt.Errorf("usage: get <iso|shutter>")
	}

	param := args[0]

	switch param {
	case "iso":
		iso, err := s.camera.GetISO()
		if err != nil {
			return fmt.Errorf("failed to get ISO: %w", err)
		}
		fmt.Printf("Current ISO: %d\n", iso)

	case "shutter":
		shutter, err := s.camera.GetShutter()
		if err != nil {
			return fmt.Errorf("failed to get shutter: %w", err)
		}
		// Convert microseconds to seconds for display
		seconds := float64(shutter) / 1000000.0
		fmt.Printf("Current shutter: %.6f seconds (%d microseconds)\n", seconds, shutter)

	default:
		return fmt.Errorf("unknown parameter: %s (use 'iso' or 'shutter')", param)
	}

	return nil
}

func (s *Shell) cmdSet(args []string) error {
	if !s.camera.IsConnected() {
		return fmt.Errorf("camera not connected - use 'connect' first")
	}

	if len(args) < 2 {
		return fmt.Errorf("usage: set <iso|shutter> <value>")
	}

	param := args[0]
	value := args[1]

	switch param {
	case "iso":
		isoValue, err := parseInt(value, 100, 12800)
		if err != nil {
			return fmt.Errorf("invalid ISO value: %w", err)
		}

		if err := s.camera.SetISO(isoValue); err != nil {
			return fmt.Errorf("failed to set ISO: %w", err)
		}

		fmt.Printf("ISO set to: %d\n", isoValue)

	case "shutter":
		shutterSeconds, err := parseShutterDuration(value)
		if err != nil {
			return fmt.Errorf("invalid shutter value: %w", err)
		}

		// Convert seconds to microseconds for camera
		shutterMicroseconds := int(shutterSeconds * 1000000)

		if err := s.camera.SetShutter(shutterMicroseconds); err != nil {
			return fmt.Errorf("failed to set shutter: %w", err)
		}

		fmt.Printf("Shutter set to: %.6f seconds (%d microseconds)\n", shutterSeconds, shutterMicroseconds)

	default:
		return fmt.Errorf("unknown parameter: %s (use 'iso' or 'shutter')", param)
	}

	return nil
}

func parseInt(s string, min, max int) (int, error) {
	value, err := parseInt64(s, int64(min), int64(max))
	if err != nil {
		return 0, err
	}
	return int(value), nil
}

func parseInt64(s string, min, max int64) (int64, error) {
	value, err := parseIntToInt64(s)
	if err != nil {
		return 0, err
	}

	if value < min || value > max {
		return 0, fmt.Errorf("value %d out of range [%d, %d]", value, min, max)
	}

	return value, nil
}

func parseIntToInt64(s string) (int64, error) {
	// Handle time duration formats for shutter speed
	if strings.HasSuffix(s, "s") {
		durationStr := s[:len(s)-1]
		duration, err := parseDuration(durationStr)
		if err != nil {
			return 0, err
		}
		return duration, nil
	}

	// Parse as plain integer
	value, err := strconv.ParseInt(s, 10, 64)
	if err != nil {
		return 0, fmt.Errorf("invalid integer: %s", s)
	}

	return value, nil
}

func parseDuration(s string) (int64, error) {
	// Parse time duration string like "0.5" (seconds) and convert to microseconds
	seconds, err := strconv.ParseFloat(s, 64)
	if err != nil {
		return 0, fmt.Errorf("invalid duration: %s", s)
	}

	if seconds < 0 {
		return 0, fmt.Errorf("duration cannot be negative")
	}

	// Convert seconds to microseconds
	microseconds := int64(seconds * 1000000)
	if microseconds < 125 { // 1/8000 second minimum
		return 0, fmt.Errorf("duration too short (minimum: 1/8000 second = 125 microseconds)")
	}

	return microseconds, nil
}

func parseShutterDuration(s string) (float64, error) {
	// Parse shutter duration and return as seconds (float64)
	// Handle optional 's' suffix (e.g., "0.5s" -> "0.5")
	if strings.HasSuffix(s, "s") {
		s = s[:len(s)-1]
	}

	seconds, err := strconv.ParseFloat(s, 64)
	if err != nil {
		return 0, fmt.Errorf("invalid shutter duration: %s", s)
	}

	if seconds < 0 {
		return 0, fmt.Errorf("shutter duration cannot be negative")
	}

	// Validate shutter speed range (1/8000s to 1 hour)
	if seconds < 0.000125 { // 1/8000 second
		return 0, fmt.Errorf("shutter too fast (minimum: 1/8000 second = 0.000125 seconds)")
	}

	if seconds > 3600 { // 1 hour maximum
		return 0, fmt.Errorf("shutter too slow (maximum: 3600 seconds = 1 hour)")
	}

	return seconds, nil
}

func (s *Shell) cmdListShutterSpeeds() error {
	if !s.camera.IsConnected() {
		return fmt.Errorf("camera not connected - use 'connect' first")
	}

	if err := s.camera.ListShutterSpeeds(); err != nil {
		return fmt.Errorf("failed to list shutter speeds: %w", err)
	}

	return nil
}

func (s *Shell) cmdExit() error {
	// Disconnect camera if connected
	if s.camera.IsConnected() {
		fmt.Println("Disconnecting camera...")
		if err := s.camera.Disconnect(); err != nil {
			fmt.Printf("Warning: failed to disconnect: %v\n", err)
		}
	}

	fmt.Println("Exiting...")
	os.Exit(0)

	return nil
}
