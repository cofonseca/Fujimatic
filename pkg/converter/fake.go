// +build !cgo

package converter

import (
	"fmt"
	"time"
)

// FakeConverter provides a fake implementation for testing without LibRaw
type FakeConverter struct {
	initialized bool
	fileOpen    bool
	filename    string
	verbose     bool
}

// NewFakeConverter creates a new fake converter instance
func NewFakeConverter() *FakeConverter {
	return &FakeConverter{
		initialized: false,
		fileOpen:    false,
		verbose:     false,
	}
}

// Init initializes the fake converter
func (c *FakeConverter) Init() error {
	// Simulate initialization delay
	time.Sleep(10 * time.Millisecond)
	c.initialized = true
	if c.verbose {
		fmt.Println("FakeConverter: Initialized (simulated)")
	}
	return nil
}

// SetVerbose enables or disables verbose logging
func (c *FakeConverter) SetVerbose(enabled bool) {
	c.verbose = enabled
	if c.verbose {
		fmt.Println("FakeConverter: Verbose logging enabled")
	}
}

// OpenFile simulates opening a RAW file
func (c *FakeConverter) OpenFile(filename string) error {
	if !c.initialized {
		return fmt.Errorf("converter not initialized, call Init() first")
	}

	if filename == "" {
		return fmt.Errorf("filename cannot be empty")
	}

	// Simulate file opening delay
	time.Sleep(20 * time.Millisecond)

	c.filename = filename
	c.fileOpen = true

	if c.verbose {
		fmt.Printf("FakeConverter: Opened file '%s' (simulated)\n", filename)
	}

	return nil
}

// Unpack simulates unpacking raw data
func (c *FakeConverter) Unpack() error {
	if !c.fileOpen {
		return fmt.Errorf("no file open, call OpenFile() first")
	}

	// Simulate unpacking delay (LibRaw takes ~200ms)
	time.Sleep(200 * time.Millisecond)

	if c.verbose {
		fmt.Println("FakeConverter: Unpacked raw data (simulated)")
	}

	return nil
}

// Process simulates processing raw data
func (c *FakeConverter) Process() error {
	if !c.fileOpen {
		return fmt.Errorf("no file open, call OpenFile() first")
	}

	// Simulate processing delay (LibRaw takes ~800ms for demosaicing)
	time.Sleep(800 * time.Millisecond)

	if c.verbose {
		fmt.Println("FakeConverter: Processed raw data (simulated)")
	}

	return nil
}

// GetRGBData simulates getting processed RGB data
func (c *FakeConverter) GetRGBData() (*RGBImage, error) {
	if !c.fileOpen {
		return nil, fmt.Errorf("no file open, call OpenFile() first")
	}

	// Create fake image data (small test image: 100x100 pixels)
	width := 100
	height := 100
	pixelCount := width * height

	// Simulate data extraction delay
	time.Sleep(50 * time.Millisecond)

	image := &RGBImage{
		RChannel: make([]uint16, pixelCount),
		GChannel: make([]uint16, pixelCount),
		BChannel: make([]uint16, pixelCount),
		Width:    width,
		Height:   height,
	}

	// Fill with fake gradient data
	for i := 0; i < pixelCount; i++ {
		// Create a simple gradient pattern
		image.RChannel[i] = uint16((i % width) * 655) // 0-65535 range
		image.GChannel[i] = uint16((i / width) * 655)
		image.BChannel[i] = uint16(32768) // Mid-gray
	}

	if c.verbose {
		fmt.Printf("FakeConverter: Generated fake RGB data (%dx%d pixels)\n", width, height)
	}

	return image, nil
}

// GetMetadata simulates getting metadata
func (c *FakeConverter) GetMetadata() (*Metadata, error) {
	if !c.fileOpen {
		return nil, fmt.Errorf("no file open, call OpenFile() first")
	}

	// Simulate metadata extraction delay
	time.Sleep(10 * time.Millisecond)

	// Generate fake but realistic metadata
	metadata := &Metadata{
		ExposureTime: 30.0,                              // 30 second exposure (typical for astrophotography)
		ISO:          800,                               // ISO 800
		Timestamp:    time.Now().UTC().Format(time.RFC3339), // Current time in ISO 8601
		CameraMake:   "FUJIFILM",
		CameraModel:  "X-T3",
		Width:        100, // Match fake image size
		Height:       100,
		PixelWidth:   3.76, // X-T3 pixel size in microns
		PixelHeight:  3.76,
		RawWidth:     100,
		RawHeight:    100,
	}

	if c.verbose {
		fmt.Printf("FakeConverter: Generated fake metadata (exposure=%.1fs, ISO=%d)\n",
			metadata.ExposureTime, metadata.ISO)
	}

	return metadata, nil
}

// Close simulates cleanup
func (c *FakeConverter) Close() {
	if c.initialized {
		if c.verbose {
			fmt.Println("FakeConverter: Closed (simulated)")
		}
		c.fileOpen = false
		c.filename = ""
	}
}

// ConvertRAFtoFITS performs the full conversion pipeline to FITS (fake)
func (c *FakeConverter) ConvertRAFtoFITS(rafPath, fitsPath string) error {
	return c.convertRAF(rafPath, fitsPath, "fits")
}

// ConvertRAFtoTIFF performs the full conversion pipeline to TIFF (fake)
func (c *FakeConverter) ConvertRAFtoTIFF(rafPath, tiffPath string) error {
	return c.convertRAF(rafPath, tiffPath, "tiff")
}

// convertRAF is the internal conversion implementation (fake)
func (c *FakeConverter) convertRAF(rafPath, outputPath, format string) error {
	defer c.Close()

	if c.verbose {
		fmt.Printf("FakeConverter: Starting %s conversion '%s' → '%s' (simulated)\n", format, rafPath, outputPath)
	}

	// Open file
	if err := c.OpenFile(rafPath); err != nil {
		return fmt.Errorf("failed to open RAF file: %w", err)
	}

	// Get metadata
	metadata, err := c.GetMetadata()
	if err != nil {
		return fmt.Errorf("failed to get metadata: %w", err)
	}

	// Unpack
	if err := c.Unpack(); err != nil {
		return fmt.Errorf("failed to unpack raw data: %w", err)
	}

	// Process
	if err := c.Process(); err != nil {
		return fmt.Errorf("failed to process raw data: %w", err)
	}

	// Get RGB data
	rgbImage, err := c.GetRGBData()
	if err != nil {
		return fmt.Errorf("failed to get RGB data: %w", err)
	}

	// Write output file in requested format (real file writing, not simulated)
	switch format {
	case "fits":
		if err := WriteFITS(outputPath, rgbImage, metadata); err != nil {
			return fmt.Errorf("failed to write FITS file: %w", err)
		}
	case "tiff":
		if err := WriteTIFF(outputPath, rgbImage, metadata); err != nil {
			return fmt.Errorf("failed to write TIFF file: %w", err)
		}
	default:
		return fmt.Errorf("unsupported format: %s", format)
	}

	if c.verbose {
		fmt.Printf("FakeConverter: Conversion complete → '%s'\n", outputPath)
	}

	return nil
}

// NewRealConverter is aliased to NewFakeConverter when cgo is disabled
// This allows the same API to work in both build modes
func NewRealConverter() *FakeConverter {
	return NewFakeConverter()
}

// ExtractThumbnail returns a fake 1x1 JPEG thumbnail (when CGO is disabled)
func ExtractThumbnail(rafPath string) ([]byte, error) {
	// Return a minimal valid JPEG (1x1 pixel black image)
	// This is a valid JPEG file that browsers can display
	return []byte{
		0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01,
		0x01, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0xFF, 0xDB, 0x00, 0x43,
		0x00, 0x08, 0x06, 0x06, 0x07, 0x06, 0x05, 0x08, 0x07, 0x07, 0x07, 0x09,
		0x09, 0x08, 0x0A, 0x0C, 0x14, 0x0D, 0x0C, 0x0B, 0x0B, 0x0C, 0x19, 0x12,
		0x13, 0x0F, 0x14, 0x1D, 0x1A, 0x1F, 0x1E, 0x1D, 0x1A, 0x1C, 0x1C, 0x20,
		0x24, 0x2E, 0x27, 0x20, 0x22, 0x2C, 0x23, 0x1C, 0x1C, 0x28, 0x37, 0x29,
		0x2C, 0x30, 0x31, 0x34, 0x34, 0x34, 0x1F, 0x27, 0x39, 0x3D, 0x38, 0x32,
		0x3C, 0x2E, 0x33, 0x34, 0x32, 0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x00, 0x01,
		0x00, 0x01, 0x01, 0x01, 0x11, 0x00, 0xFF, 0xC4, 0x00, 0x14, 0x00, 0x01,
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x03, 0xFF, 0xC4, 0x00, 0x14, 0x10, 0x01, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0xFF, 0xDA, 0x00, 0x08, 0x01, 0x01, 0x00, 0x00, 0x3F, 0x00,
		0x37, 0xFF, 0xD9,
	}, nil
}
