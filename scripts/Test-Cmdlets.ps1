  # ============================================
  # FUJIMATIC REST API TEST COMMANDS (PowerShell)
  # ============================================

  # Set the server URL (change if needed)
  $server = "http://localhost:8080"

  # ============================================
  # CAMERA CONTROL
  # ============================================

  # Connect to camera
  Invoke-RestMethod -Uri "$server/api/camera/connect" -Method Post

  # Disconnect from camera
  Invoke-RestMethod -Uri "$server/api/camera/disconnect" -Method Post

  # Get camera status
  Invoke-RestMethod -Uri "$server/api/camera/status" -Method Get

  # Get battery level
  Invoke-RestMethod -Uri "$server/api/camera/battery" -Method Get

  # ============================================
  # ISO SETTINGS
  # ============================================

  # Get current ISO
  Invoke-RestMethod -Uri "$server/api/settings/iso" -Method Get

  # Set ISO to 800
  $body = @{ iso = 800 } | ConvertTo-Json
  Invoke-RestMethod -Uri "$server/api/settings/iso" -Method Post -Body $body -ContentType "application/json"

  # Set ISO to 1600
  $body = @{ iso = 1600 } | ConvertTo-Json
  Invoke-RestMethod -Uri "$server/api/settings/iso" -Method Post -Body $body -ContentType "application/json"

  # Set ISO to 3200
  $body = @{ iso = 3200 } | ConvertTo-Json
  Invoke-RestMethod -Uri "$server/api/settings/iso" -Method Post -Body $body -ContentType "application/json"

  # ============================================
  # SHUTTER SPEED SETTINGS
  # ============================================

  # Get current shutter speed
  Invoke-RestMethod -Uri "$server/api/settings/shutter" -Method Get

  # Set shutter speed using microseconds (1/250s = 4000µs)
  $body = @{ shutter_us = 4000 } | ConvertTo-Json
  Invoke-RestMethod -Uri "$server/api/settings/shutter" -Method Post -Body $body -ContentType "application/json"

  # Set shutter speed using photographic notation (1/125s)
  $body = @{ shutter = "1/125" } | ConvertTo-Json
  Invoke-RestMethod -Uri "$server/api/settings/shutter" -Method Post -Body $body -ContentType "application/json"

  # Set shutter speed to 2 seconds
  $body = @{ shutter = "2s" } | ConvertTo-Json
  Invoke-RestMethod -Uri "$server/api/settings/shutter" -Method Post -Body $body -ContentType "application/json"

  # Set shutter speed to 30 seconds
  $body = @{ shutter = "30" } | ConvertTo-Json
  Invoke-RestMethod -Uri "$server/api/settings/shutter" -Method Post -Body $body -ContentType "application/json"

  # ============================================
  # FOCUS MODE SETTINGS
  # ============================================

  # Get current focus mode
  Invoke-RestMethod -Uri "$server/api/settings/focus" -Method Get

  # Set focus mode to manual
  $body = @{ focus_mode = "manual" } | ConvertTo-Json
  Invoke-RestMethod -Uri "$server/api/settings/focus" -Method Post -Body $body -ContentType "application/json"

  # Set focus mode to auto (AF-S)
  $body = @{ focus_mode = "auto" } | ConvertTo-Json
  Invoke-RestMethod -Uri "$server/api/settings/focus" -Method Post -Body $body -ContentType "application/json"

  # Set focus mode to continuous auto (AF-C)
  $body = @{ focus_mode = "auto_continuous" } | ConvertTo-Json
  Invoke-RestMethod -Uri "$server/api/settings/focus" -Method Post -Body $body -ContentType "application/json"

  # ============================================
  # SESSION MANAGEMENT
  # ============================================

  # Get current session info
  Invoke-RestMethod -Uri "$server/api/session" -Method Get

  # Start a new session
  $body = @{
      project = "astrophoto_m31"
      output_dir = "./captures/m31"
  } | ConvertTo-Json
  Invoke-RestMethod -Uri "$server/api/session/start" -Method Post -Body $body -ContentType "application/json"

  # Start another session with different project
  $body = @{
      project = "test_session"
      output_dir = "./test_captures"
  } | ConvertTo-Json
  Invoke-RestMethod -Uri "$server/api/session/start" -Method Post -Body $body -ContentType "application/json"

  # Stop the current session
  Invoke-RestMethod -Uri "$server/api/session/stop" -Method Post

  # ============================================
  # CAPTURE OPERATIONS
  # ============================================

  # Single capture (no session required)
  Invoke-RestMethod -Uri "$server/api/capture/single" -Method Post

  # Get capture status
  Invoke-RestMethod -Uri "$server/api/capture/status" -Method Get

  # ============================================
  # COMPLETE WORKFLOW EXAMPLES
  # ============================================

  # Example 1: Quick single capture
  Write-Host "`n=== Quick Single Capture Workflow ===" -ForegroundColor Cyan
  Invoke-RestMethod -Uri "$server/api/camera/connect" -Method Post
  Invoke-RestMethod -Uri "$server/api/capture/single" -Method Post
  Invoke-RestMethod -Uri "$server/api/camera/disconnect" -Method Post

  # Example 2: Session-based capture with settings
  Write-Host "`n=== Session-Based Capture Workflow ===" -ForegroundColor Cyan
  Invoke-RestMethod -Uri "$server/api/camera/connect" -Method Post
  $body = @{ iso = 1600 } | ConvertTo-Json
  Invoke-RestMethod -Uri "$server/api/settings/iso" -Method Post -Body $body -ContentType "application/json"
  $body = @{ shutter = "30s" } | ConvertTo-Json
  Invoke-RestMethod -Uri "$server/api/settings/shutter" -Method Post -Body $body -ContentType "application/json"
  $body = @{ focus_mode = "manual" } | ConvertTo-Json
  Invoke-RestMethod -Uri "$server/api/settings/focus" -Method Post -Body $body -ContentType "application/json"
  $body = @{ project = "deep_sky"; output_dir = "./captures" } | ConvertTo-Json
  Invoke-RestMethod -Uri "$server/api/session/start" -Method Post -Body $body -ContentType "application/json"
  Invoke-RestMethod -Uri "$server/api/capture/single" -Method Post
  Invoke-RestMethod -Uri "$server/api/capture/single" -Method Post
  Invoke-RestMethod -Uri "$server/api/capture/single" -Method Post
  Invoke-RestMethod -Uri "$server/api/session/stop" -Method Post
  Invoke-RestMethod -Uri "$server/api/camera/disconnect" -Method Post

  # Example 3: Status monitoring
  Write-Host "`n=== Status Check ===" -ForegroundColor Cyan
  $status = Invoke-RestMethod -Uri "$server/api/camera/status" -Method Get
  Write-Host "Connected: $($status.connected)"
  Write-Host "Battery: $($status.battery)%"
  Write-Host "Model: $($status.model)"

  $iso = Invoke-RestMethod -Uri "$server/api/settings/iso" -Method Get
  Write-Host "ISO: $($iso.iso)"

  $shutter = Invoke-RestMethod -Uri "$server/api/settings/shutter" -Method Get
  Write-Host "Shutter: $($shutter.shutter_display) ($($shutter.shutter_us)µs)"

  $focus = Invoke-RestMethod -Uri "$server/api/settings/focus" -Method Get
  Write-Host "Focus: $($focus.focus_mode)"

  # ============================================
  # ERROR HANDLING EXAMPLES
  # ============================================

  # Test error: Capture without connection
  Write-Host "`n=== Testing Error Cases ===" -ForegroundColor Yellow
  try {
      Invoke-RestMethod -Uri "$server/api/capture/single" -Method Post -ErrorAction Stop
  } catch {
      Write-Host "Expected error: $($_.Exception.Message)" -ForegroundColor Red
  }

  # Test error: Invalid ISO value
  try {
      $body = @{ iso = 999999 } | ConvertTo-Json
      Invoke-RestMethod -Uri "$server/api/settings/iso" -Method Post -Body $body -ContentType "application/json" -ErrorAction Stop
  } catch {
      Write-Host "Expected error: $($_.Exception.Message)" -ForegroundColor Red
  }

  # ============================================
  # HELPER FUNCTIONS
  # ============================================

  # Function to pretty-print JSON response
  function Show-ApiResponse {
      param([string]$Uri, [string]$Method = "Get")
      Write-Host "`n$Method $Uri" -ForegroundColor Green
      $response = Invoke-RestMethod -Uri $Uri -Method $Method
      $response | ConvertTo-Json -Depth 10
  }

  # Function to test all settings at once
  function Test-AllSettings {
      Write-Host "`n=== Testing All Settings ===" -ForegroundColor Cyan
      Show-ApiResponse "$server/api/camera/status" "Get"
      Show-ApiResponse "$server/api/camera/battery" "Get"
      Show-ApiResponse "$server/api/settings/iso" "Get"
      Show-ApiResponse "$server/api/settings/shutter" "Get"
      Show-ApiResponse "$server/api/settings/focus" "Get"
      Show-ApiResponse "$server/api/session" "Get"
      Show-ApiResponse "$server/api/capture/status" "Get"
  }

  # Run comprehensive test
  # Test-AllSettings
