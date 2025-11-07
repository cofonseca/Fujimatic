---
project: fujimatic
version: 0.1.0
description: "fujimatic — interactive CLI/TUI to tether and control Fujifilm X-T3 cameras for film scanning and astrophotography"
author: "Corey Fonseca"
license: "MIT"
sdk_path_default: "C:\\Users\\cfonseca\\Documents\\fujimatic\\sdk"
platforms: ["windows-x86_64", "linux-x86_64", "linux-arm64"]
entrypoint: "cmd/fujimatic"
notes_for_ai: |
  - The repo uses a small C wrapper around the Fujifilm Camera Control SDK and Go cgo.
  - Minimal APIs required: init, connect, disconnect, getBattery, setShutter, capture, downloadLast.
  - Session persistence is JSON; config is YAML.
  - Prioritize synchronous capture+download for MVP to support plate-solving.
---

# Fujimatic

`fujimatic` is an interactive CLI/TUI for tethered control of Fujifilm mirrorless cameras (initial focus: X-T3) to support film scanning and astrophotography workflows. Written in Go with a small C wrapper for the Fujifilm Camera Control SDK.

## MVP Scope
- Connect/disconnect to camera via USB using Fuji Camera Control SDK.
- Set shutter speed and capture RAW (.RAF) images.
- Immediately download images to a project directory.
- Intervalometer (start/pause/resume/stop) with integration-time calculations.
- Battery safety (pause at 10%).
- Interactive shell plus `--non-interactive` single-command mode.
- Config file for default outdir, battery threshold, and sdk path.

## Design Goals
- Modular: `sdk` bindings, hardware abstraction layer (HAL), session manager, CLI/TUI client.
- Testable: fake HAL implementation for unit tests; integration tests run manually against hardware.
- Cross-platform: Windows and Linux (incl. Raspberry Pi ARM) support.

---

## Architecture Overview

### Components
- **sdk-c-wrapper (C)** – Minimal API over the Fujifilm SDK.
- **pkg/sdk (Go)** – cgo bindings that expose `Init`, `Connect`, etc.
- **pkg/hal** – Hardware abstraction layer; `Camera` interface + real/fake implementations.
- **pkg/session** – Session manager (handles intervalometer, file naming, persistence).
- **cmd/fujimatic** – CLI shell frontend (REPL).
- **libs/** – SDK redistributables (not committed).
- **scripts/** – build/test utilities.

### Data Flow
User → CLI → Session Manager → HAL → SDK Wrapper → Fujifilm SDK → Camera.

### Config File Example
```yaml
default_outdir: "C:\\Users\\cfonseca\\Pictures\\fujimatic"
last_project_name: "orion_m42"
battery_threshold_percent: 10
sdk_path: "C:\\Users\\cfonseca\\Documents\\fujimatic\\sdk\\REDISTRIBUTABLES"
viewer_cmd: "mpv"
```

---

## File Layout
```
fujimatic/
├─ cmd/fujimatic/
├─ pkg/sdk/
├─ pkg/hal/
├─ pkg/session/
├─ sdk-c-wrapper/
├─ libs/
├─ scripts/
└─ README.md
```

---

## Build Instructions

### Windows
```powershell
setx FUJI_SDK_PATH "C:\\Users\\cfonseca\\Documents\\fujimatic\\sdk\\REDISTRIBUTABLES"
go build -o bin\\fujimatic.exe ./cmd/fujimatic
```

### Linux / Raspberry Pi
```bash
export FUJI_SDK_PATH=/home/pi/fujimatic/sdk/redist
GOARCH=arm64 GOOS=linux go build -o bin/fujimatic ./cmd/fujimatic
```

---

# User Stories (Scrum Style)

### Epic A — Foundation & SDK Binding

**A-1. Initialize Repository**
- Create Go module and folder structure.
- Verify `go list ./...` builds cleanly.

**A-2. Add C Wrapper Project Files**
- Create `fm_wrapper.h` and `fm_wrapper.c`.
- Build scripts produce `.dll` and `.so`.
- Verify successful compilation.

**A-3. Add Go cgo Bindings**
- Implement Go-friendly wrappers for C functions.
- Test using dummy return codes.

---

### Epic B — HAL, Shell & Basic Workflows

**B-1. Implement HAL Interface**
- Define `Camera` interface and `fake` implementation.
- Unit tests run with fake HAL.

**B-2. Session Manager**
- Implement file naming and sequence tracking.
- Validate synchronous capture + download.

**B-3. CLI Shell**
- Add `connect`, `capture`, and `exit` commands.
- Verify REPL responds correctly.

---

### Epic C — Intervalometer & Battery Handling

**C-1. Intervalometer**
- Implement synchronous intervalometer (`start`, `stop`).
- Support delay between frames and battery checks.

**C-2. Pause/Resume**
- Pause saves session state; resume loads and continues.

**C-3. Battery Threshold**
- Auto-pause at ≤10% battery, notify user.

---

### Epic D — Integration & Hardware

**D-1. Connect/Disconnect with Real SDK**
- Camera connects and disconnects cleanly.

**D-2. Capture RAW & Download**
- `.RAF` file successfully saved to configured folder.

**D-3. Integration Test Script**
- PowerShell script automates basic camera tests.

---

### Epic E — Polishing & Optional Features

**E-1. Non-interactive Mode**
- Support single commands via CLI flags.

**E-2. Async Intervalometer**
- Enable concurrent capture + download.

**E-3. Live View (External Viewer)**
- Launch `mpv` or similar for live feed display.

---

# License
MIT License
