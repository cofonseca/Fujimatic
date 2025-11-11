package api

// Request and response types for REST API

// Camera Control

type ConnectResponse struct {
	Status  string `json:"status"`
	Battery int    `json:"battery"`
}

type DisconnectResponse struct {
	Status string `json:"status"`
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
	ISO int `json:"iso"`
}

type ISOSetRequest struct {
	ISO int `json:"iso"`
}

type ISOSetResponse struct {
	ISO    int    `json:"iso"`
	Status string `json:"status"`
}

type ShutterGetResponse struct {
	ShutterUS      int    `json:"shutter_us"`
	ShutterDisplay string `json:"shutter_display"`
}

type ShutterSetRequest struct {
	Shutter   string `json:"shutter,omitempty"`    // e.g., "1/250" or "2s"
	ShutterUS int    `json:"shutter_us,omitempty"` // microseconds
}

type ShutterSetResponse struct {
	ShutterUS      int    `json:"shutter_us"`
	ShutterDisplay string `json:"shutter_display,omitempty"` // Human-readable format (e.g., "1/125s")
	Status         string `json:"status"`
}

type FocusGetResponse struct {
	FocusMode string `json:"focus_mode"` // "manual", "auto"
}

type FocusSetRequest struct {
	FocusMode string `json:"focus_mode"` // "manual", "auto"
}

type FocusSetResponse struct {
	FocusMode string `json:"focus_mode"`
	Status    string `json:"status"`
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
	Status string `json:"status"`
}

// Capture

type CaptureSingleResponse struct {
	Filename string `json:"filename"`
	Size     int64  `json:"size"`
	Status   string `json:"status"`
}

type CaptureStartRequest struct {
	Count int  `json:"count"` // 0 for infinite
	Delay int  `json:"delay"` // seconds
	Async bool `json:"async"`
}

type CaptureStartResponse struct {
	Status      string `json:"status"`
	TotalFrames int    `json:"total_frames"`
}

type CapturePauseResponse struct {
	Status string `json:"status"`
	Frame  int    `json:"frame"`
	Total  int    `json:"total"`
}

type CaptureResumeResponse struct {
	Status string `json:"status"`
	Frame  int    `json:"frame"`
	Total  int    `json:"total"`
}

type CaptureStopResponse struct {
	Status string `json:"status"`
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
	Status string `json:"status"`
}

type LiveViewStopResponse struct {
	Status string `json:"status"`
}

// Error Response

type ErrorResponse struct {
	Error   string `json:"error"`
	Message string `json:"message,omitempty"`
	Code    int    `json:"code,omitempty"`
}
