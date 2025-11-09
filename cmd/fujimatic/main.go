package main

import (
	"bufio"
	"flag"
	"fmt"
	"os"
	"os/signal"
	"path/filepath"
	"strconv"
	"strings"
	"syscall"
	"time"

	"github.com/cfonseca/fujimatic/pkg/hal"
	"github.com/cfonseca/fujimatic/pkg/session"
)

type Shell struct {
	camera         hal.Camera
	session        *session.Session
	reader         *bufio.Reader
	interruptChan  chan os.Signal
	intervalActive bool // Track if intervalometer is running
	pauseReqChan   chan bool // Request pause from command
	stopReqChan    chan bool // Request stop from command
}

func NewShell(camera hal.Camera) *Shell {
	interruptChan := make(chan os.Signal, 1)
	return &Shell{
		camera:        camera,
		reader:        bufio.NewReader(os.Stdin),
		interruptChan: interruptChan,
		pauseReqChan:  make(chan bool, 1),
		stopReqChan:   make(chan bool, 1),
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

	// Set up Ctrl+C handling for the entire shell
	signal.Notify(s.interruptChan, os.Interrupt, syscall.SIGTERM)

	// Handle Ctrl+C in a separate goroutine
	go func() {
		for range s.interruptChan {
			fmt.Println() // Print newline after ^C
			if s.intervalActive {
				// Pause the active intervalometer
				fmt.Println("Ctrl+C detected - pausing intervalometer...")
				select {
				case s.pauseReqChan <- true:
					// Successfully sent pause request
				default:
					fmt.Println("Warning: could not send pause request")
				}
			} else {
				// Ask if user wants to exit
				fmt.Println("Press Ctrl+C again to exit, or press Enter to continue")
			}
		}
	}()

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
		return s.cmdCapture(args)
	case "pause":
		return s.cmdPause()
	case "resume":
		return s.cmdResume()
	case "stop":
		return s.cmdStop()
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
	fmt.Println("                         Shutter speeds (photographer-friendly):")
	fmt.Println("                         - Fractions: 1/30, 1/125, 1/250, 1/500, 1/1000, 1/2000, 1/4000")
	fmt.Println("                         - Decimals: 0.5, 1, 2, 4, 30 (in seconds)")
	fmt.Println("                         - Optional 's' suffix: 1/125s, 0.5s, 2s")
	fmt.Println("                         - Range: 1/8000s to 3600s (1 hour)")
	fmt.Println("                         - Auto-validates against camera's supported speeds")
	fmt.Println("  shutters              - List available shutter speeds (diagnostic)")
	fmt.Println("                         (Camera set to Manual exposure mode automatically)")
	fmt.Println()
	fmt.Println("Session Management:")
	fmt.Println("  session start <project> <outdir> - Start new capture session")
	fmt.Println("  session save         - Save current session state")
	fmt.Println("  session load         - Load saved session state")
	fmt.Println()
	fmt.Println("Capture:")
	fmt.Println("  capture              - Capture and download single image")
	fmt.Println("  capture <count>      - Capture multiple frames (count frames, no delay)")
	fmt.Println("  capture <count> <delay> - Capture with intervalometer (delay in seconds)")
	fmt.Println("                         Use count=0 for infinite captures")
	fmt.Println("  pause                - Pause active intervalometer (saves state)")
	fmt.Println("  resume               - Resume paused intervalometer")
	fmt.Println("  stop                 - Stop intervalometer (discards state)")
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

		// Show current camera settings
		if iso, err := s.camera.GetISO(); err == nil {
			fmt.Printf("  ISO: %d\n", iso)
		}

		if shutter, err := s.camera.GetShutter(); err == nil {
			seconds := float64(shutter) / 1000000.0
			fraction := getPhotographicFraction(seconds)
			if fraction != "" {
				fmt.Printf("  Shutter: %.6fs (%s)\n", seconds, fraction)
			} else {
				fmt.Printf("  Shutter: %.6fs\n", seconds)
			}
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
		fmt.Printf("  Next File: %s\n", s.session.GetNextFilename())

		// Intervalometer status (if active)
		if s.session.IntervalActive {
			fmt.Println()
			fmt.Println("  Intervalometer:")

			// Progress
			if s.session.IntervalTotalFrames == 0 {
				fmt.Printf("    Progress: Frame %d (infinite)\n", s.session.IntervalCurrentFrame)
			} else {
				percentage := float64(s.session.IntervalCurrentFrame) * 100.0 / float64(s.session.IntervalTotalFrames)
				fmt.Printf("    Progress: Frame %d/%d (%.0f%%)\n",
					s.session.IntervalCurrentFrame,
					s.session.IntervalTotalFrames,
					percentage)
			}

			// Delay
			delaySec := int(s.session.IntervalDelay.Seconds())
			fmt.Printf("    Delay: %ds between frames\n", delaySec)

			// Elapsed time
			elapsed := time.Since(s.session.IntervalStartTime)
			fmt.Printf("    Elapsed: %s\n", formatDuration(elapsed))

			// Integration time
			integrationTime := s.session.GetIntegrationTime()
			integrationDuration := time.Duration(integrationTime * float64(time.Second))
			fmt.Printf("    Integration time: %s (%d × %.3fs exposures)\n",
				formatDuration(integrationDuration),
				s.session.IntervalCurrentFrame,
				float64(s.session.LastShutterSpeed)/1000000.0)

			// Estimated remaining time (if not infinite)
			if s.session.IntervalTotalFrames > 0 && s.session.IntervalCurrentFrame > 0 {
				framesRemaining := s.session.IntervalTotalFrames - s.session.IntervalCurrentFrame
				avgTimePerFrame := elapsed.Seconds() / float64(s.session.IntervalCurrentFrame)
				estimatedRemaining := time.Duration(float64(framesRemaining)*avgTimePerFrame) * time.Second
				fmt.Printf("    Estimated remaining: %s\n", formatDuration(estimatedRemaining))
			}

			// Status
			if s.session.IntervalPaused {
				fmt.Println("    Status: Paused")
			} else {
				fmt.Println("    Status: Active")
			}
		}
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

func (s *Shell) cmdCapture(args []string) error {
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

	// Parse arguments: capture [count] [delay]
	var count int = 1 // Default: single capture
	var delay int = 0 // Default: no delay

	if len(args) >= 1 {
		var err error
		count, err = strconv.Atoi(args[0])
		if err != nil {
			return fmt.Errorf("invalid count argument: %s", args[0])
		}
		if count < 0 {
			return fmt.Errorf("count cannot be negative")
		}
	}

	if len(args) >= 2 {
		var err error
		delay, err = strconv.Atoi(args[1])
		if err != nil {
			return fmt.Errorf("invalid delay argument: %s", args[1])
		}
		if delay < 0 {
			return fmt.Errorf("delay cannot be negative")
		}
	}

	// Single capture mode (count=0 or count=1 with no args)
	if count == 1 && len(args) == 0 {
		return s.captureSingle()
	}

	// Intervalometer mode (count > 1 or count == 0 for infinite)
	// Run in background goroutine so REPL stays responsive
	go s.captureInterval(count, delay)

	fmt.Println("Intervalometer started in background")
	fmt.Println("Type 'status' to see progress, 'pause' to pause, 'stop' to end")

	return nil
}

func (s *Shell) captureSingle() error {
	filename := s.session.GetNextFilename()
	filepath := s.session.GetNextFilePath()
	fmt.Printf("Capturing %s...\n", filename)

	if err := s.session.Capture(); err != nil {
		return fmt.Errorf("capture failed: %w", err)
	}

	fmt.Printf("Captured and saved: %s\n", filepath)
	return nil
}

func (s *Shell) captureInterval(count int, delaySec int) {
	// Get current shutter speed for integration time tracking
	shutterSpeed, err := s.camera.GetShutter()
	if err != nil {
		fmt.Printf("\nError: failed to get shutter speed: %v\n> ", err)
		return
	}

	delayDuration := time.Duration(delaySec) * time.Second

	// Start intervalometer (only if not already active/resuming)
	if !s.session.IntervalActive {
		s.session.StartInterval(count, delayDuration, shutterSpeed)
		if count == 0 {
			fmt.Printf("\nStarting intervalometer: infinite frames, %ds delay\n", delaySec)
		} else {
			fmt.Printf("\nStarting intervalometer: %d frames, %ds delay\n", count, delaySec)
		}
	} else {
		// Resuming
		if count == 0 {
			fmt.Printf("\nResuming intervalometer: frame %d onwards (infinite), %ds delay\n",
				s.session.IntervalCurrentFrame+1, delaySec)
		} else {
			fmt.Printf("\nResuming intervalometer: frame %d/%d, %ds delay\n",
				s.session.IntervalCurrentFrame+1, count, delaySec)
		}
	}

	// Mark intervalometer as active
	s.intervalActive = true
	defer func() {
		s.intervalActive = false
	}()

	fmt.Print("Type 'pause' to pause, 'stop' to end, or press Ctrl+C to pause\n> ")

	// Main capture loop
	for {
		// Check for pause/stop requests from commands
		select {
		case <-s.pauseReqChan:
			s.session.PauseInterval()
			if err := s.session.Save(); err != nil {
				fmt.Printf("\nWarning: failed to save session: %v\n", err)
			}
			fmt.Print("\nIntervalometer paused - type 'resume' to continue\n> ")
			return

		case <-s.stopReqChan:
			s.session.ResetInterval()
			if err := s.session.Save(); err != nil {
				fmt.Printf("\nWarning: failed to save session: %v\n", err)
			}
			fmt.Print("\nIntervalometer stopped\n> ")
			return

		default:
			// Continue with capture
		}

		// Check if paused (by other means)
		if s.session.IntervalPaused {
			fmt.Print("\nIntervalometer paused\n> ")
			return
		}

		// Check if we've reached the count (if not infinite)
		if count > 0 && s.session.IntervalCurrentFrame >= count {
			fmt.Print("\n✓ Intervalometer completed!\n> ")
			s.session.ResetInterval()
			if err := s.session.Save(); err != nil {
				fmt.Printf("Warning: failed to save session: %v\n> ", err)
			}
			return
		}

		// Increment frame counter
		s.session.IntervalCurrentFrame++

		// Check battery before capture
		battery, err := s.camera.GetBattery()
		if err != nil {
			fmt.Printf("\nWarning: failed to check battery: %v\n> ", err)
		} else {
			// Auto-pause at 10% battery
			if battery <= 10 {
				fmt.Printf("\n⚠️  Battery at %d%% - AUTO-PAUSED\n", battery)
				fmt.Print("Please charge or replace battery, then type 'resume' to continue\n> ")
				s.session.PauseInterval()
				if err := s.session.Save(); err != nil {
					fmt.Printf("Warning: failed to save session: %v\n> ", err)
				}
				return
			}
		}

		// Display progress
		if count == 0 {
			fmt.Printf("\nFrame %d - ", s.session.IntervalCurrentFrame)
		} else {
			fmt.Printf("\nFrame %d/%d - ", s.session.IntervalCurrentFrame, count)
		}

		// Capture and download
		startTime := time.Now()
		filename := s.session.GetNextFilename()
		fmt.Printf("Capturing %s... ", filename)

		if err := s.session.Capture(); err != nil {
			fmt.Printf("Error: capture failed: %v\n> ", err)
			return
		}

		elapsed := time.Since(startTime)
		fmt.Printf("Done (%.1fs) - ", elapsed.Seconds())

		// Display battery level
		if battery, err := s.camera.GetBattery(); err == nil {
			fmt.Printf("Battery: %d%%", battery)
		}

		// Save session state periodically (every 5 frames)
		if s.session.IntervalCurrentFrame%5 == 0 {
			if err := s.session.Save(); err != nil {
				fmt.Printf(" - Warning: failed to save session: %v", err)
			}
		}

		// Wait for delay before next capture (if not the last frame)
		if delaySec > 0 && (count == 0 || s.session.IntervalCurrentFrame < count) {
			fmt.Printf(" - Next in %ds\n> ", delaySec)

			// Use select to handle delay and pause/stop commands
			delayTimer := time.NewTimer(delayDuration)
			select {
			case <-delayTimer.C:
				// Delay completed normally
			case <-s.pauseReqChan:
				delayTimer.Stop()
				s.session.PauseInterval()
				if err := s.session.Save(); err != nil {
					fmt.Printf("\nWarning: failed to save session: %v\n", err)
				}
				fmt.Print("\nIntervalometer paused - type 'resume' to continue\n> ")
				return
			case <-s.stopReqChan:
				delayTimer.Stop()
				s.session.ResetInterval()
				if err := s.session.Save(); err != nil {
					fmt.Printf("\nWarning: failed to save session: %v\n", err)
				}
				fmt.Print("\nIntervalometer stopped\n> ")
				return
			}
		} else {
			fmt.Print("\n> ")
		}
	}
}

func (s *Shell) cmdPause() error {
	if s.session == nil {
		return fmt.Errorf("no active session")
	}

	if !s.session.IntervalActive {
		return fmt.Errorf("no intervalometer active")
	}

	if s.session.IntervalPaused {
		fmt.Println("Intervalometer is already paused")
		return nil
	}

	if !s.intervalActive {
		return fmt.Errorf("intervalometer not running")
	}

	// Send pause request to the running interval goroutine
	select {
	case s.pauseReqChan <- true:
		fmt.Println("Pause request sent...")
	default:
		fmt.Println("Warning: pause channel full, trying again...")
		s.pauseReqChan <- true
	}

	return nil
}

func (s *Shell) cmdResume() error {
	if s.session == nil {
		return fmt.Errorf("no active session")
	}

	if !s.session.IntervalActive {
		return fmt.Errorf("no intervalometer active")
	}

	if !s.session.IntervalPaused {
		fmt.Println("Intervalometer is already running")
		return nil
	}

	s.session.ResumeInterval()

	// Restart the interval loop in background goroutine
	delaySec := int(s.session.IntervalDelay.Seconds())
	go s.captureInterval(s.session.IntervalTotalFrames, delaySec)

	return nil
}

func (s *Shell) cmdStop() error {
	if s.session == nil {
		return fmt.Errorf("no active session")
	}

	if !s.session.IntervalActive {
		return fmt.Errorf("no intervalometer active")
	}

	if !s.intervalActive {
		return fmt.Errorf("intervalometer not running")
	}

	// Send stop request to the running interval goroutine
	select {
	case s.stopReqChan <- true:
		fmt.Println("Stop request sent...")
	default:
		fmt.Println("Warning: stop channel full, trying again...")
		s.stopReqChan <- true
	}

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
		fraction := getPhotographicFraction(seconds)
		fmt.Printf("Current shutter: %.6f seconds (%d microseconds)", seconds, shutter)
		if fraction != "" {
			fmt.Printf(" = %s", fraction)
		}
		fmt.Println()

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

		// CRITICAL: Ensure camera is in Manual exposure mode for ISO control
		// Some cameras reject manual settings if still in auto mode
		fmt.Println("Setting camera to Manual exposure mode...")
		if err := s.camera.SetExposureMode(0x0001); err != nil {
			return fmt.Errorf("failed to set Manual exposure mode: %w", err)
		}
		fmt.Println("✅ Manual exposure mode set - full shutter/ISO control available")

		if err := s.camera.SetISO(isoValue); err != nil {
			return fmt.Errorf("failed to set ISO: %w", err)
		}

		fmt.Printf("ISO set to: %d\n", isoValue)

	case "shutter":
		shutterSeconds, err := parseShutterDuration(value)
		if err != nil {
			return fmt.Errorf("invalid shutter value: %w", err)
		}

		// CRITICAL: Ensure camera is in Manual exposure mode for shutter control
		// Some cameras reject manual settings if still in auto mode
		fmt.Println("Setting camera to Manual exposure mode...")
		if err := s.camera.SetExposureMode(0x0001); err != nil {
			return fmt.Errorf("failed to set Manual exposure mode: %w", err)
		}
		fmt.Println("✅ Manual exposure mode set - full shutter/ISO control available")

		// Get the list of supported shutter speeds from the camera
		supportedSpeeds, err := s.camera.GetSupportedShutterSpeeds()
		if err != nil {
			return fmt.Errorf("failed to get supported shutter speeds: %w", err)
		}

		if len(supportedSpeeds) == 0 {
			return fmt.Errorf("no supported shutter speeds found - camera may not be in Manual mode")
		}

		// Convert requested seconds to microseconds
		requestedMicroseconds := int(shutterSeconds * 1000000)
		fmt.Printf("Requested: %.6f seconds (%d microseconds)\n", shutterSeconds, requestedMicroseconds)

		// Find the closest supported shutter speed
		closestSpeed := findClosestShutterSpeed(requestedMicroseconds, supportedSpeeds)
		fmt.Printf("Using closest supported speed: %d microseconds (%.6f seconds)", closestSpeed, float64(closestSpeed)/1000000.0)

		actualFraction := getPhotographicFraction(float64(closestSpeed) / 1000000.0)
		if actualFraction != "" {
			fmt.Printf(" = %s", actualFraction)
		}
		fmt.Println()

		if err := s.camera.SetShutter(closestSpeed); err != nil {
			return fmt.Errorf("failed to set shutter: %w", err)
		}

		// Display result confirmation
		actualSeconds := float64(closestSpeed) / 1000000.0
		actualDisplayFraction := getPhotographicFraction(actualSeconds)
		fmt.Printf("Shutter set to: %.6f seconds (%d microseconds)", actualSeconds, closestSpeed)
		if actualDisplayFraction != "" {
			fmt.Printf(" = %s", actualDisplayFraction)
		}
		fmt.Println()

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

// findClosestShutterSpeed finds the closest supported shutter speed to the requested value
func findClosestShutterSpeed(requested int, supported []int) int {
	if len(supported) == 0 {
		return requested
	}

	closest := supported[0]
	minDiff := absInt(requested - closest)

	for _, speed := range supported {
		diff := absInt(requested - speed)
		if diff < minDiff {
			minDiff = diff
			closest = speed
		}
	}

	return closest
}

func absInt(x int) int {
	if x < 0 {
		return -x
	}
	return x
}

// getPhotographicFraction converts decimal seconds to a photographic fraction string
// e.g., 0.03125 -> "1/32s", 0.125 -> "1/8s", 0.5 -> "1/2s", 2.0 -> "2s"
func getPhotographicFraction(seconds float64) string {
	if seconds <= 0 {
		return ""
	}

	// If it's 1 second or less, express as 1/x
	if seconds <= 1.0 {
		denominator := 1.0 / seconds
		// Round to nearest integer
		rounded := int(denominator + 0.5)
		return fmt.Sprintf("1/%ds", rounded)
	} else {
		// Whole seconds
		return fmt.Sprintf("%.0fs", seconds)
	}
}

func parseShutterDuration(s string) (float64, error) {
	// Parse shutter duration and return as seconds (float64)
	// Supports both decimal and fraction formats:
	// - "0.5s" or "0.5" (decimal seconds)
	// - "1/30s" or "1/30" (photographic fractions)
	// - "1/4s" or "1/4" (fractional seconds)

	// Handle optional 's' suffix
	if strings.HasSuffix(s, "s") {
		s = s[:len(s)-1]
	}

	var seconds float64
	var err error

	// Check if this is a fraction format (contains '/')
	if strings.Contains(s, "/") {
		// Parse as fraction (e.g., "1/30" -> 1/30 = 0.0333...)
		parts := strings.Split(s, "/")
		if len(parts) != 2 {
			return 0, fmt.Errorf("invalid shutter duration format: %s", s)
		}

		// Parse numerator and denominator
		var numerator, denominator float64
		numerator, err = strconv.ParseFloat(strings.TrimSpace(parts[0]), 64)
		if err != nil {
			return 0, fmt.Errorf("invalid fraction numerator: %s", parts[0])
		}

		denominator, err = strconv.ParseFloat(strings.TrimSpace(parts[1]), 64)
		if err != nil {
			return 0, fmt.Errorf("invalid fraction denominator: %s", parts[1])
		}

		// Validate denominator
		if denominator <= 0 {
			return 0, fmt.Errorf("fraction denominator must be positive: %s", s)
		}

		// Calculate fraction value
		seconds = numerator / denominator
	} else {
		// Parse as decimal (e.g., "0.5" -> 0.5 seconds)
		seconds, err = strconv.ParseFloat(s, 64)
		if err != nil {
			return 0, fmt.Errorf("invalid shutter duration: %s", s)
		}
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

	// Ensure we're in Manual mode first
	fmt.Println("Setting camera to Manual exposure mode...")
	if err := s.camera.SetExposureMode(0x0001); err != nil {
		return fmt.Errorf("failed to set Manual exposure mode: %w", err)
	}
	fmt.Println("✅ Manual exposure mode set")

	// List shutter speeds using the existing diagnostic function
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

// formatDuration formats a duration in a human-readable format
// e.g., "0h 15m 32s" or "1h 52m 18s"
func formatDuration(d time.Duration) string {
	hours := int(d.Hours())
	minutes := int(d.Minutes()) % 60
	seconds := int(d.Seconds()) % 60

	return fmt.Sprintf("%dh %dm %ds", hours, minutes, seconds)
}
