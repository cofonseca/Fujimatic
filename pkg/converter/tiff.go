package converter

import (
	"fmt"
	"image"
	"os"

	"golang.org/x/image/tiff"
)

// WriteTIFF writes an RGB image and metadata to a TIFF file with LZW compression
func WriteTIFF(filename string, img *RGBImage, metadata *Metadata) error {
	if img == nil {
		return fmt.Errorf("image cannot be nil")
	}

	if metadata == nil {
		return fmt.Errorf("metadata cannot be nil")
	}

	// Create output file
	f, err := os.Create(filename)
	if err != nil {
		return fmt.Errorf("failed to create TIFF file: %w", err)
	}
	defer f.Close()

	// Create 16-bit RGB image
	// Use RGBA64 (Go's 16-bit format)
	rgbaImg := image.NewRGBA64(image.Rect(0, 0, img.Width, img.Height))

	// Convert planar RGB to interleaved RGBA
	// Note: Unlike FITS, standard image formats use top-left origin by default
	// So we don't need to flip for TIFF
	for y := 0; y < img.Height; y++ {
		srcRowStart := y * img.Width

		for x := 0; x < img.Width; x++ {
			srcIdx := srcRowStart + x
			dstIdx := rgbaImg.PixOffset(x, y)

			// Get RGB values from planar channels
			r := img.RChannel[srcIdx]
			g := img.GChannel[srcIdx]
			b := img.BChannel[srcIdx]

			// Write as RGBA64 (16-bit per channel, big-endian)
			// Alpha channel = 0xFFFF (fully opaque)
			rgbaImg.Pix[dstIdx+0] = byte(r >> 8)
			rgbaImg.Pix[dstIdx+1] = byte(r & 0xFF)
			rgbaImg.Pix[dstIdx+2] = byte(g >> 8)
			rgbaImg.Pix[dstIdx+3] = byte(g & 0xFF)
			rgbaImg.Pix[dstIdx+4] = byte(b >> 8)
			rgbaImg.Pix[dstIdx+5] = byte(b & 0xFF)
			rgbaImg.Pix[dstIdx+6] = 0xFF // Alpha high byte
			rgbaImg.Pix[dstIdx+7] = 0xFF // Alpha low byte
		}
	}

	// Encode with Deflate compression
	// Deflate (zlib) provides better compression than LZW for 16-bit images
	err = tiff.Encode(f, rgbaImg, &tiff.Options{
		Compression: tiff.Deflate,
	})

	if err != nil {
		return fmt.Errorf("failed to encode TIFF: %w", err)
	}

	return nil
}
