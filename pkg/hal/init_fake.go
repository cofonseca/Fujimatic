//go:build !cgo
// +build !cgo

package hal

// InitSDK initializes the SDK (fake implementation without cgo)
// Always returns success for fake camera mode
func InitSDK(sdkPath string) (InitResult, error) {
	// No actual SDK initialization needed for fake camera
	return InitSuccess, nil
}

// SetVerbose is a no-op for fake camera mode
func SetVerbose(enabled bool) error {
	// Fake camera has no SDK layer to configure
	return nil
}
