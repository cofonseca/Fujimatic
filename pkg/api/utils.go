package api

import (
	"fmt"
	"strconv"
	"strings"
)

// formatShutterSpeed converts microseconds to a display string
func formatShutterSpeed(microseconds int) string {
	seconds := float64(microseconds) / 1000000.0

	// If less than 1 second, show as fraction
	if seconds < 1 {
		denominator := int(1.0 / seconds)
		return fmt.Sprintf("1/%d", denominator)
	}

	// Otherwise show as decimal with "s"
	return fmt.Sprintf("%.2fs", seconds)
}

// parseShutterSpeed parses a shutter speed string to microseconds
// Supports formats like: "1/250", "1/125", "2s", "0.5"
func parseShutterSpeed(input string) (int, error) {
	input = strings.TrimSpace(input)

	// Check if it's a fraction (e.g., "1/250")
	if strings.Contains(input, "/") {
		parts := strings.Split(input, "/")
		if len(parts) != 2 {
			return 0, fmt.Errorf("invalid fraction format: %s", input)
		}

		numerator, err := strconv.ParseFloat(parts[0], 64)
		if err != nil {
			return 0, fmt.Errorf("invalid numerator: %s", parts[0])
		}

		denominator, err := strconv.ParseFloat(parts[1], 64)
		if err != nil {
			return 0, fmt.Errorf("invalid denominator: %s", parts[1])
		}

		seconds := numerator / denominator
		return int(seconds * 1000000), nil
	}

	// Check if it has 's' suffix (e.g., "2s", "0.5s")
	if strings.HasSuffix(input, "s") {
		input = strings.TrimSuffix(input, "s")
		seconds, err := strconv.ParseFloat(input, 64)
		if err != nil {
			return 0, fmt.Errorf("invalid seconds value: %s", input)
		}
		return int(seconds * 1000000), nil
	}

	// Try parsing as decimal seconds (e.g., "0.5", "2.0")
	seconds, err := strconv.ParseFloat(input, 64)
	if err != nil {
		return 0, fmt.Errorf("invalid shutter speed format: %s", input)
	}

	return int(seconds * 1000000), nil
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

// abs returns the absolute value of an integer
func abs(x int) int {
	if x < 0 {
		return -x
	}
	return x
}
