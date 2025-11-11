package client

import (
	"fmt"
	"strings"
)

// parseServerAddress converts various server address formats to a full URL
// Supports:
//   - hostname → http://hostname:8080
//   - hostname:port → http://hostname:port
//   - 192.168.1.100 → http://192.168.1.100:8080
//   - 192.168.1.100:9000 → http://192.168.1.100:9000
//   - http://hostname:port → http://hostname:port (already formatted)
func parseServerAddress(addr string) (string, error) {
	if addr == "" {
		return "", fmt.Errorf("server address cannot be empty")
	}

	// If already has http://, use as-is
	if strings.HasPrefix(addr, "http://") || strings.HasPrefix(addr, "https://") {
		return strings.TrimSuffix(addr, "/"), nil
	}

	// Add default port if not specified
	if !strings.Contains(addr, ":") {
		addr = addr + ":8080"
	}

	// Add http:// prefix
	return "http://" + addr, nil
}
