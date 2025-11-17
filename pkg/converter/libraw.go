// +build cgo

package converter

/*
#cgo CFLAGS: -I../../sdk-c-wrapper -I/c/msys64/mingw64/include/libraw
#cgo LDFLAGS: -L../../libs/windows -lraw_wrapper -L/c/msys64/mingw64/lib -lraw

#include <stdlib.h>
#include "libraw_wrapper.h"
*/
import "C"

import (
	"fmt"
	"unsafe"
)

// RealConverter implements RAF to FITS conversion using LibRaw
type RealConverter struct {
	initialized bool
	fileOpen    bool
}

// NewRealConverter creates a new LibRaw-based converter
func NewRealConverter() *RealConverter {
	return &RealConverter{
		initialized: false,
		fileOpen:    false,
	}
}

// Init initializes the LibRaw library
func (c *RealConverter) Init() error {
	result := C.lr_init()
	if result != 0 {
		return fmt.Errorf("LibRaw initialization failed with code: %d", result)
	}
	c.initialized = true
	return nil
}

// SetVerbose enables or disables verbose logging in the LibRaw wrapper
func (c *RealConverter) SetVerbose(enabled bool) {
	var cEnabled C.int
	if enabled {
		cEnabled = 1
	} else {
		cEnabled = 0
	}
	C.lr_set_verbose(cEnabled)
}

// OpenFile opens a RAW file and reads its metadata
func (c *RealConverter) OpenFile(filename string) error {
	if !c.initialized {
		return fmt.Errorf("converter not initialized, call Init() first")
	}

	if filename == "" {
		return fmt.Errorf("filename cannot be empty")
	}

	cFilename := C.CString(filename)
	defer C.free(unsafe.Pointer(cFilename))

	result := C.lr_open_file(cFilename)
	if result != 0 {
		return fmt.Errorf("failed to open file '%s' with code: %d", filename, result)
	}

	c.fileOpen = true
	return nil
}

// Unpack unpacks the raw image data from the opened file
func (c *RealConverter) Unpack() error {
	if !c.fileOpen {
		return fmt.Errorf("no file open, call OpenFile() first")
	}

	result := C.lr_unpack()
	if result != 0 {
		return fmt.Errorf("failed to unpack raw data with code: %d", result)
	}

	return nil
}

// Process processes the raw data (demosaic, white balance, color correction)
func (c *RealConverter) Process() error {
	if !c.fileOpen {
		return fmt.Errorf("no file open, call OpenFile() first")
	}

	result := C.lr_process()
	if result != 0 {
		return fmt.Errorf("failed to process raw data with code: %d", result)
	}

	return nil
}

// GetRGBData retrieves the processed RGB image data (16-bit per channel, planar format)
func (c *RealConverter) GetRGBData() (*RGBImage, error) {
	if !c.fileOpen {
		return nil, fmt.Errorf("no file open, call OpenFile() first")
	}

	var cImage C.lr_rgb_image_t

	result := C.lr_get_rgb_data(&cImage)
	if result != 0 {
		return nil, fmt.Errorf("failed to get RGB data with code: %d", result)
	}

	// Ensure cleanup happens
	defer C.lr_free_rgb_data(&cImage)

	width := int(cImage.width)
	height := int(cImage.height)
	pixelCount := width * height

	// Allocate Go slices
	image := &RGBImage{
		RChannel: make([]uint16, pixelCount),
		GChannel: make([]uint16, pixelCount),
		BChannel: make([]uint16, pixelCount),
		Width:    width,
		Height:   height,
	}

	// Copy data from C arrays to Go slices
	// Convert from C unsigned short to Go uint16
	rSlice := unsafe.Slice(cImage.r_channel, pixelCount)
	gSlice := unsafe.Slice(cImage.g_channel, pixelCount)
	bSlice := unsafe.Slice(cImage.b_channel, pixelCount)

	for i := 0; i < pixelCount; i++ {
		image.RChannel[i] = uint16(rSlice[i])
		image.GChannel[i] = uint16(gSlice[i])
		image.BChannel[i] = uint16(bSlice[i])
	}

	return image, nil
}

// GetMetadata retrieves metadata from the RAW file for FITS headers
func (c *RealConverter) GetMetadata() (*Metadata, error) {
	if !c.fileOpen {
		return nil, fmt.Errorf("no file open, call OpenFile() first")
	}

	var cMetadata C.lr_metadata_t

	result := C.lr_get_metadata(&cMetadata)
	if result != 0 {
		return nil, fmt.Errorf("failed to get metadata with code: %d", result)
	}

	// Convert C struct to Go struct
	metadata := &Metadata{
		ExposureTime: float64(cMetadata.exposure_time),
		ISO:          int(cMetadata.iso),
		Timestamp:    C.GoString(&cMetadata.timestamp[0]),
		CameraMake:   C.GoString(&cMetadata.camera_make[0]),
		CameraModel:  C.GoString(&cMetadata.camera_model[0]),
		Width:        int(cMetadata.width),
		Height:       int(cMetadata.height),
		PixelWidth:   float32(cMetadata.pixel_width),
		PixelHeight:  float32(cMetadata.pixel_height),
		RawWidth:     int(cMetadata.raw_width),
		RawHeight:    int(cMetadata.raw_height),
	}

	return metadata, nil
}

// Close cleans up and closes the currently opened file
func (c *RealConverter) Close() {
	if c.initialized {
		C.lr_cleanup()
		c.fileOpen = false
	}
}

// ConvertRAFtoFITS is a high-level convenience function that performs the full conversion pipeline to FITS
func (c *RealConverter) ConvertRAFtoFITS(rafPath, fitsPath string) error {
	return c.convertRAF(rafPath, fitsPath, "fits")
}

// ConvertRAFtoTIFF is a high-level convenience function that performs the full conversion pipeline to TIFF
func (c *RealConverter) ConvertRAFtoTIFF(rafPath, tiffPath string) error {
	return c.convertRAF(rafPath, tiffPath, "tiff")
}

// convertRAF is the internal conversion implementation
func (c *RealConverter) convertRAF(rafPath, outputPath, format string) error {
	// Ensure cleanup happens even if we panic
	defer c.Close()

	// Open the RAF file
	if err := c.OpenFile(rafPath); err != nil {
		return fmt.Errorf("failed to open RAF file: %w", err)
	}

	// Get metadata
	metadata, err := c.GetMetadata()
	if err != nil {
		return fmt.Errorf("failed to get metadata: %w", err)
	}

	// Unpack raw data
	if err := c.Unpack(); err != nil {
		return fmt.Errorf("failed to unpack raw data: %w", err)
	}

	// Process raw data
	if err := c.Process(); err != nil {
		return fmt.Errorf("failed to process raw data: %w", err)
	}

	// Get RGB image data
	rgbImage, err := c.GetRGBData()
	if err != nil {
		return fmt.Errorf("failed to get RGB data: %w", err)
	}

	// Write output file in requested format
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

	return nil
}

// ExtractThumbnail extracts the embedded JPEG thumbnail from a RAF file
// This is much faster than processing the full RAW image and is suitable for preview purposes
func ExtractThumbnail(rafPath string) ([]byte, error) {
	if rafPath == "" {
		return nil, fmt.Errorf("rafPath cannot be empty")
	}

	cFilename := C.CString(rafPath)
	defer C.free(unsafe.Pointer(cFilename))

	var cThumbData *C.uchar
	var cThumbSize C.int

	result := C.lr_extract_thumbnail(cFilename, &cThumbData, &cThumbSize)
	if result != 0 {
		return nil, fmt.Errorf("failed to extract thumbnail from '%s' with code: %d", rafPath, result)
	}

	// Convert C buffer to Go slice
	thumbData := C.GoBytes(unsafe.Pointer(cThumbData), cThumbSize)

	// Free the C buffer
	C.free(unsafe.Pointer(cThumbData))

	return thumbData, nil
}
