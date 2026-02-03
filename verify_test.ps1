$headers = @{ Authorization = "Basic YWRtaW46YWRtaW4xMjM=" }
try {
    $p = Invoke-RestMethod -Uri "http://localhost:5212/api/projects/16" -Headers $headers
    $nominal = [math]::Round($p.nominalBudget)
    if ($nominal -eq 125000) {
        Write-Host "TEST PASS: NominalBudget is 125000"
    }
    else {
        Write-Host "TEST FAIL: NominalBudget is $nominal (Expected 125000)"
    }
    
    # Also verify Snapshot Matrix visibility (Epic 8)
    # We need to simulate checking if snapshots exist. 
    # Usually they are created on demand. Let's try to fetch them.
    # We need a ForecastVersionId. 
    # Let's list versions for this project.
    $versions = Invoke-RestMethod -Uri "http://localhost:5212/api/forecasts/16/versions" -Headers $headers
    
    if ($versions.Count -gt 0) {
        $vId = $versions[0].id
        Write-Host "Forecast Version found: $vId"
        
        $snapshots = Invoke-RestMethod -Uri "http://localhost:5212/api/financials/16/snapshots/$vId" -Headers $headers
        
        if ($snapshots.Count -gt 0) {
            Write-Host "TEST PASS: Snapshots found ($($snapshots.Count) months)"
            $jan = $snapshots | Where-Object { $_.month -like "*2026-01-01*" }
            if ($jan.status -eq 1) {
                # 1 = Editable
                Write-Host "TEST PASS: January 2026 is Editable"
            }
            else {
                Write-Host "TEST FAIL: January 2026 status is $($jan.status) (Expected 1)"
            }
        }
        else {
            Write-Host "TEST WARNING: No snapshots found (might be auto-generated later)"
        }
    }
    else {
        Write-Host "TEST WARNING: No forecast versions found (Project created without version)"
    }

}
catch {
    Write-Host "ERROR: $_"
}
