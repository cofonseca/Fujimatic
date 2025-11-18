package api

import (
	"encoding/json"
	"fmt"
	"net/http"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"time"

	"github.com/cfonseca/fujimatic/pkg/converter"
	"github.com/cfonseca/fujimatic/pkg/logger"
	"github.com/cfonseca/fujimatic/pkg/session"
)

// Helper functions

func (s *Server) sendJSON(w http.ResponseWriter, statusCode int, data interface{}) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(statusCode)
	if err := json.NewEncoder(w).Encode(data); err != nil {
		logger.Error("Failed to encode JSON response: %v", err)
	}
}

func (s *Server) sendError(w http.ResponseWriter, statusCode int, message string) {
	s.sendJSON(w, statusCode, ErrorResponse{
		Error:   http.StatusText(statusCode),
		Message: message,
		Code:    statusCode,
	})
}

func (s *Server) parseJSON(r *http.Request, v interface{}) error {
	return json.NewDecoder(r.Body).Decode(v)
}

// Camera Control Handlers

func (s *Server) handleCameraConnect(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		s.sendError(w, http.StatusMethodNotAllowed, "Method not allowed")
		return
	}

	camera := s.state.GetCamera()

	// Check if already connected
	if camera.IsConnected() {
		s.sendJSON(w, http.StatusOK, ConnectResponse{
			Status:      "already_connected",
			StatusCode:  200,
			Battery:     0,
		})
		return
	}

	// Connect to camera
	if err := camera.Connect(); err != nil {
		s.sendError(w, http.StatusInternalServerError, fmt.Sprintf("Failed to connect: %v", err))
		return
	}

	// Get battery level
	battery, err := camera.GetBattery()
	if err != nil {
		battery = 0
	}

	s.sendJSON(w, http.StatusOK, ConnectResponse{
		Status:      "connected",
		StatusCode:  200,
		Battery:     battery,
	})
}

func (s *Server) handleCameraDisconnect(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		s.sendError(w, http.StatusMethodNotAllowed, "Method not allowed")
		return
	}

	camera := s.state.GetCamera()

	// Check if not connected
	if !camera.IsConnected() {
		s.sendJSON(w, http.StatusOK, DisconnectResponse{
			Status:     "already_disconnected",
			StatusCode: 200,
		})
		return
	}

	// Auto-stop live view if active (required for proper camera disconnect)
	active, err := camera.IsLiveViewActive()
	if err == nil && active {
		logger.Info("Auto-stopping live view before disconnect...")
		if err := camera.StopLiveView(); err != nil {
			logger.Error("Failed to stop live view: %v", err)
			// Continue with disconnect anyway
		} else {
			logger.Info("Live view stopped successfully")
		}
	}

	// Disconnect from camera
	if err := camera.Disconnect(); err != nil {
		s.sendError(w, http.StatusInternalServerError, fmt.Sprintf("Failed to disconnect: %v", err))
		return
	}

	s.sendJSON(w, http.StatusOK, DisconnectResponse{
		Status:     "disconnected",
		StatusCode: 200,
	})
}

func (s *Server) handleCameraStatus(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodGet {
		s.sendError(w, http.StatusMethodNotAllowed, "Method not allowed")
		return
	}

	camera := s.state.GetCamera()
	connected := camera.IsConnected()

	response := CameraStatusResponse{
		Connected: connected,
	}

	if connected {
		// Get battery level
		if battery, err := camera.GetBattery(); err == nil {
			response.Battery = battery
		}
		// Model is hardcoded for now (could be extended later)
		response.Model = "X-T3"
	}

	s.sendJSON(w, http.StatusOK, response)
}

func (s *Server) handleCameraBattery(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodGet {
		s.sendError(w, http.StatusMethodNotAllowed, "Method not allowed")
		return
	}

	camera := s.state.GetCamera()

	if !camera.IsConnected() {
		s.sendError(w, http.StatusServiceUnavailable, "Camera not connected")
		return
	}

	battery, err := camera.GetBattery()
	if err != nil {
		s.sendError(w, http.StatusInternalServerError, fmt.Sprintf("Failed to get battery: %v", err))
		return
	}

	s.sendJSON(w, http.StatusOK, BatteryResponse{
		Battery:  battery,
		Charging: false, // SDK doesn't provide charging status
	})
}

// Settings Handlers

func (s *Server) handleSettingsISO(w http.ResponseWriter, r *http.Request) {
	camera := s.state.GetCamera()

	if !camera.IsConnected() {
		s.sendError(w, http.StatusServiceUnavailable, "Camera not connected")
		return
	}

	switch r.Method {
	case http.MethodGet:
		iso, err := camera.GetISO()
		if err != nil {
			s.sendError(w, http.StatusInternalServerError, fmt.Sprintf("Failed to get ISO: %v", err))
			return
		}
		s.sendJSON(w, http.StatusOK, ISOGetResponse{
			ISO:        iso,
			Status:     "ok",
			StatusCode: 200,
		})

	case http.MethodPost:
		var req ISOSetRequest
		if err := s.parseJSON(r, &req); err != nil {
			s.sendError(w, http.StatusBadRequest, fmt.Sprintf("Invalid request: %v", err))
			return
		}

		// Auto-stop live view if active (required for ISO changes)
		var liveViewWasActive bool
		active, err := camera.IsLiveViewActive()
		if err == nil && active {
			liveViewWasActive = true
			logger.Info("Auto-stopping live view before ISO change...")
			if err := camera.StopLiveView(); err != nil {
				logger.Error("Failed to stop live view: %v", err)
				// Continue with ISO change anyway
			} else {
				logger.Info("Live view stopped successfully")
				// Small delay to ensure live view is fully stopped before proceeding
				time.Sleep(100 * time.Millisecond)
			}
		}

		if err := camera.SetISO(req.ISO); err != nil {
			s.sendError(w, http.StatusInternalServerError, fmt.Sprintf("Failed to set ISO: %v", err))
			return
		}

		// Auto-restart live view if it was active before
		if liveViewWasActive {
			logger.Info("Auto-restarting live view after ISO change...")
			if err := camera.StartLiveView(); err != nil {
				logger.Error("Failed to restart live view after ISO change: %v", err)
				// Don't fail the ISO change - just log the error
			} else {
				logger.Info("Live view restarted successfully")
			}
		}

		s.sendJSON(w, http.StatusOK, ISOSetResponse{
			ISO:        req.ISO,
			Status:     "ok",
			StatusCode: 200,
		})

	default:
		s.sendError(w, http.StatusMethodNotAllowed, "Method not allowed")
	}
}

func (s *Server) handleSettingsShutter(w http.ResponseWriter, r *http.Request) {
	camera := s.state.GetCamera()

	if !camera.IsConnected() {
		s.sendError(w, http.StatusServiceUnavailable, "Camera not connected")
		return
	}

	switch r.Method {
	case http.MethodGet:
		shutterUS, err := camera.GetShutter()
		if err != nil {
			s.sendError(w, http.StatusInternalServerError, fmt.Sprintf("Failed to get shutter: %v", err))
			return
		}

		// Format for display
		shutterDisplay := formatShutterSpeed(shutterUS)

		s.sendJSON(w, http.StatusOK, ShutterGetResponse{
			ShutterSpeed: shutterDisplay,
			Status:       "ok",
			StatusCode:   200,
		})

	case http.MethodPost:
		var req ShutterSetRequest
		if err := s.parseJSON(r, &req); err != nil {
			s.sendError(w, http.StatusBadRequest, fmt.Sprintf("Invalid request: %v", err))
			return
		}

		var requestedUS int
		if req.ShutterUS > 0 {
			requestedUS = req.ShutterUS
		} else if req.Shutter != "" {
			// Parse shutter string (e.g., "1/250", "2s")
			parsed, err := parseShutterSpeed(req.Shutter)
			if err != nil {
				s.sendError(w, http.StatusBadRequest, fmt.Sprintf("Invalid shutter value: %v", err))
				return
			}
			requestedUS = parsed
		} else {
			s.sendError(w, http.StatusBadRequest, "Either 'shutter' or 'shutter_us' must be provided")
			return
		}

		// Auto-stop live view if active (required for shutter changes)
		var liveViewWasActive bool
		active, err := camera.IsLiveViewActive()
		if err == nil && active {
			liveViewWasActive = true
			logger.Info("Auto-stopping live view before shutter change...")
			if err := camera.StopLiveView(); err != nil {
				logger.Error("Failed to stop live view: %v", err)
				// Continue with shutter change anyway
			} else {
				logger.Info("Live view stopped successfully")
				// Small delay to ensure live view is fully stopped before proceeding
				time.Sleep(100 * time.Millisecond)
			}
		}

		// Get supported shutter speeds and find closest match
		supported, err := camera.GetSupportedShutterSpeeds()
		if err != nil || len(supported) == 0 {
			// If we can't get supported speeds, try setting directly
			logger.Error("Warning: Could not get supported shutter speeds, trying direct set")
			if err := camera.SetShutter(requestedUS); err != nil {
				s.sendError(w, http.StatusInternalServerError, fmt.Sprintf("Failed to set shutter: %v", err))
				return
			}

			// Auto-restart live view if it was active before
			if liveViewWasActive {
				logger.Info("Auto-restarting live view after shutter change...")
				if err := camera.StartLiveView(); err != nil {
					logger.Error("Failed to restart live view after shutter change: %v", err)
					// Don't fail the shutter change - just log the error
				} else {
					logger.Info("Live view restarted successfully")
				}
			}

			s.sendJSON(w, http.StatusOK, ShutterSetResponse{
				ShutterSpeed: formatShutterSpeed(requestedUS),
				Status:       "ok",
				StatusCode:   200,
			})
			return
		}

		// Find closest supported speed
		closestUS := findClosestShutterSpeed(requestedUS, supported)

		// Set the closest supported speed
		if err := camera.SetShutter(closestUS); err != nil {
			s.sendError(w, http.StatusInternalServerError, fmt.Sprintf("Failed to set shutter: %v", err))
			return
		}

		// Auto-restart live view if it was active before
		if liveViewWasActive {
			logger.Info("Auto-restarting live view after shutter change...")
			if err := camera.StartLiveView(); err != nil {
				logger.Error("Failed to restart live view after shutter change: %v", err)
				// Don't fail the shutter change - just log the error
			} else {
				logger.Info("Live view restarted successfully")
			}
		}

		// Return the actual speed that was set
		actualDisplay := formatShutterSpeed(closestUS)
		s.sendJSON(w, http.StatusOK, ShutterSetResponse{
			ShutterSpeed: actualDisplay,
			Status:       "ok",
			StatusCode:   200,
		})

	default:
		s.sendError(w, http.StatusMethodNotAllowed, "Method not allowed")
	}
}

func (s *Server) handleSettingsFocus(w http.ResponseWriter, r *http.Request) {
	camera := s.state.GetCamera()

	if !camera.IsConnected() {
		s.sendError(w, http.StatusServiceUnavailable, "Camera not connected")
		return
	}

	switch r.Method {
	case http.MethodGet:
		mode, err := camera.GetFocusMode()
		if err != nil {
			s.sendError(w, http.StatusInternalServerError, fmt.Sprintf("Failed to get focus mode: %v", err))
			return
		}
		// Convert int mode to string
		modeStr := focusModeToString(mode)
		s.sendJSON(w, http.StatusOK, FocusGetResponse{
			FocusMode:  modeStr,
			Status:     "ok",
			StatusCode: 200,
		})

	case http.MethodPost:
		var req FocusSetRequest
		if err := s.parseJSON(r, &req); err != nil {
			s.sendError(w, http.StatusBadRequest, fmt.Sprintf("Invalid request: %v", err))
			return
		}

		// Convert string mode to int
		modeInt, err := focusModeFromString(req.FocusMode)
		if err != nil {
			s.sendError(w, http.StatusBadRequest, fmt.Sprintf("Invalid focus mode: %v", err))
			return
		}

		// Auto-stop live view if active (required for focus mode changes)
		var liveViewWasActive bool
		active, err := camera.IsLiveViewActive()
		if err == nil && active {
			liveViewWasActive = true
			logger.Info("Auto-stopping live view before focus mode change...")
			if err := camera.StopLiveView(); err != nil {
				logger.Error("Failed to stop live view: %v", err)
				// Continue with focus mode change anyway
			} else {
				logger.Info("Live view stopped successfully")
				// Small delay to ensure live view is fully stopped before proceeding
				time.Sleep(100 * time.Millisecond)
			}
		}

		if err := camera.SetFocusMode(modeInt); err != nil {
			s.sendError(w, http.StatusInternalServerError, fmt.Sprintf("Failed to set focus mode: %v", err))
			return
		}

		// Auto-restart live view if it was active before
		if liveViewWasActive {
			logger.Info("Auto-restarting live view after focus mode change...")
			if err := camera.StartLiveView(); err != nil {
				logger.Error("Failed to restart live view: %v", err)
				// Don't fail the focus mode change - just log the error
			} else {
				logger.Info("Live view restarted successfully")
			}
		}

		s.sendJSON(w, http.StatusOK, FocusSetResponse{
			FocusMode:  req.FocusMode,
			Status:     "ok",
			StatusCode: 200,
		})

	default:
		s.sendError(w, http.StatusMethodNotAllowed, "Method not allowed")
	}
}

func (s *Server) handleSettingsQuality(w http.ResponseWriter, r *http.Request) {
	camera := s.state.GetCamera()

	if !camera.IsConnected() {
		s.sendError(w, http.StatusServiceUnavailable, "Camera not connected")
		return
	}

	switch r.Method {
	case http.MethodGet:
		quality, err := camera.GetImageQuality()
		if err != nil {
			s.sendError(w, http.StatusInternalServerError, fmt.Sprintf("Failed to get image quality: %v", err))
			return
		}
		// Convert int quality to string
		qualityStr := imageQualityToString(quality)
		s.sendJSON(w, http.StatusOK, QualityGetResponse{
			Quality:    qualityStr,
			Status:     "ok",
			StatusCode: 200,
		})

	case http.MethodPost:
		var req QualitySetRequest
		if err := s.parseJSON(r, &req); err != nil {
			s.sendError(w, http.StatusBadRequest, fmt.Sprintf("Invalid request: %v", err))
			return
		}

		// Convert string quality to int
		qualityInt, err := imageQualityFromString(req.Quality)
		if err != nil {
			s.sendError(w, http.StatusBadRequest, fmt.Sprintf("Invalid image quality: %v", err))
			return
		}

		// Auto-stop live view if active (required for image quality changes)
		var liveViewWasActive bool
		active, err := camera.IsLiveViewActive()
		if err == nil && active {
			liveViewWasActive = true
			logger.Info("Auto-stopping live view before image quality change...")
			if err := camera.StopLiveView(); err != nil {
				logger.Error("Failed to stop live view: %v", err)
				// Continue with quality change anyway
			} else {
				logger.Info("Live view stopped successfully")
				// Small delay to ensure live view is fully stopped before proceeding
				time.Sleep(100 * time.Millisecond)
			}
		}

		if err := camera.SetImageQuality(qualityInt); err != nil {
			s.sendError(w, http.StatusInternalServerError, fmt.Sprintf("Failed to set image quality: %v", err))
			return
		}

		// Auto-restart live view if it was active before
		if liveViewWasActive {
			logger.Info("Auto-restarting live view after image quality change...")
			if err := camera.StartLiveView(); err != nil {
				logger.Error("Failed to restart live view: %v", err)
				// Don't fail the quality change - just log the error
			} else {
				logger.Info("Live view restarted successfully")
			}
		}

		s.sendJSON(w, http.StatusOK, QualitySetResponse{
			Quality:    imageQualityToString(qualityInt),
			Status:     "ok",
			StatusCode: 200,
		})

	default:
		s.sendError(w, http.StatusMethodNotAllowed, "Method not allowed")
	}
}

// Conversion Settings Handler

func (s *Server) handleSettingsConversion(w http.ResponseWriter, r *http.Request) {
	sess := s.state.GetSession()

	// Conversion settings require an active session
	if sess == nil {
		s.sendError(w, http.StatusBadRequest, "Conversion settings require an active session - start a session first")
		return
	}

	switch r.Method {
	case http.MethodGet:
		s.sendJSON(w, http.StatusOK, ConversionGetResponse{
			ConvertFormat:  sess.ConvertFormat,
			DeleteRAFAfter: sess.DeleteRAFAfter,
			Status:         "ok",
			StatusCode:     200,
		})

	case http.MethodPost:
		var req ConversionSetRequest
		if err := s.parseJSON(r, &req); err != nil {
			s.sendError(w, http.StatusBadRequest, fmt.Sprintf("Invalid request: %v", err))
			return
		}

		// Validate format
		format := req.ConvertFormat
		if format != "none" && format != "fits" && format != "tiff" {
			s.sendError(w, http.StatusBadRequest, fmt.Sprintf("Invalid conversion format: %s (must be 'none', 'fits', or 'tiff')", format))
			return
		}

		// Update session settings
		sess.ConvertFormat = format
		sess.DeleteRAFAfter = req.DeleteRAFAfter

		s.sendJSON(w, http.StatusOK, ConversionSetResponse{
			ConvertFormat:  sess.ConvertFormat,
			DeleteRAFAfter: sess.DeleteRAFAfter,
			Status:         "ok",
			StatusCode:     200,
		})

	default:
		s.sendError(w, http.StatusMethodNotAllowed, "Method not allowed")
	}
}

// Focus Operations Handlers

// handleFocusTrigger triggers a single-shot autofocus operation
func (s *Server) handleFocusTrigger(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		s.sendError(w, http.StatusMethodNotAllowed, "Method not allowed")
		return
	}

	camera := s.state.GetCamera()
	if !camera.IsConnected() {
		s.sendError(w, http.StatusServiceUnavailable, "Camera not connected")
		return
	}

	// Auto-stop live view if active (required for autofocus triggering)
	var liveViewWasActive bool
	active, err := camera.IsLiveViewActive()
	if err == nil && active {
		liveViewWasActive = true
		logger.Info("Auto-stopping live view before autofocus trigger...")
		if err := camera.StopLiveView(); err != nil {
			logger.Error("Failed to stop live view: %v", err)
			// Continue with autofocus anyway
		} else {
			logger.Info("Live view stopped successfully")
		}
	}

	if err := camera.TriggerAutoFocus(); err != nil {
		s.sendError(w, http.StatusInternalServerError, fmt.Sprintf("Failed to trigger autofocus: %v", err))
		return
	}

	// Auto-restart live view if it was active before
	if liveViewWasActive {
		// Wait 300ms for the SDK to settle after autofocus operation
		// The SDK sample code uses similar delays between operations
		time.Sleep(300 * time.Millisecond)

		if err := camera.StartLiveView(); err != nil {
			logger.Error("Failed to restart live view after autofocus: %v", err)
			// Don't fail the autofocus trigger - just log the error
		}
	}

	s.sendJSON(w, http.StatusOK, FocusTriggerResponse{
		Status:     "ok",
		Message:    "Autofocus triggered successfully",
		StatusCode: 200,
	})
}

// Session Management Handlers

func (s *Server) handleSession(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodGet {
		s.sendError(w, http.StatusMethodNotAllowed, "Method not allowed")
		return
	}

	sess := s.state.GetSession()
	if sess == nil {
		s.sendJSON(w, http.StatusOK, SessionGetResponse{})
		return
	}

	s.sendJSON(w, http.StatusOK, SessionGetResponse{
		Project:   sess.ProjectName,
		Sequence:  sess.SequenceNumber,
		OutputDir: sess.OutputDir,
	})
}

func (s *Server) handleSessionStart(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		s.sendError(w, http.StatusMethodNotAllowed, "Method not allowed")
		return
	}

	var req SessionStartRequest
	if err := s.parseJSON(r, &req); err != nil {
		logger.Error("Session start - failed to parse JSON: %v", err)
		s.sendError(w, http.StatusBadRequest, fmt.Sprintf("Invalid request: %v", err))
		return
	}

	logger.Info("Session start request: project=%s, output_dir=%s", req.Project, req.OutputDir)

	// Expand tilde in output directory
	outputDir := req.OutputDir
	if len(outputDir) > 0 && outputDir[0] == '~' {
		home, _ := os.UserHomeDir()
		outputDir = filepath.Join(home, outputDir[1:])
	}

	// Create new session
	camera := s.state.GetCamera()
	sess, err := session.New(req.Project, outputDir, camera)
	if err != nil {
		logger.Error("Session start - failed to create session: %v", err)
		s.sendError(w, http.StatusBadRequest, fmt.Sprintf("Failed to create session: %v", err))
		return
	}

	logger.Info("Session created successfully: project=%s, output_dir=%s, sequence=%d",
		sess.ProjectName, sess.OutputDir, sess.SequenceNumber)

	s.state.SetSession(sess)

	s.sendJSON(w, http.StatusOK, SessionStartResponse{
		Project:   sess.ProjectName,
		Sequence:  sess.SequenceNumber,
		CreatedAt: sess.CreatedAt.Format("2006-01-02T15:04:05Z"),
	})
}

func (s *Server) handleSessionStop(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		s.sendError(w, http.StatusMethodNotAllowed, "Method not allowed")
		return
	}

	s.state.ClearSession()

	s.sendJSON(w, http.StatusOK, SessionStopResponse{
		Status:     "stopped",
		StatusCode: 200,
	})
}

// handleSessionBrowseDirectory shows a native folder picker dialog on the server
func (s *Server) handleSessionBrowseDirectory(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		s.sendError(w, http.StatusMethodNotAllowed, "Method not allowed")
		return
	}

	// Use PowerShell to show Windows folder browser dialog
	// This runs on the server side, so it shows the server's file system
	psScript := `Add-Type -AssemblyName System.Windows.Forms
$folderBrowser = New-Object System.Windows.Forms.FolderBrowserDialog
$folderBrowser.Description = "Select capture directory"
$folderBrowser.RootFolder = [System.Environment+SpecialFolder]::MyComputer
$folderBrowser.ShowNewFolderButton = $true
$result = $folderBrowser.ShowDialog()
if ($result -eq [System.Windows.Forms.DialogResult]::OK) {
    Write-Output $folderBrowser.SelectedPath
} else {
    exit 1
}`

	// NOTE: Must NOT use -NonInteractive flag as it prevents GUI dialogs
	cmd := exec.Command("powershell", "-NoProfile", "-Command", psScript)
	output, err := cmd.Output()

	logger.Info("Browse directory result: err=%v, output=%s", err, string(output))

	if err != nil {
		// User cancelled or error occurred
		s.sendJSON(w, http.StatusOK, BrowseDirectoryResponse{
			Selected:   false,
			Path:       "",
			Status:     "cancelled",
			StatusCode: 200,
		})
		return
	}

	selectedPath := strings.TrimSpace(string(output))

	s.sendJSON(w, http.StatusOK, BrowseDirectoryResponse{
		Selected:   true,
		Path:       selectedPath,
		Status:     "ok",
		StatusCode: 200,
	})
}

// Capture Handlers (placeholders - full implementation requires more session integration)

func (s *Server) handleCaptureSingle(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		s.sendError(w, http.StatusMethodNotAllowed, "Method not allowed")
		return
	}

	camera := s.state.GetCamera()

	if !camera.IsConnected() {
		s.sendError(w, http.StatusServiceUnavailable, "Camera not connected")
		return
	}

	// Auto-stop live view if active (required for capture)
	var liveViewWasActive bool
	active, err := camera.IsLiveViewActive()
	if err == nil && active {
		liveViewWasActive = true
		logger.Info("Auto-stopping live view before capture...")
		if err := camera.StopLiveView(); err != nil {
			logger.Error("Failed to stop live view: %v", err)
			// Continue with capture anyway
		} else {
			logger.Info("Live view stopped successfully")
		}
	}

	sess := s.state.GetSession()

	var filename string
	var outputDir string
	var filePath string

	if sess != nil {
		// Session exists - use session-based capture
		logger.Info("Capture: using active session (project=%s, output_dir=%s)", sess.ProjectName, sess.OutputDir)
		filename = sess.GetNextFilename()

		// Capture and download
		if err := sess.Capture(); err != nil {
			s.sendError(w, http.StatusInternalServerError, fmt.Sprintf("Capture failed: %v", err))
			return
		}

		filePath = filepath.Join(sess.OutputDir, filename)
		logger.Info("Capture: saved to %s", filePath)
	} else {
		// No session - do standalone capture
		logger.Info("Capture: no active session, using default directory ./captures")
		outputDir = "./captures"

		// Get next available filename
		var err error
		filename, err = session.GetNextStandaloneFilename(outputDir)
		if err != nil {
			s.sendError(w, http.StatusInternalServerError, fmt.Sprintf("Failed to generate filename: %v", err))
			return
		}

		// Trigger camera capture
		if err := camera.Capture(); err != nil {
			s.sendError(w, http.StatusInternalServerError, fmt.Sprintf("Capture failed: %v", err))
			return
		}

		// Download the captured image
		if err := camera.DownloadLast(outputDir, filename); err != nil {
			s.sendError(w, http.StatusInternalServerError, fmt.Sprintf("Download failed: %v", err))
			return
		}

		filePath = filepath.Join(outputDir, filename)
	}

	// Get file size
	stat, err := os.Stat(filePath)
	var size int64
	if err == nil {
		size = stat.Size()
	}

	// Auto-restart live view if it was active before capture
	if liveViewWasActive {
		logger.Info("Auto-restarting live view after capture...")
		if err := camera.StartLiveView(); err != nil {
			logger.Error("Failed to restart live view: %v", err)
			// Don't fail the capture - just log the error
		} else {
			logger.Info("Live view restarted successfully")
		}
	}

	s.sendJSON(w, http.StatusOK, CaptureSingleResponse{
		Filename:   filename,
		Size:       size,
		Status:     "ok",
		StatusCode: 200,
	})
}

func (s *Server) handleCaptureStart(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		s.sendError(w, http.StatusMethodNotAllowed, "Method not allowed")
		return
	}

	var req CaptureStartRequest
	if err := s.parseJSON(r, &req); err != nil {
		s.sendError(w, http.StatusBadRequest, fmt.Sprintf("Invalid request: %v", err))
		return
	}

	sess := s.state.GetSession()
	if sess == nil {
		s.sendError(w, http.StatusBadRequest, "No active session - set a capture directory first")
		return
	}

	camera := s.state.GetCamera()
	if !camera.IsConnected() {
		s.sendError(w, http.StatusServiceUnavailable, "Camera not connected")
		return
	}

	// Stop live view if active (required for intervalometer)
	active, err := camera.IsLiveViewActive()
	if err == nil && active {
		logger.Info("Auto-stopping live view before intervalometer...")
		if err := camera.StopLiveView(); err != nil {
			logger.Error("Failed to stop live view: %v", err)
			s.sendError(w, http.StatusInternalServerError, fmt.Sprintf("Failed to stop live view: %v", err))
			return
		}
		logger.Info("Live view stopped successfully")

		// Give camera time to settle after stopping live view
		// The X-T3 needs time to release hardware before capture can start
		time.Sleep(2 * time.Second)
	}

	logger.Info("Starting intervalometer: count=%d, delay=%d, async=%v", req.Count, req.Delay, req.Async)

	// Set interval tracking fields on the session
	sess.IntervalActive = true
	sess.IntervalTotalFrames = req.Count
	sess.IntervalCurrentFrame = 0
	sess.IntervalDelay = time.Duration(req.Delay) * time.Second
	sess.IntervalStartTime = time.Now()
	sess.IntervalAsyncMode = req.Async

	// Start the intervalometer in a background goroutine
	go func() {
		// Ensure we clear the active flag when done
		defer func() {
			sess.IntervalActive = false
			logger.Info("Intervalometer stopped (active flag cleared)")
		}()

		// Give camera a moment to settle before first capture
		// The SDK needs time after the HTTP response is sent
		// Increased from 1s to 2s to reduce "camera not ready" errors
		time.Sleep(2 * time.Second)
		logger.Info("Intervalometer starting captures...")

		successCount := 0
		for i := 0; i < req.Count; i++ {
			// Check if user requested stop
			if !sess.IntervalActive {
				logger.Info("Intervalometer stopped by user at frame %d/%d", i+1, req.Count)
				break
			}

			// Update current frame counter (1-indexed for user display)
			sess.IntervalCurrentFrame = i + 1

			// Capture using the session
			if err := sess.Capture(); err != nil {
				logger.Error("Intervalometer capture %d/%d failed: %v", i+1, req.Count, err)
				// Continue with next frame despite error
			} else {
				successCount++
				logger.Info("Intervalometer: captured frame %d/%d (success: %d)", i+1, req.Count, successCount)
			}

			// Apply delay (except after last frame)
			if i < req.Count-1 && req.Delay > 0 {
				time.Sleep(time.Duration(req.Delay) * time.Second)
			}
		}
		logger.Info("Intervalometer complete: %d/%d frames captured successfully", successCount, req.Count)
	}()

	s.sendJSON(w, http.StatusOK, CaptureStartResponse{
		Status:      "started",
		StatusCode:  200,
		TotalFrames: req.Count,
	})
}

func (s *Server) handleCapturePause(w http.ResponseWriter, r *http.Request) {
	// Placeholder - full intervalometer integration needed
	s.sendError(w, http.StatusNotImplemented, "Intervalometer not yet implemented in server mode")
}

func (s *Server) handleCaptureResume(w http.ResponseWriter, r *http.Request) {
	// Placeholder - full intervalometer integration needed
	s.sendError(w, http.StatusNotImplemented, "Intervalometer not yet implemented in server mode")
}

func (s *Server) handleCaptureStop(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		s.sendError(w, http.StatusMethodNotAllowed, "Method not allowed")
		return
	}

	sess := s.state.GetSession()
	if sess == nil {
		s.sendError(w, http.StatusBadRequest, "No active session")
		return
	}

	// Clear interval active flag (the goroutine will finish its current capture then exit)
	if sess.IntervalActive {
		sess.IntervalActive = false
		logger.Info("Intervalometer stop requested - setting active flag to false")
		s.sendJSON(w, http.StatusOK, CaptureStopResponse{
			Status:     "stopped",
			StatusCode: 200,
		})
	} else {
		s.sendJSON(w, http.StatusOK, CaptureStopResponse{
			Status:     "not_running",
			StatusCode: 200,
		})
	}
}

func (s *Server) handleCaptureStatus(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodGet {
		s.sendError(w, http.StatusMethodNotAllowed, "Method not allowed")
		return
	}

	// Get session from state
	sess := s.state.GetSession()
	if sess == nil {
		// No session - return inactive
		s.sendJSON(w, http.StatusOK, CaptureStatusResponse{
			Active: false,
		})
		return
	}

	// Check if intervalometer is running
	if !sess.IntervalActive {
		s.sendJSON(w, http.StatusOK, CaptureStatusResponse{
			Active: false,
		})
		return
	}

	// Calculate elapsed time and ETA
	elapsed := int(time.Since(sess.IntervalStartTime).Seconds())
	eta := 0
	if sess.IntervalTotalFrames > 0 && sess.IntervalCurrentFrame > 0 {
		// Estimate ETA based on progress so far
		averageTimePerFrame := float64(elapsed) / float64(sess.IntervalCurrentFrame)
		framesRemaining := sess.IntervalTotalFrames - sess.IntervalCurrentFrame
		eta = int(averageTimePerFrame * float64(framesRemaining))
	}

	// Return active status with progress
	s.sendJSON(w, http.StatusOK, CaptureStatusResponse{
		Active:  true,
		Frame:   sess.IntervalCurrentFrame,
		Total:   sess.IntervalTotalFrames,
		Elapsed: elapsed,
		ETA:     eta,
	})
}

func (s *Server) handleCaptureLatestPreview(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodGet && r.Method != http.MethodHead {
		s.sendError(w, http.StatusMethodNotAllowed, "GET or HEAD required")
		return
	}

	// Get session from state
	sess := s.state.GetSession()
	if sess == nil {
		s.sendError(w, http.StatusNotFound, "No active session")
		return
	}

	// Check if we have a latest capture
	if sess.LatestCapturePath == "" {
		s.sendError(w, http.StatusNotFound, "No captures available yet")
		return
	}

	// Check if the file exists
	if _, err := os.Stat(sess.LatestCapturePath); os.IsNotExist(err) {
		s.sendError(w, http.StatusNotFound, fmt.Sprintf("Capture file not found: %s", sess.LatestCapturePath))
		return
	}

	// For HEAD requests, just return headers without body
	if r.Method == http.MethodHead {
		w.Header().Set("Content-Type", "image/jpeg")
		w.Header().Set("Cache-Control", "no-cache, no-store, must-revalidate")
		w.WriteHeader(http.StatusOK)
		return
	}

	// Check file extension to determine how to serve preview
	ext := strings.ToLower(filepath.Ext(sess.LatestCapturePath))
	var thumbData []byte
	var err error

	if ext == ".jpg" || ext == ".jpeg" {
		// JPEG file - serve directly (already compressed)
		thumbData, err = os.ReadFile(sess.LatestCapturePath)
		if err != nil {
			s.sendError(w, http.StatusInternalServerError, fmt.Sprintf("Failed to read JPEG: %v", err))
			return
		}
	} else {
		// RAF file - extract embedded JPEG thumbnail using LibRaw
		thumbData, err = converter.ExtractThumbnail(sess.LatestCapturePath)
		if err != nil {
			s.sendError(w, http.StatusInternalServerError, fmt.Sprintf("Failed to extract thumbnail: %v", err))
			return
		}
	}

	// Serve the JPEG image/thumbnail
	w.Header().Set("Content-Type", "image/jpeg")
	w.Header().Set("Cache-Control", "no-cache, no-store, must-revalidate")
	w.Header().Set("Content-Length", fmt.Sprintf("%d", len(thumbData)))
	w.WriteHeader(http.StatusOK)
	w.Write(thumbData)
}

// Live View Handlers (E-3)

func (s *Server) handleLiveViewStart(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		s.sendError(w, http.StatusMethodNotAllowed, "POST required")
		return
	}

	camera := s.state.GetCamera()
	if camera == nil {
		s.sendError(w, http.StatusInternalServerError, "Camera not initialized")
		return
	}

	// Check if camera is connected
	if !camera.IsConnected() {
		s.sendError(w, http.StatusBadRequest, "Camera not connected")
		return
	}

	// Check if intervalometer is running - camera can't do both simultaneously
	sess := s.state.GetSession()
	if sess != nil && sess.IntervalActive {
		s.sendError(w, http.StatusConflict, "Cannot start live view while intervalometer is running. Camera can only perform one operation at a time.")
		return
	}

	// Start live view
	err := camera.StartLiveView()
	if err != nil {
		s.sendError(w, http.StatusInternalServerError, fmt.Sprintf("Failed to start live view: %v", err))
		return
	}

	// NOTE: SetLiveViewSize removed - SDK uses default size automatically
	// Attempting to set size after start causes XSDK_SetProp to fail

	// Return success response
	response := map[string]interface{}{
		"status":  "started",
		"message": "Live view started successfully",
	}
	s.sendJSON(w, http.StatusOK, response)
}

func (s *Server) handleLiveViewStop(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		s.sendError(w, http.StatusMethodNotAllowed, "POST required")
		return
	}

	camera := s.state.GetCamera()
	if camera == nil {
		s.sendError(w, http.StatusInternalServerError, "Camera not initialized")
		return
	}

	// Check if live view is already stopped (idempotent operation)
	active, err := camera.IsLiveViewActive()
	if err == nil && !active {
		// Already stopped - return success anyway (idempotent)
		response := map[string]interface{}{
			"status":  "already_stopped",
			"message": "Live view already stopped",
		}
		s.sendJSON(w, http.StatusOK, response)
		return
	}

	// Stop live view
	err = camera.StopLiveView()
	if err != nil {
		s.sendError(w, http.StatusInternalServerError, fmt.Sprintf("Failed to stop live view: %v", err))
		return
	}

	// Return success response
	response := map[string]interface{}{
		"status":  "stopped",
		"message": "Live view stopped successfully",
	}
	s.sendJSON(w, http.StatusOK, response)
}

func (s *Server) handleLiveViewStream(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodGet {
		s.sendError(w, http.StatusMethodNotAllowed, "GET required")
		return
	}

	camera := s.state.GetCamera()
	if camera == nil {
		logger.Error("handleLiveViewStream: Camera not initialized")
		s.sendError(w, http.StatusInternalServerError, "Camera not initialized")
		return
	}

	// Check if camera is connected
	if !camera.IsConnected() {
		logger.Error("handleLiveViewStream: Camera not connected")
		s.sendError(w, http.StatusBadRequest, "Camera not connected")
		return
	}

	// Check if live view is already active (don't auto-start)
	logger.Info("Checking if live view is active...")
	active, err := camera.IsLiveViewActive()
	if err != nil {
		logger.Error("handleLiveViewStream: Failed to check live view status: %v", err)
		s.sendError(w, http.StatusInternalServerError, fmt.Sprintf("Failed to check live view status: %v", err))
		return
	}
	logger.Info("Live view active status: %v", active)

	if !active {
		logger.Error("handleLiveViewStream: Live view not started - use 'Start Live View' button first")
		s.sendError(w, http.StatusBadRequest, "Live view not started - click 'Start Live View' button first")
		return
	}

	// Set MJPEG headers
	logger.Info("Setting MJPEG headers...")
	w.Header().Set("Content-Type", "multipart/x-mixed-replace; boundary=frame")
	w.Header().Set("Cache-Control", "no-cache, no-store, must-revalidate")
	w.Header().Set("Pragma", "no-cache")
	w.Header().Set("Expires", "0")
	w.Header().Set("Access-Control-Allow-Origin", "*")

	// Get flusher for streaming
	logger.Info("Checking for streaming support...")
	flusher, ok := w.(http.Flusher)
	if !ok {
		logger.Error("Streaming not supported by response writer")
		s.sendError(w, http.StatusInternalServerError, "Streaming not supported")
		return
	}
	logger.Info("Flusher obtained successfully")

	// Stream frames at ~5 fps (200ms interval)
	logger.Info("Creating ticker for 5fps streaming...")
	ticker := time.NewTicker(200 * time.Millisecond)
	defer ticker.Stop()

	logger.Info("Live view stream started for client %s", r.RemoteAddr)
	defer logger.Info("Live view stream ended for client %s", r.RemoteAddr)

	for {
		select {
		case <-r.Context().Done():
			// Client disconnected
			return

		case <-ticker.C:
			// Get frame from camera
			frameData, err := camera.GetLiveViewFrame()
			if err != nil {
				// Frame polling errors are normal - camera buffer may be temporarily empty
				// Don't log these as errors to reduce noise (they happen frequently)
				continue
			}

			// Skip empty frames
			if len(frameData) == 0 {
				// Empty frames are also normal - just wait for next frame
				continue
			}

			// Write MJPEG frame
			fmt.Fprintf(w, "--frame\r\n")
			fmt.Fprintf(w, "Content-Type: image/jpeg\r\n")
			fmt.Fprintf(w, "Content-Length: %d\r\n", len(frameData))
			fmt.Fprintf(w, "\r\n")
			w.Write(frameData)
			fmt.Fprintf(w, "\r\n")

			flusher.Flush()
		}
	}
}

// Homepage Handler - serves the live view HTML (E-3)

func (s *Server) handleHome(w http.ResponseWriter, r *http.Request) {
	if r.URL.Path != "/" {
		http.NotFound(w, r)
		return
	}

	// Serve the live view HTML page
	http.ServeFile(w, r, "static/liveview.html")
}
