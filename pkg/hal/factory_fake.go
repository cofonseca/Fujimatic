//go:build !cgo
// +build !cgo

package hal

// NewDefaultCamera creates a camera instance when cgo is not available (fake camera)
func NewDefaultCamera() Camera {
	return NewFakeCamera()
}
