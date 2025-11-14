package api

import (
	"context"
	"fmt"
	"net/http"
	"time"

	"github.com/cfonseca/fujimatic/pkg/logger"
	"github.com/cfonseca/fujimatic/pkg/server"
)

// Server represents the HTTP REST API server
type Server struct {
	state      *server.State
	httpServer *http.Server
	port       int
}

// NewServer creates a new API server
func NewServer(state *server.State, port int) *Server {
	return &Server{
		state: state,
		port:  port,
	}
}

// Start starts the HTTP server
func (s *Server) Start() error {
	mux := http.NewServeMux()

	// Register routes
	s.registerRoutes(mux)

	// Create HTTP server
	s.httpServer = &http.Server{
		Addr:         fmt.Sprintf(":%d", s.port),
		Handler:      s.withMiddleware(mux),
		ReadTimeout:  15 * time.Second,
		WriteTimeout: 0, // Disabled for MJPEG streaming support (live view)
		IdleTimeout:  60 * time.Second,
	}

	logger.Info("Starting HTTP server on port %d", s.port)
	return s.httpServer.ListenAndServe()
}

// Stop gracefully stops the HTTP server
func (s *Server) Stop(ctx context.Context) error {
	if s.httpServer == nil {
		return nil
	}
	logger.Info("Stopping HTTP server...")
	return s.httpServer.Shutdown(ctx)
}

// registerRoutes registers all API routes
func (s *Server) registerRoutes(mux *http.ServeMux) {
	// Camera Control
	mux.HandleFunc("/api/camera/connect", s.handleCameraConnect)
	mux.HandleFunc("/api/camera/disconnect", s.handleCameraDisconnect)
	mux.HandleFunc("/api/camera/status", s.handleCameraStatus)
	mux.HandleFunc("/api/camera/battery", s.handleCameraBattery)

	// Camera Settings
	mux.HandleFunc("/api/settings/iso", s.handleSettingsISO)
	mux.HandleFunc("/api/settings/shutter", s.handleSettingsShutter)
	mux.HandleFunc("/api/settings/focus", s.handleSettingsFocus)

	// Session Management
	mux.HandleFunc("/api/session", s.handleSession)
	mux.HandleFunc("/api/session/start", s.handleSessionStart)
	mux.HandleFunc("/api/session/stop", s.handleSessionStop)

	// Capture
	mux.HandleFunc("/api/capture/single", s.handleCaptureSingle)
	mux.HandleFunc("/api/capture/start", s.handleCaptureStart)
	mux.HandleFunc("/api/capture/pause", s.handleCapturePause)
	mux.HandleFunc("/api/capture/resume", s.handleCaptureResume)
	mux.HandleFunc("/api/capture/stop", s.handleCaptureStop)
	mux.HandleFunc("/api/capture/status", s.handleCaptureStatus)

	// Live View (placeholder for E-3)
	mux.HandleFunc("/api/liveview/start", s.handleLiveViewStart)
	mux.HandleFunc("/api/liveview/stop", s.handleLiveViewStop)
	mux.HandleFunc("/liveview", s.handleLiveViewStream)

	// Static files and homepage (placeholder for G-5)
	mux.HandleFunc("/", s.handleHome)
}

// withMiddleware wraps the handler with middleware
func (s *Server) withMiddleware(handler http.Handler) http.Handler {
	// Apply middleware in reverse order (outermost first)
	handler = s.corsMiddleware(handler)
	handler = s.loggingMiddleware(handler)
	handler = s.recoveryMiddleware(handler)
	return handler
}
