package server

import (
	"sync"

	"github.com/cfonseca/fujimatic/pkg/hal"
	"github.com/cfonseca/fujimatic/pkg/session"
)

// State represents the server-side state for camera control
type State struct {
	mu      sync.RWMutex
	camera  hal.Camera
	session *session.Session
}

// NewState creates a new server state with the given camera
func NewState(camera hal.Camera) *State {
	return &State{
		camera: camera,
	}
}

// GetCamera returns the camera instance (read-only access)
func (s *State) GetCamera() hal.Camera {
	s.mu.RLock()
	defer s.mu.RUnlock()
	return s.camera
}

// GetSession returns the current session (read-only access)
func (s *State) GetSession() *session.Session {
	s.mu.RLock()
	defer s.mu.RUnlock()
	return s.session
}

// SetSession sets the current session
func (s *State) SetSession(sess *session.Session) {
	s.mu.Lock()
	defer s.mu.Unlock()
	s.session = sess
}

// ClearSession clears the current session
func (s *State) ClearSession() {
	s.mu.Lock()
	defer s.mu.Unlock()
	s.session = nil
}

// Lock acquires a write lock (for operations that modify state)
func (s *State) Lock() {
	s.mu.Lock()
}

// Unlock releases a write lock
func (s *State) Unlock() {
	s.mu.Unlock()
}

// RLock acquires a read lock (for operations that only read state)
func (s *State) RLock() {
	s.mu.RLock()
}

// RUnlock releases a read lock
func (s *State) RUnlock() {
	s.mu.RUnlock()
}
