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
var standardShutterSpeeds = []struct {
	microseconds int
	display      string
}{
	{125, "1/8000"},       // 1/8000 second (125 µs)
	{250, "1/4000"},       // 1/4000 second (250 µs)
	{500, "1/2000"},       // 1/2000 second (500 µs)
	{1000, "1/1000"},      // 1/1000 second (1000 µs)
	{2000, "1/500"},       // 1/500 second (2000 µs)
	{4000, "1/250"},       // 1/250 second (4000 µs)
	{8000, "1/125"},       // 1/125 second (8000 µs)
	{16667, "1/60"},       // 1/60 second (16667 µs)
	{33333, "1/30"},       // 1/30 second (33333 µs)
	{66667, "1/15"},       // 1/15 second (66667 µs)
	{125000, "1/8"},       // 1/8 second (125000 µs)
	{250000, "1/4"},       // 1/4 second (250000 µs)
	{500000, "1/2"},       // 1/2 second (500000 µs)
	{1000000, "1"},        // 1 second (1000000 µs)
	{2000000, "2"},        // 2 seconds (2000000 µs)
	{4000000, "4"},        // 4 seconds (4000000 µs)
	{8000000, "8"},        // 8 seconds (8000000 µs)
	{15000000, "15"},      // 15 seconds (15000000 µs)
	{30000000, "30"},      // 30 seconds (30000000 µs)
	{60000000, "60"},      // 60 seconds (60000000 µs)
}

// getPhotographicFraction converts microseconds to a photographic fraction string
// Maps to standard Fuji camera shutter speeds instead of mathematical rounding
func getPhotographicFraction(microseconds int) string {
	// Find the closest standard shutter speed
	closest := standardShutterSpeeds[0]
	minDiff := abs(microseconds - closest.microseconds)

	for _, speed := range standardShutterSpeeds[1:] {
		diff := abs(microseconds - speed.microseconds)
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

// parseShutterSpeed parses a shutter speed string to microseconds
// This delegates to parseShutterDuration for consistency with main package logic
func parseShutterSpeed(input string) (int, error) {
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
