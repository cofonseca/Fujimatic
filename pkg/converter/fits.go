package converter

import (
	"fmt"
	"os"
	"strings"
)

// WriteFITS writes an RGB image and metadata to a FITS file
// FITS format: https://fits.gsfc.nasa.gov/fits_standard.html
func WriteFITS(filename string, image *RGBImage, metadata *Metadata) error {
	if image == nil {
		return fmt.Errorf("image cannot be nil")
	}

	if metadata == nil {
		return fmt.Errorf("metadata cannot be nil")
	}

	// Create output file
	f, err := os.Create(filename)
	if err != nil {
		return fmt.Errorf("failed to create FITS file: %w", err)
	}
	defer f.Close()

	// Write primary HDU (Header + Data Unit)
	if err := writePrimaryHDU(f, image, metadata); err != nil {
		return fmt.Errorf("failed to write primary HDU: %w", err)
	}

	return nil
}

// writePrimaryHDU writes the primary Header + Data Unit
func writePrimaryHDU(f *os.File, image *RGBImage, metadata *Metadata) error {
	// Build FITS header
	header := buildFITSHeader(image, metadata)

	// Write header (must be multiple of 2880 bytes)
	if _, err := f.Write(header); err != nil {
		return fmt.Errorf("failed to write header: %w", err)
	}

	// Write image data (planar RGB, 16-bit big-endian)
	if err := writeImageData(f, image); err != nil {
		return fmt.Errorf("failed to write image data: %w", err)
	}

	return nil
}

// buildFITSHeader creates a FITS header with metadata
func buildFITSHeader(image *RGBImage, metadata *Metadata) []byte {
	// FITS header is 80-character records (36 cards per block)
	// Each block is 2880 bytes (36 * 80)

	cards := []string{
		// Mandatory keywords
		formatCard("SIMPLE", "T", "file conforms to FITS standard"),
		formatCard("BITPIX", "16", "bits per pixel"),
		formatCard("NAXIS", "3", "number of axes"),
		formatCard("NAXIS1", fmt.Sprintf("%d", image.Width), "width in pixels"),
		formatCard("NAXIS2", fmt.Sprintf("%d", image.Height), "height in pixels"),
		formatCard("NAXIS3", "3", "number of color channels (RGB)"),

		// Optional keywords (astrophotography metadata)
		formatCard("EXPTIME", fmt.Sprintf("%.6f", metadata.ExposureTime), "exposure time in seconds"),
		formatCard("ISO", fmt.Sprintf("%d", metadata.ISO), "ISO sensitivity"),
		formatCard("DATE-OBS", fmt.Sprintf("'%s'", metadata.Timestamp), "observation timestamp (UTC)"),
		formatCard("INSTRUME", fmt.Sprintf("'%s %s'", metadata.CameraMake, metadata.CameraModel), "camera model"),
		formatCard("IMAGETYP", "'Light Frame'", "type of image"),
		formatCard("BAYERPAT", "'X-TRANS'", "Fujifilm X-Trans color filter array"),
		formatCard("XPIXSZ", fmt.Sprintf("%.6f", metadata.PixelWidth), "pixel width in microns"),
		formatCard("YPIXSZ", fmt.Sprintf("%.6f", metadata.PixelHeight), "pixel height in microns"),

		// Custom keywords
		formatCard("CREATED", "'fujimatic'", "created by fujimatic RAF converter"),

		// END keyword (required)
		"END" + strings.Repeat(" ", 77),
	}

	// Calculate total size (must be multiple of 2880)
	totalCards := len(cards)
	cardsPerBlock := 36
	blocksNeeded := (totalCards + cardsPerBlock - 1) / cardsPerBlock // Round up
	totalSize := blocksNeeded * 2880

	// Build header buffer
	header := make([]byte, totalSize)

	// Write cards
	offset := 0
	for _, card := range cards {
		copy(header[offset:], card)
		offset += 80
	}

	// Fill remaining space with spaces
	for i := offset; i < totalSize; i++ {
		header[i] = ' '
	}

	return header
}

// formatCard formats a FITS header card (80 characters)
// Format: "KEYWORD = VALUE / COMMENT"
func formatCard(keyword, value, comment string) string {
	// FITS keyword format: left-justified, max 8 chars
	if len(keyword) > 8 {
		keyword = keyword[:8]
	}

	// Build card
	var card string
	if comment != "" {
		card = fmt.Sprintf("%-8s= %20s / %s", keyword, value, comment)
	} else {
		card = fmt.Sprintf("%-8s= %20s", keyword, value)
	}

	// Pad or truncate to exactly 80 characters
	if len(card) > 80 {
		card = card[:80]
	} else {
		card = card + strings.Repeat(" ", 80-len(card))
	}

	return card
}

// writeImageData writes 16-bit RGB image data in planar format
// FITS standard requires big-endian byte order
// Images are written top-down (flipped vertically from LibRaw's bottom-up orientation)
func writeImageData(f *os.File, image *RGBImage) error {
	pixelCount := image.Width * image.Height

	// Write R channel (vertically flipped)
	if err := writeChannelFlipped(f, image.RChannel, image.Width, image.Height); err != nil {
		return fmt.Errorf("failed to write R channel: %w", err)
	}

	// Write G channel (vertically flipped)
	if err := writeChannelFlipped(f, image.GChannel, image.Width, image.Height); err != nil {
		return fmt.Errorf("failed to write G channel: %w", err)
	}

	// Write B channel (vertically flipped)
	if err := writeChannelFlipped(f, image.BChannel, image.Width, image.Height); err != nil {
		return fmt.Errorf("failed to write B channel: %w", err)
	}

	// FITS data section must be padded to multiple of 2880 bytes
	dataSize := pixelCount * 3 * 2 // 3 channels × 2 bytes per pixel
	paddingNeeded := (2880 - (dataSize % 2880)) % 2880

	if paddingNeeded > 0 {
		padding := make([]byte, paddingNeeded)
		if _, err := f.Write(padding); err != nil {
			return fmt.Errorf("failed to write padding: %w", err)
		}
	}

	return nil
}

// writeChannelFlipped writes a single 16-bit channel in big-endian format
// with vertical flip (rows written bottom-to-top for correct orientation)
func writeChannelFlipped(f *os.File, channel []uint16, width, height int) error {
	// Pre-allocate byte buffer for entire channel (2 bytes per pixel)
	buf := make([]byte, len(channel)*2)

	// Write rows in reverse order (vertical flip)
	// This converts from LibRaw's bottom-up to standard top-down orientation
	bufOffset := 0
	for row := height - 1; row >= 0; row-- {
		rowStart := row * width
		rowEnd := rowStart + width

		// Convert pixels in this row to big-endian
		for _, pixel := range channel[rowStart:rowEnd] {
			buf[bufOffset] = byte(pixel >> 8)      // High byte
			buf[bufOffset+1] = byte(pixel & 0xFF) // Low byte
			bufOffset += 2
		}
	}

	// Write entire buffer at once
	if _, err := f.Write(buf); err != nil {
		return fmt.Errorf("failed to write channel: %w", err)
	}

	return nil
}
