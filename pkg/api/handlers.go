package api

import (
	"encoding/json"
	"fmt"
	"net/http"
	"os"
	"path/filepath"

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

		if err := camera.SetISO(req.ISO); err != nil {
			s.sendError(w, http.StatusInternalServerError, fmt.Sprintf("Failed to set ISO: %v", err))
			return
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

		// Get supported shutter speeds and find closest match
		supported, err := camera.GetSupportedShutterSpeeds()
		if err != nil || len(supported) == 0 {
			// If we can't get supported speeds, try setting directly
			logger.Error("Warning: Could not get supported shutter speeds, trying direct set")
			if err := camera.SetShutter(requestedUS); err != nil {
				s.sendError(w, http.StatusInternalServerError, fmt.Sprintf("Failed to set shutter: %v", err))
				return
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

		if err := camera.SetFocusMode(modeInt); err != nil {
			s.sendError(w, http.StatusInternalServerError, fmt.Sprintf("Failed to set focus mode: %v", err))
			return
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

// Live View Handlers (placeholders for E-3)

func (s *Server) handleLiveViewStart(w http.ResponseWriter, r *http.Request) {
	s.sendError(w, http.StatusNotImplemented, "Live view not yet implemented")
}

func (s *Server) handleLiveViewStop(w http.ResponseWriter, r *http.Request) {
	s.sendError(w, http.StatusNotImplemented, "Live view not yet implemented")
}

func (s *Server) handleLiveViewStream(w http.ResponseWriter, r *http.Request) {
	s.sendError(w, http.StatusNotImplemented, "Live view not yet implemented")
}

// Homepage Handler (placeholder for G-5)

func (s *Server) handleHome(w http.ResponseWriter, r *http.Request) {
	if r.URL.Path != "/" {
		http.NotFound(w, r)
		return
	}

	// Serve a simple HTML page
	html := `<!DOCTYPE html>
<html>
<head>
    <title>Fujimatic Server</title>
    <style>
        body { font-family: Arial, sans-serif; max-width: 800px; margin: 50px auto; padding: 20px; }
        h1 { color: #333; }
        .endpoint { background: #f4f4f4; padding: 10px; margin: 10px 0; border-left: 3px solid #007bff; }
        .method { font-weight: bold; color: #007bff; }
    </style>
</head>
<body>
    <h1>Fujimatic REST API Server</h1>
    <p>Server is running. Web UI coming in G-5.</p>
    
    <h2>API Response Format</h2>
    <div style="background: #f9f9f9; padding: 15px; margin: 15px 0; border: 1px solid #ddd;">
        <p><strong>All endpoints return both numeric status codes and human-readable messages:</strong></p>
        <pre style="background: #fff; padding: 10px; border: 1px solid #ccc;">
{
  "status": "ok",
  "status_code": 200
}</pre>
    </div>
    
    <h2>Available Endpoints:</h2>
    <div class="endpoint"><span class="method">POST</span> /api/camera/connect</div>
    <div class="endpoint"><span class="method">POST</span> /api/camera/disconnect</div>
    <div class="endpoint"><span class="method">GET</span> /api/camera/status</div>
    <div class="endpoint"><span class="method">GET</span> /api/camera/battery</div>
    <div class="endpoint"><span class="method">GET/POST</span> /api/settings/iso</div>
    <div class="endpoint"><span class="method">GET/POST</span> /api/settings/shutter <small>(shutter_speed field)</small></div>
    <div class="endpoint"><span class="method">GET/POST</span> /api/settings/focus</div>
    <div class="endpoint"><span class="method">GET</span> /api/session</div>
    <div class="endpoint"><span class="method">POST</span> /api/session/start</div>
    <div class="endpoint"><span class="method">POST</span> /api/session/stop</div>
    <div class="endpoint"><span class="method">POST</span> /api/capture/single</div>
    <div class="endpoint"><span class="method">GET</span> /api/capture/status</div>
    
    <h2>Example Usage</h2>
    <div style="background: #f9f9f9; padding: 15px; margin: 15px 0; border: 1px solid #ddd;">
        <h4>Set shutter speed:</h4>
        <pre style="background: #fff; padding: 10px; border: 1px solid #ccc;">
curl -X POST http://localhost:8080/api/settings/shutter \
  -H "Content-Type: application/json" \
  -d '{"shutter": "1/60"}'

Response:
{
  "shutter_speed": "1/60",
  "status": "ok", 
  "status_code": 200
}</pre>
    </div>
</body>
</html>`

	w.Header().Set("Content-Type", "text/html")
	fmt.Fprint(w, html)
}
