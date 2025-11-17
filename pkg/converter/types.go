package converter

// Metadata represents EXIF and camera metadata from a RAW file
type Metadata struct {
	ExposureTime float64 // Exposure time in seconds
	ISO          int     // ISO sensitivity
	Timestamp    string  // Capture timestamp (ISO 8601 format)
	CameraMake   string  // Camera manufacturer
	CameraModel  string  // Camera model
	Width        int     // Image width in pixels
	Height       int     // Image height in pixels
	PixelWidth   float32 // Pixel width in microns
	PixelHeight  float32 // Pixel height in microns
	RawWidth     int     // Raw image width (before crop)
	RawHeight    int     // Raw image height (before crop)
}

// RGBImage represents a processed RGB image with 16-bit channels
type RGBImage struct {
	RChannel []uint16 // Red channel data
	GChannel []uint16 // Green channel data
	BChannel []uint16 // Blue channel data
	Width    int      // Image width
	Height   int      // Image height
}
