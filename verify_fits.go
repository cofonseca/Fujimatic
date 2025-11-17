// +build ignore

package main

import (
	"encoding/binary"
	"fmt"
	"os"
)

func main() {
	if len(os.Args) != 2 {
		fmt.Println("Usage: go run verify_fits.go <fits_file>")
		os.Exit(1)
	}

	filename := os.Args[1]
	fmt.Printf("=== Verifying FITS: %s ===\n\n", filename)

	f, err := os.Open(filename)
	if err != nil {
		fmt.Printf("ERROR: %v\n", err)
		os.Exit(1)
	}
	defer f.Close()

	// Get file size
	info, _ := f.Stat()
	fmt.Printf("File size: %d bytes (%.1f MB)\n\n", info.Size(), float64(info.Size())/1024/1024)

	// Read header (first 2880 bytes)
	header := make([]byte, 2880)
	n, err := f.Read(header)
	if err != nil || n != 2880 {
		fmt.Printf("ERROR: Failed to read header\n")
		os.Exit(1)
	}

	fmt.Println("=== FITS Header ===")
	// Print key header cards
	keys := []string{"SIMPLE", "BITPIX", "NAXIS", "NAXIS1", "NAXIS2", "NAXIS3", "EXPTIME", "ISO"}
	for _, key := range keys {
		for i := 0; i < len(header); i += 80 {
			card := string(header[i : i+80])
			if len(card) >= len(key) && card[:len(key)] == key {
				fmt.Println(card)
				break
			}
		}
	}
	fmt.Println()

	// Calculate expected dimensions
	width := 6246
	height := 4170
	channels := 3
	channelSize := width * height * 2 // 2 bytes per pixel (16-bit)

	fmt.Println("=== Channel Data Verification ===")
	fmt.Printf("Expected: %d x %d x %d channels\n", width, height, channels)
	fmt.Printf("Channel size: %d bytes (%.1f MB)\n\n", channelSize, float64(channelSize)/1024/1024)

	// Read samples from each channel
	sampleCount := 10
	samples := make([]uint16, sampleCount)

	for ch := 0; ch < channels; ch++ {
		channelName := []string{"Red", "Green", "Blue"}[ch]
		offset := int64(2880 + ch*channelSize)

		// Seek to channel start
		_, err := f.Seek(offset, 0)
		if err != nil {
			fmt.Printf("ERROR seeking to %s channel: %v\n", channelName, err)
			continue
		}

		// Read sample pixels
		for i := 0; i < sampleCount; i++ {
			var pixel uint16
			err := binary.Read(f, binary.BigEndian, &pixel)
			if err != nil {
				fmt.Printf("ERROR reading pixel: %v\n", err)
				break
			}
			samples[i] = pixel
		}

		// Calculate statistics
		var min, max, sum uint16 = 65535, 0, 0
		for _, v := range samples {
			if v < min {
				min = v
			}
			if v > max {
				max = v
			}
			sum += v
		}
		avg := float64(sum) / float64(sampleCount)

		fmt.Printf("%s Channel (offset: %d):\n", channelName, offset)
		fmt.Printf("  First 10 pixels: %v\n", samples)
		fmt.Printf("  Range: %d - %d, Average: %.1f\n", min, max, avg)
		fmt.Println()
	}

	fmt.Println("=== Verification Complete ===")
	fmt.Println("\nNote: All 3 channels should have DIFFERENT values for a color image.")
	fmt.Println("If all channels are identical, the image will appear monochrome.")
}
