function Test-FujimaticAPI {

    Write-Host "`n=== FUJIMATIC API COMPREHENSIVE TEST ===" -ForegroundColor Magenta
    Write-Host "Testing API endpoints with new response format (status + status_code)" -ForegroundColor Yellow

    # 1. Connect
    Write-Host "`n[1/11] Connecting to camera..." -ForegroundColor Cyan
    $result = Invoke-RestMethod -Uri "$server/api/camera/connect" -Method Post
    Write-Host "Status: $($result.status), Battery: $($result.battery)%" -ForegroundColor Green

    # 2. Check status
    Write-Host "`n[2/11] Checking camera status..." -ForegroundColor Cyan
    $status = Invoke-RestMethod -Uri "$server/api/camera/status" -Method Get
    Write-Host "Connected: $($status.connected), Model: $($status.model)" -ForegroundColor Green

    # 3. Set ISO
    Write-Host "`n[3/11] Setting ISO to 800..." -ForegroundColor Cyan
    $body = @{ iso = 800 } | ConvertTo-Json
    $result = Invoke-RestMethod -Uri "$server/api/settings/iso" -Method Post -Body $body -ContentType "application/json"
    Write-Host "ISO set to: $($result.iso) (Code: $($result.status_code))" -ForegroundColor Green

    # 4. Set shutter
    Write-Host "`n[4/11] Setting shutter to 1/250s..." -ForegroundColor Cyan
    $body = @{ shutter = "1/250" } | ConvertTo-Json
    $result = Invoke-RestMethod -Uri "$server/api/settings/shutter" -Method Post -Body $body -ContentType "application/json"
    Write-Host "Shutter set to: $($result.shutter_speed) (Code: $($result.status_code))" -ForegroundColor Green

    # 5. Set focus
    Write-Host "`n[5/11] Setting focus to manual..." -ForegroundColor Cyan
    $body = @{ focus_mode = "manual" } | ConvertTo-Json
    $result = Invoke-RestMethod -Uri "$server/api/settings/focus" -Method Post -Body $body -ContentType "application/json"
    Write-Host "Focus mode: $($result.focus_mode) (Code: $($result.status_code))" -ForegroundColor Green

    # 6. Verify GET responses have consistent format
    Write-Host "`n[6/11] Verifying GET response format..." -ForegroundColor Cyan
    $iso = Invoke-RestMethod -Uri "$server/api/settings/iso" -Method Get
    Write-Host "GET ISO: $($iso.iso) (Status: $($iso.status), Code: $($iso.status_code))" -ForegroundColor Green
    
    $shutter = Invoke-RestMethod -Uri "$server/api/settings/shutter" -Method Get  
    Write-Host "GET Shutter: $($shutter.shutter_speed) (Status: $($shutter.status), Code: $($shutter.status_code))" -ForegroundColor Green
    
    $focus = Invoke-RestMethod -Uri "$server/api/settings/focus" -Method Get
    Write-Host "GET Focus: $($focus.focus_mode) (Status: $($focus.status), Code: $($focus.status_code))" -ForegroundColor Green

    # 7. Single capture (no session)
    Write-Host "`n[7/11] Single capture without session..." -ForegroundColor Cyan
    $result = Invoke-RestMethod -Uri "$server/api/capture/single" -Method Post
    Write-Host "Captured: $($result.filename), Size: $($result.size) bytes" -ForegroundColor Green

    # 8. Start session
    Write-Host "`n[8/11] Starting session..." -ForegroundColor Cyan
    $body = @{ project = "api_test"; output_dir = "./test_api" } | ConvertTo-Json
    $result = Invoke-RestMethod -Uri "$server/api/session/start" -Method Post -Body $body -ContentType "application/json"
    Write-Host "Session: $($result.project), Sequence: $($result.sequence)" -ForegroundColor Green

    # 9. Session-based capture
    Write-Host "`n[9/11] Capture with session..." -ForegroundColor Cyan
    $result = Invoke-RestMethod -Uri "$server/api/capture/single" -Method Post
    Write-Host "Captured: $($result.filename), Size: $($result.size) bytes" -ForegroundColor Green

    # 10. Stop session
    Write-Host "`n[10/11] Stopping session..." -ForegroundColor Cyan
    $result = Invoke-RestMethod -Uri "$server/api/session/stop" -Method Post
    Write-Host "Status: $($result.status)" -ForegroundColor Green

    # 11. Disconnect
    Write-Host "`n[11/11] Disconnecting..." -ForegroundColor Cyan
    $result = Invoke-RestMethod -Uri "$server/api/camera/disconnect" -Method Post
    Write-Host "Status: $($result.status)" -ForegroundColor Green

    Write-Host "`n=== TEST COMPLETE ===" -ForegroundColor Magenta
}