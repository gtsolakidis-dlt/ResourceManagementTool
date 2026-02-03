# ApiTests.ps1
$baseUrl = "http://localhost:5212/api"

function Test-Endpoint {
    param($name, $url, $method="GET", $body=$null, $user="admin", $expectError=$false)
    
    $creds = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("${user}:admin123"))
    $headers = @{ Authorization = "Basic $creds"; "Content-Type" = "application/json" }
    
    Write-Host "Testing $name..." -NoNewline
    
    try {
        $p = @{ Uri = "$baseUrl/$url"; Method = $method; Headers = $headers; ErrorAction = "Stop" }
        if ($body) { $p.Body = ($body | ConvertTo-Json) }
        
        $res = Invoke-RestMethod @p
        
        if ($expectError) {
            Write-Host " FAILED (Expected error, got success)" -ForegroundColor Red
            return $false
        }
        Write-Host " PASSED" -ForegroundColor Green
        return $true
    }
    catch {
        if ($expectError) {
            Write-Host " PASSED (Got expected error)" -ForegroundColor Green
            return $true
        }
        Write-Host " FAILED ($($_.Exception.Message))" -ForegroundColor Red
        return $false
    }
}

Write-Host "=== STARTING API TESTS ===" -ForegroundColor Cyan

# 1. Auth
Test-Endpoint "Login (Admin)" "auth/me" -user "admin"
Test-Endpoint "Login (Invalid)" "auth/me" -user "admin" -headers @{Authorization="Basic YWRtaW46d3Jvbmc="} -expectError $true

# 2. Roster
Test-Endpoint "Get Roster (Admin)" "roster" -user "admin"

# 3. Projects Visibility (RBAC)
# Manager should see all
Test-Endpoint "Get Projects (Manager)" "projects" -user "manager"
# Employee should see only assigned (TEST.005)
$empProjects = Invoke-RestMethod -Uri "$baseUrl/projects" -Headers @{Authorization="Basic "+[Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("employee:admin123"))}
if ($empProjects.Count -eq 1 -and $empProjects[0].wbs -eq "TEST.005") {
    Write-Host "Test Employee Project Visibility... PASSED" -ForegroundColor Green
} else {
    Write-Host "Test Employee Project Visibility... FAILED (Count: $($empProjects.Count))" -ForegroundColor Red
}

write-host "=== COMPLETED API TESTS ===" -ForegroundColor Cyan
