<#
.SYNOPSIS
  Builds the frontend, copies it into the API's wwwroot, and starts FlipFlow on one port.
.EXAMPLE
  .\run.ps1                   # build everything and start on http://localhost:5000
.EXAMPLE
  .\run.ps1 -SkipFrontend     # reuse the frontend already in wwwroot
.NOTES
  Optional environment variables, set before running:
    $env:ASPNETCORE_URLS = "http://0.0.0.0:5000"   # allow other devices on your network
    $env:Access__Passcode = "your passcode"        # require a passcode (recommended with 0.0.0.0)
    $env:DemoData__Enabled = "true"                # add sample items to an empty database
#>
[CmdletBinding()]
param([switch]$SkipFrontend)

$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$wwwroot = Join-Path $root 'src/backend/FlipFlow.Api/wwwroot'

if (-not $SkipFrontend) {
    Write-Host '==> Building frontend'
    Push-Location (Join-Path $root 'src/frontend')
    try {
        if (-not (Test-Path 'node_modules')) { npm ci; if ($LASTEXITCODE) { throw 'npm ci failed' } }
        npm run build
        if ($LASTEXITCODE) { throw 'npm run build failed' }

        Write-Host '==> Copying frontend into wwwroot'
        Get-ChildItem $wwwroot -Force | Where-Object Name -ne '.gitkeep' | Remove-Item -Recurse -Force
        Copy-Item -Path 'dist/*' -Destination $wwwroot -Recurse -Force
    }
    finally { Pop-Location }
}

Write-Host '==> Starting FlipFlow (Ctrl+C to stop)'
Push-Location (Join-Path $root 'src/backend/FlipFlow.Api')
try {
    if (-not $env:ASPNETCORE_ENVIRONMENT) { $env:ASPNETCORE_ENVIRONMENT = 'Production' }
    dotnet run -c Release --no-launch-profile
}
finally { Pop-Location }
