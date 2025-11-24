package api

import (
	"fmt"
	"strconv"
	"strings"
)

// formatShutterSpeed converts microseconds to a display string
// Maps to standard Fuji camera shutter speeds instead of mathematical rounding
func formatShutterSpeed(microseconds int) string {
	return getPhotographicFraction(microseconds)
}

// abs returns the absolute value of an integer
func abs(x int) int {
	if x < 0 {
		return -x
	}
	return x
}

// Standard photographic shutter speeds (in microseconds) as shown on Fuji cameras
// Includes all T-mode values for astrophotography long exposures
var standardShutterSpeeds = []struct {
	microseconds int
	display      string
}{
	// Fast shutter speeds (fractions)
	{125, "1/8000"},       // 1/8000 second
	{250, "1/4000"},       // 1/4000 second
	{500, "1/2000"},       // 1/2000 second
	{1000, "1/1000"},      // 1/1000 second
	{2000, "1/500"},       // 1/500 second
	{4000, "1/250"},       // 1/250 second
	{8000, "1/125"},       // 1/125 second
	{16667, "1/60"},       // 1/60 second
	{33333, "1/30"},       // 1/30 second
	{66667, "1/15"},       // 1/15 second
	{125000, "1/8"},       // 1/8 second
	{250000, "1/4"},       // 1/4 second
	{500000, "1/2"},       // 1/2 second
	// T-mode shutter speeds (whole seconds and fractions)
	{1000000, "1"},        // 1 second
	{1300000, "1.3"},      // 1.3 seconds
	{1500000, "1.5"},      // 1.5 seconds
	{2000000, "2"},        // 2 seconds
	{2500000, "2.5"},      // 2.5 seconds
	{3000000, "3"},        // 3 seconds
	{4000000, "4"},        // 4 seconds
	{5000000, "5"},        // 5 seconds
	{6500000, "6.5"},      // 6.5 seconds
	{8000000, "8"},        // 8 seconds
	{10000000, "10"},      // 10 seconds
	{13000000, "13"},      // 13 seconds
	{15000000, "15"},      // 15 seconds (SDK: 16000000)
	{16000000, "15"},      // 15 seconds (actual SDK value)
	{20000000, "20"},      // 20 seconds
	{25000000, "25"},      // 25 seconds
	{30000000, "30"},      // 30 seconds (SDK: 32000000)
	{32000000, "30"},      // 30 seconds (actual SDK value)
	{40000000, "40"},      // 40 seconds
	{50000000, "50"},      // 50 seconds
	{60000000, "60"},      // 1 minute
}

// Extended T-mode shutter speeds (specially encoded SDK values, NOT microseconds)
// These values are natively supported by the camera for long exposures > 1 minute
// The encoding follows a special SDK format: 64000xxx where xxx indicates the duration
var extendedTModeShutterSpeeds = []struct {
	sdkValue int    // Raw SDK value (NOT microseconds)
	display  string // Human-readable display
	seconds  int    // Duration in seconds (for reference)
}{
	{64000000, "1m", 60},    // 1 minute
	{64000030, "2m", 120},   // 2 minutes
	{64000060, "4m", 240},   // 4 minutes
	{64000090, "8m", 480},   // 8 minutes
	{64000120, "15m", 900},  // 15 minutes
}

// getPhotographicFraction converts microseconds/SDK values to a photographic fraction string
// Maps to standard Fuji camera shutter speeds instead of mathematical rounding
// Also handles extended T-mode values (64000xxx encoded format)
func getPhotographicFraction(value int) string {
	// First, check if this is an extended T-mode value (64000xxx format)
	if value >= 64000000 && value < 65000000 {
		for _, speed := range extendedTModeShutterSpeeds {
			if speed.sdkValue == value {
				return speed.display
			}
		}
		// Unknown extended T-mode value, return raw
		return fmt.Sprintf("%d (extended)", value)
	}

	// Standard shutter speed (microseconds)
	// Find the closest standard shutter speed
	closest := standardShutterSpeeds[0]
	minDiff := abs(value - closest.microseconds)

	for _, speed := range standardShutterSpeeds[1:] {
		diff := abs(value - speed.microseconds)
		if diff < minDiff {
			closest = speed
			minDiff = diff
		}
	}

	return closest.display
}

// ShutterSpeedToMicroseconds converts a shutter speed string to microseconds
// This is used internally by the client library to parse API responses
func ShutterSpeedToMicroseconds(shutterSpeed string) (int, error) {
	return parseShutterSpeed(shutterSpeed)
}

// parseShutterSpeed parses a shutter speed string to the appropriate SDK value
// For standard speeds, returns microseconds. For extended T-mode, returns SDK encoded values.
// Supported formats:
// - "1/250", "1/60" (fractions)
// - "1", "2", "30" (seconds)
// - "1m", "2m", "4m", "8m", "15m" (extended T-mode minutes)
func parseShutterSpeed(input string) (int, error) {
	// First, check for extended T-mode format (e.g., "2m", "4m", "8m", "15m")
	if strings.HasSuffix(input, "m") && !strings.Contains(input, "/") {
		// Extract the number before 'm'
		minuteStr := strings.TrimSuffix(input, "m")
		minutes, err := strconv.Atoi(minuteStr)
		if err != nil {
			return 0, fmt.Errorf("invalid minute format: %s", input)
		}

		// Map to extended T-mode SDK values
		for _, speed := range extendedTModeShutterSpeeds {
			if speed.seconds == minutes*60 {
				return speed.sdkValue, nil
			}
		}

		// Not a known extended T-mode value
		return 0, fmt.Errorf("unsupported extended T-mode duration: %s (supported: 1m, 2m, 4m, 8m, 15m)", input)
	}

	// Standard shutter speed - convert to microseconds
	seconds, err := parseShutterDuration(input)
	if err != nil {
		return 0, err
	}
	return int(seconds * 1000000), nil
}

// parseShutterDuration parses a shutter speed string to seconds (float64)
// Supports both decimal and fraction formats:
// - "0.5s" or "0.5" (decimal seconds)
// - "1/30s" or "1/30" (photographic fractions)
// - "1/4s" or "1/4" (fractional seconds)
// NOTE: Does NOT handle extended T-mode ("2m", "4m", etc.) - use parseShutterSpeed instead
func parseShutterDuration(s string) (float64, error) {
	// Handle optional 's' suffix
	if strings.HasSuffix(s, "s") {
		s = s[:len(s)-1]
	}

	var seconds float64
	var err error

	// Check if this is a fraction format (contains '/')
	if strings.Contains(s, "/") {
		// Parse as fraction (e.g., "1/30" -> 1/30 = 0.0333...)
		parts := strings.Split(s, "/")
		if len(parts) != 2 {
			return 0, fmt.Errorf("invalid shutter duration format: %s", s)
		}

		// Parse numerator and denominator
		var numerator, denominator float64
		numerator, err = strconv.ParseFloat(strings.TrimSpace(parts[0]), 64)
		if err != nil {
			return 0, fmt.Errorf("invalid fraction numerator: %s", parts[0])
		}

		denominator, err = strconv.ParseFloat(strings.TrimSpace(parts[1]), 64)
		if err != nil {
			return 0, fmt.Errorf("invalid fraction denominator: %s", parts[1])
		}

		// Validate denominator
		if denominator <= 0 {
			return 0, fmt.Errorf("fraction denominator must be positive: %s", s)
		}

		// Calculate fraction value
		seconds = numerator / denominator
	} else {
		// Parse as decimal (e.g., "0.5" -> 0.5 seconds)
		seconds, err = strconv.ParseFloat(s, 64)
		if err != nil {
			return 0, fmt.Errorf("invalid shutter duration: %s", s)
		}
	}

	if seconds < 0 {
		return 0, fmt.Errorf("shutter duration cannot be negative")
	}

	// Validate shutter speed range (1/8000s to 1 hour)
	if seconds < 0.000125 { // 1/8000 second
		return 0, fmt.Errorf("shutter too fast (minimum: 1/8000 second = 0.000125 seconds)")
	}

	if seconds > 3600 { // 1 hour maximum
		return 0, fmt.Errorf("shutter too slow (maximum: 3600 seconds = 1 hour)")
	}

	return seconds, nil
}

// Focus mode constants (from SDK)
const (
	FocusModeManual = 0x0001
	FocusModeAFS    = 0x8001
	FocusModeAFC    = 0x8002
)

// focusModeToString converts integer focus mode to string
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

// focusModeFromString converts string focus mode to integer
func focusModeFromString(mode string) (int, error) {
	switch mode {
	case "manual":
		return FocusModeManual, nil
	case "auto":
		return FocusModeAFS, nil
	case "auto_continuous":
		return FocusModeAFC, nil
	default:
		return 0, fmt.Errorf("invalid focus mode: %s (valid: manual, auto, auto_continuous)", mode)
	}
}

// findClosestShutterSpeed finds the closest supported shutter speed
// This is necessary because cameras only accept discrete values from a predefined list
func findClosestShutterSpeed(requested int, supported []int) int {
	if len(supported) == 0 {
		return requested
	}

	// Start with the first supported speed
	closest := supported[0]
	minDiff := abs(requested - closest)

	// Find the closest match
	for _, speed := range supported[1:] {
		diff := abs(requested - speed)
		if diff < minDiff {
			closest = speed
			minDiff = diff
		}
	}

	return closest
}

// Image quality constants (from SDK)
const (
	ImageQualityRAW       = 0x0001
	ImageQualityFine      = 0x0002
	ImageQualityNormal    = 0x0003
	ImageQualityRAWFine   = 0x0004
	ImageQualityRAWNormal = 0x0005
)

// imageQualityToString converts integer image quality to string
func imageQualityToString(quality int) string {
	switch quality {
	case ImageQualityRAW:
		return "RAW"
	case ImageQualityFine:
		return "FINE"
	case ImageQualityNormal:
		return "NORMAL"
	case ImageQualityRAWFine:
		return "RAW+FINE"
	case ImageQualityRAWNormal:
		return "RAW+JPEG"
	default:
		return "unknown"
	}
}

// imageQualityFromString converts string image quality to integer
func imageQualityFromString(quality string) (int, error) {
	switch quality {
	case "RAW":
		return ImageQualityRAW, nil
	case "FINE":
		return ImageQualityFine, nil
	case "NORMAL":
		return ImageQualityNormal, nil
	case "RAW+FINE":
		return ImageQualityRAWFine, nil
	case "RAW+JPEG", "RAW+NORMAL":
		return ImageQualityRAWNormal, nil
	default:
		return 0, fmt.Errorf("invalid image quality: %s (valid: RAW, FINE, NORMAL, RAW+FINE, RAW+JPEG)", quality)
	}
}
