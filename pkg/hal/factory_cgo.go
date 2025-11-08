//go:build cgo
// +build cgo

package hal

// NewDefaultCamera creates a camera instance for production use (real camera with cgo)
func NewDefaultCamera() Camera {
	return NewRealCamera()
}
