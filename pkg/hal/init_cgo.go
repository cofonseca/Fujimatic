//go:build cgo
// +build cgo

package hal

import (
	"github.com/cfonseca/fujimatic/pkg/sdk"
)

// InitSDK initializes the Fujifilm SDK (real implementation with cgo)
func InitSDK(sdkPath string) (InitResult, error) {
	result, err := sdk.Init(sdkPath)
	if err != nil {
		return InitError, err
	}

	// Convert sdk.InitResult to hal.InitResult
	if result == sdk.InitSuccess {
		return InitSuccess, nil
	}

	return InitError, nil
}

// SetVerbose enables or disables verbose logging in the SDK layer
func SetVerbose(enabled bool) error {
	return sdk.SetVerbose(enabled)
}
