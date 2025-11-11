package client

import (
	"bytes"
	"encoding/json"
	"fmt"
	"io"
	"net/http"
	"sync"
	"time"

	"github.com/cfonseca/fujimatic/pkg/api"
)

// RemoteCamera implements hal.Camera interface for remote camera control via HTTP
type RemoteCamera struct {
	baseURL    string
	httpClient *http.Client
	mu         sync.RWMutex
	connected  bool // Track connection state locally
}

// NewRemoteCamera creates a new remote camera client
func NewRemoteCamera(serverAddr string) (*RemoteCamera, error) {
	baseURL, err := parseServerAddress(serverAddr)
	if err != nil {
		return nil, fmt.Errorf("invalid server address: %w", err)
	}

	return &RemoteCamera{
		baseURL: baseURL,
		httpClient: &http.Client{
			Timeout: 30 * time.Second,
		},
	}, nil
}

// Helper methods for HTTP requests

func (c *RemoteCamera) get(endpoint string, response interface{}) error {
	url := c.baseURL + endpoint
	resp, err := c.httpClient.Get(url)
	if err != nil {
		return fmt.Errorf("HTTP GET failed: %w", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return c.parseError(resp)
	}

	return json.NewDecoder(resp.Body).Decode(response)
}

func (c *RemoteCamera) post(endpoint string, request interface{}, response interface{}) error {
	url := c.baseURL + endpoint

	var body io.Reader
	if request != nil {
		jsonData, err := json.Marshal(request)
		if err != nil {
			return fmt.Errorf("failed to marshal request: %w", err)
		}
		body = bytes.NewBuffer(jsonData)
	}

	resp, err := c.httpClient.Post(url, "application/json", body)
	if err != nil {
		return fmt.Errorf("HTTP POST failed: %w", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return c.parseError(resp)
	}

	if response != nil {
		return json.NewDecoder(resp.Body).Decode(response)
	}

	return nil
}

func (c *RemoteCamera) parseError(resp *http.Response) error {
	var errResp api.ErrorResponse
	if err := json.NewDecoder(resp.Body).Decode(&errResp); err != nil {
		return fmt.Errorf("server returned status %d", resp.StatusCode)
	}
	return fmt.Errorf("server error (%d): %s", resp.StatusCode, errResp.Message)
}

// Camera interface implementation

// Connect connects to the camera on the server
func (c *RemoteCamera) Connect() error {
	var resp api.ConnectResponse
	if err := c.post("/api/camera/connect", nil, &resp); err != nil {
		return err
	}

	c.mu.Lock()
	c.connected = true
	c.mu.Unlock()

	return nil
}

// Disconnect disconnects from the camera on the server
func (c *RemoteCamera) Disconnect() error {
	var resp api.DisconnectResponse
	if err := c.post("/api/camera/disconnect", nil, &resp); err != nil {
		return err
	}

	c.mu.Lock()
	c.connected = false
	c.mu.Unlock()

	return nil
}

// IsConnected returns the connection status
func (c *RemoteCamera) IsConnected() bool {
	c.mu.RLock()
	defer c.mu.RUnlock()
	return c.connected
}

// GetBattery returns the battery level from the server
func (c *RemoteCamera) GetBattery() (int, error) {
	var resp api.BatteryResponse
	if err := c.get("/api/camera/battery", &resp); err != nil {
		return 0, err
	}
	return resp.Battery, nil
}

// GetISO returns the current ISO setting from the server
func (c *RemoteCamera) GetISO() (int, error) {
	var resp api.ISOGetResponse
	if err := c.get("/api/settings/iso", &resp); err != nil {
		return 0, err
	}
	return resp.ISO, nil
}

// SetISO sets the ISO setting on the server
func (c *RemoteCamera) SetISO(iso int) error {
	req := api.ISOSetRequest{ISO: iso}
	var resp api.ISOSetResponse
	return c.post("/api/settings/iso", req, &resp)
}

// GetShutter returns the current shutter speed in microseconds
func (c *RemoteCamera) GetShutter() (int, error) {
	var resp api.ShutterGetResponse
	if err := c.get("/api/settings/shutter", &resp); err != nil {
		return 0, err
	}
	return resp.ShutterUS, nil
}

// SetShutter sets the shutter speed in microseconds
func (c *RemoteCamera) SetShutter(microseconds int) error {
	req := api.ShutterSetRequest{ShutterUS: microseconds}
	var resp api.ShutterSetResponse
	return c.post("/api/settings/shutter", req, &resp)
}

// GetFocusMode returns the current focus mode
func (c *RemoteCamera) GetFocusMode() (int, error) {
	var resp api.FocusGetResponse
	if err := c.get("/api/settings/focus", &resp); err != nil {
		return 0, err
	}

	// Convert string back to int (reverse of server conversion)
	return focusModeFromString(resp.FocusMode)
}

// SetFocusMode sets the focus mode
func (c *RemoteCamera) SetFocusMode(mode int) error {
	// Convert int to string (same as server does)
	modeStr := focusModeToString(mode)

	req := api.FocusSetRequest{FocusMode: modeStr}
	var resp api.FocusSetResponse
	return c.post("/api/settings/focus", req, &resp)
}

// GetSupportedFocusModes returns supported focus modes
func (c *RemoteCamera) GetSupportedFocusModes() ([]int, error) {
	// For now, return standard modes
	// Could be enhanced with a server endpoint
	return []int{0x0001, 0x8001, 0x8002}, nil
}

// GetSupportedShutterSpeeds returns supported shutter speeds
func (c *RemoteCamera) GetSupportedShutterSpeeds() ([]int, error) {
	// For now, return empty - would need server endpoint
	// This is used by CLI for validation, so not critical
	return []int{}, nil
}

// Capture triggers a capture on the server
func (c *RemoteCamera) Capture() error {
	var resp api.CaptureSingleResponse
	return c.post("/api/capture/single", nil, &resp)
}

// DownloadLast downloads the last captured image
// Note: This is handled server-side for RemoteCamera
// The file remains on the server until explicitly downloaded via G-3
func (c *RemoteCamera) DownloadLast(outputDir, filename string) error {
	// For remote camera, the file is already downloaded on the server
	// This method is a no-op since the server handled the download
	// Actual file transfer will be implemented in G-3
	return nil
}

// Focus mode conversion (same as API server)
const (
	FocusModeManual = 0x0001
	FocusModeAFS    = 0x8001
	FocusModeAFC    = 0x8002
)

func focusModeToString(mode int) string {
	switch mode {
	case FocusModeManual:
		return "manual"
	case FocusModeAFS:
		return "auto"
	case FocusModeAFC:
		return "auto_continuous"
	default:
		return "unknown"
	}
}

func focusModeFromString(mode string) (int, error) {
	switch mode {
	case "manual":
		return FocusModeManual, nil
	case "auto":
		return FocusModeAFS, nil
	case "auto_continuous":
		return FocusModeAFC, nil
	default:
		return 0, fmt.Errorf("unknown focus mode: %s", mode)
	}
}

// Accessor methods for connectivity testing

// GetBaseURL returns the base URL of the server
func (c *RemoteCamera) GetBaseURL() string {
	return c.baseURL
}

// GetHTTPClient returns the HTTP client
func (c *RemoteCamera) GetHTTPClient() *http.Client {
	return c.httpClient
}

// ListShutterSpeeds is a diagnostic method for local cameras only
// Not implemented for remote cameras
func (c *RemoteCamera) ListShutterSpeeds() error {
	return fmt.Errorf("ListShutterSpeeds not available for remote cameras")
}

// SetExposureMode sets the camera to Manual exposure mode
// The server automatically manages exposure mode, so this is a no-op
func (c *RemoteCamera) SetExposureMode(mode int) error {
	// Server-side camera handles exposure mode automatically
	// Client doesn't need to explicitly set this
	return nil
}

// GetExposureMode returns the current camera exposure mode
// Always returns Manual mode (0x0001) for tethered control
func (c *RemoteCamera) GetExposureMode() (int, error) {
	// Remote cameras are always in Manual mode for tethered control
	const ManualMode = 0x0001
	return ManualMode, nil
}
