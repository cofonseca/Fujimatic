package api

import (
	"encoding/json"
	"fmt"
	"net/http"
	"os"
	"path/filepath"
	"time"

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
			if err := camera.StopLiveView(); err != nil {
				logger.Error("Failed to stop live view before ISO change: %v", err)
				// Continue with ISO change anyway
			}
		}

		if err := camera.SetISO(req.ISO); err != nil {
			s.sendError(w, http.StatusInternalServerError, fmt.Sprintf("Failed to set ISO: %v", err))
			return
		}

		// Auto-restart live view if it was active before
		if liveViewWasActive {
			if err := camera.StartLiveView(); err != nil {
				logger.Error("Failed to restart live view after ISO change: %v", err)
				// Don't fail the ISO change - just log the error
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
			if err := camera.StopLiveView(); err != nil {
				logger.Error("Failed to stop live view before shutter change: %v", err)
				// Continue with shutter change anyway
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
				if err := camera.StartLiveView(); err != nil {
					logger.Error("Failed to restart live view after shutter change: %v", err)
					// Don't fail the shutter change - just log the error
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
			if err := camera.StartLiveView(); err != nil {
				logger.Error("Failed to restart live view after shutter change: %v", err)
				// Don't fail the shutter change - just log the error
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

	if err := camera.TriggerAutoFocus(); err != nil {
		s.sendError(w, http.StatusInternalServerError, fmt.Sprintf("Failed to trigger autofocus: %v", err))
		return
	}

	s.sendJSON(w, http.StatusOK, FocusTriggerResponse{
		Status:     "ok",
		Message:    "Autofocus triggered successfully",
		StatusCode: 200,
	})
}

// handleFocusAdjust adjusts focus manually in NEAR or FAR direction
func (s *Server) handleFocusAdjust(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		s.sendError(w, http.StatusMethodNotAllowed, "Method not allowed")
		return
	}

	camera := s.state.GetCamera()
	if !camera.IsConnected() {
		s.sendError(w, http.StatusServiceUnavailable, "Camera not connected")
		return
	}

	var req FocusAdjustRequest
	if err := s.parseJSON(r, &req); err != nil {
		s.sendError(w, http.StatusBadRequest, "Invalid JSON request")
		return
	}

	// Validate direction
	if req.Direction != "near" && req.Direction != "far" {
		s.sendError(w, http.StatusBadRequest, "Invalid direction (must be 'near' or 'far')")
		return
	}

	// Validate steps (must be positive)
	if req.Steps <= 0 {
		s.sendError(w, http.StatusBadRequest, "Invalid steps (must be positive)")
		return
	}

	if err := camera.AdjustFocus(req.Direction, req.Steps); err != nil {
		s.sendError(w, http.StatusInternalServerError, fmt.Sprintf("Failed to adjust focus: %v", err))
		return
	}

	s.sendJSON(w, http.StatusOK, FocusAdjustResponse{
		Status:     "ok",
		Message:    fmt.Sprintf("Focus adjusted %s by %d steps", req.Direction, req.Steps),
		Direction:  req.Direction,
		Steps:      req.Steps,
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
		s.sendError(w, http.StatusBadRequest, fmt.Sprintf("Invalid request: %v", err))
		return
	}

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
		s.sendError(w, http.StatusBadRequest, fmt.Sprintf("Failed to create session: %v", err))
		return
	}

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
		filename = sess.GetNextFilename()

		// Capture and download
		if err := sess.Capture(); err != nil {
			s.sendError(w, http.StatusInternalServerError, fmt.Sprintf("Capture failed: %v", err))
			return
		}

		filePath = filepath.Join(sess.OutputDir, filename)
	} else {
		// No session - do standalone capture
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
	// Placeholder - full intervalometer integration needed
	s.sendError(w, http.StatusNotImplemented, "Intervalometer not yet implemented in server mode")
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
	// Placeholder - full intervalometer integration needed
	s.sendError(w, http.StatusNotImplemented, "Intervalometer not yet implemented in server mode")
}

func (s *Server) handleCaptureStatus(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodGet {
		s.sendError(w, http.StatusMethodNotAllowed, "Method not allowed")
		return
	}

	// Placeholder - return inactive for now
	s.sendJSON(w, http.StatusOK, CaptureStatusResponse{
		Active: false,
	})
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
