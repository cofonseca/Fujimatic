//go:build !cgo
// +build !cgo

package hal

// InitSDK initializes the SDK (fake implementation without cgo)
// Always returns success for fake camera mode
func InitSDK(sdkPath string) (InitResult, error) {
	// No actual SDK initialization needed for fake camera
	return InitSuccess, nil
}
