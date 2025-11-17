package api

// Request and response types for REST API

// Camera Control

type ConnectResponse struct {
	Status      string `json:"status"`
	StatusCode  int    `json:"status_code"`
	Battery     int    `json:"battery"`
}

type DisconnectResponse struct {
	Status     string `json:"status"`
	StatusCode int    `json:"status_code"`
}

type CameraStatusResponse struct {
	Connected bool   `json:"connected"`
	Battery   int    `json:"battery,omitempty"`
	Model     string `json:"model,omitempty"`
}

type BatteryResponse struct {
	Battery  int  `json:"battery"`
	Charging bool `json:"charging"`
}

// Camera Settings

type ISOGetResponse struct {
	ISO        int    `json:"iso"`
	Status     string `json:"status"`
	StatusCode int    `json:"status_code"`
}

type ISOSetRequest struct {
	ISO int `json:"iso"`
}

type ISOSetResponse struct {
	ISO        int    `json:"iso"`
	Status     string `json:"status"`
	StatusCode int    `json:"status_code"`
}

type ShutterGetResponse struct {
	ShutterSpeed string `json:"shutter_speed"` // API response
	Status       string `json:"status"`        // API response
	StatusCode   int    `json:"status_code"`   // API response
}

type ShutterSetRequest struct {
	Shutter   string `json:"shutter,omitempty"`    // e.g., "1/250" or "2s"
	ShutterUS int    `json:"shutter_us,omitempty"` // microseconds
}

type ShutterSetResponse struct {
	ShutterSpeed string `json:"shutter_speed"`
	Status       string `json:"status"`
	StatusCode   int    `json:"status_code"`
}

type FocusGetResponse struct {
	FocusMode  string `json:"focus_mode"` // "manual", "auto"
	Status     string `json:"status"`
	StatusCode int    `json:"status_code"`
}

type FocusSetRequest struct {
	FocusMode string `json:"focus_mode"` // "manual", "auto"
}

type FocusSetResponse struct {
	FocusMode  string `json:"focus_mode"`
	Status     string `json:"status"`
	StatusCode int    `json:"status_code"`
}

// Session Management

type SessionGetResponse struct {
	Project   string `json:"project,omitempty"`
	Sequence  int    `json:"sequence,omitempty"`
	OutputDir string `json:"output_dir,omitempty"`
}

type SessionStartRequest struct {
	Project   string `json:"project"`
	OutputDir string `json:"output_dir"`
}

type SessionStartResponse struct {
	Project   string `json:"project"`
	Sequence  int    `json:"sequence"`
	CreatedAt string `json:"created_at"`
}

type SessionStopResponse struct {
	Status     string `json:"status"`
	StatusCode int    `json:"status_code"`
}

type BrowseDirectoryResponse struct {
	Selected   bool   `json:"selected"`    // true if user selected a directory, false if cancelled
	Path       string `json:"path"`        // Selected directory path (empty if cancelled)
	Status     string `json:"status"`      // "ok" or "cancelled"
	StatusCode int    `json:"status_code"` // HTTP status code
}

// Capture

type CaptureSingleResponse struct {
	Filename   string `json:"filename"`
	Size       int64  `json:"size"`
	Status     string `json:"status"`
	StatusCode int    `json:"status_code"`
}

type CaptureStartRequest struct {
	Count int  `json:"count"` // 0 for infinite
	Delay int  `json:"delay"` // seconds
	Async bool `json:"async"`
}

type CaptureStartResponse struct {
	Status      string `json:"status"`
	StatusCode  int    `json:"status_code"`
	TotalFrames int    `json:"total_frames"`
}

type CapturePauseResponse struct {
	Status     string `json:"status"`
	StatusCode int    `json:"status_code"`
	Frame      int    `json:"frame"`
	Total      int    `json:"total"`
}

type CaptureResumeResponse struct {
	Status     string `json:"status"`
	StatusCode int    `json:"status_code"`
	Frame      int    `json:"frame"`
	Total      int    `json:"total"`
}

type CaptureStopResponse struct {
	Status     string `json:"status"`
	StatusCode int    `json:"status_code"`
}

type CaptureStatusResponse struct {
	Active  bool `json:"active"`
	Frame   int  `json:"frame,omitempty"`
	Total   int  `json:"total,omitempty"`
	Elapsed int  `json:"elapsed,omitempty"` // seconds
	ETA     int  `json:"eta,omitempty"`     // seconds
}

// Live View

type LiveViewStartResponse struct {
	Status     string `json:"status"`
	StatusCode int    `json:"status_code"`
}

type LiveViewStopResponse struct {
	Status     string `json:"status"`
	StatusCode int    `json:"status_code"`
}

// Focus Operations

type FocusTriggerResponse struct {
	Status     string `json:"status"`
	Message    string `json:"message"`
	StatusCode int    `json:"status_code"`
}

type FocusAdjustRequest struct {
	Direction string `json:"direction"` // "near" or "far"
	Steps     int    `json:"steps"`     // Number of focus steps to move (positive integer)
}

type FocusAdjustResponse struct {
	Status     string `json:"status"`
	Message    string `json:"message"`
	Direction  string `json:"direction"`
	Steps      int    `json:"steps"`
	StatusCode int    `json:"status_code"`
}

// Conversion Settings

type ConversionGetResponse struct {
	ConvertFormat  string `json:"convert_format"`  // "none", "fits", or "tiff"
	DeleteRAFAfter bool   `json:"delete_raf_after"` // Delete RAF files after conversion
	Status         string `json:"status"`
	StatusCode     int    `json:"status_code"`
}

type ConversionSetRequest struct {
	ConvertFormat  string `json:"convert_format"`   // "none", "fits", or "tiff"
	DeleteRAFAfter bool   `json:"delete_raf_after"` // Delete RAF files after conversion
}

type ConversionSetResponse struct {
	ConvertFormat  string `json:"convert_format"`
	DeleteRAFAfter bool   `json:"delete_raf_after"`
	Status         string `json:"status"`
	StatusCode     int    `json:"status_code"`
}

// Error Response

type ErrorResponse struct {
	Error   string `json:"error"`
	Message string `json:"message,omitempty"`
	Code    int    `json:"code,omitempty"`
}
