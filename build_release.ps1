<#
.SYNOPSIS
    Aegis-Link C2 Release Automation Script
    Builds, Tests, and Packages v3.0.0
.DESCRIPTION
    1. Restoration
    2. Unit Testing
    3. Publishing (Self-Contained)
    4. Zipping Artifacts
    5. Git Tagging (Optional)
.EXAMPLE
    .\build_release.ps1 -Version "3.0.0" -Tag
#>

param (
    [string]$Version = "3.0.0",
    [switch]$Tag = $false,
    [switch]$SkipTests = $false
)

$ErrorActionPreference = "Stop"

Write-Host ">>> AEGIS-LINK v$Version RELEASE PIPELINE <<<" -ForegroundColor Cyan

# 1. Clean & Restore
Write-Host "`n[1/5] Cleaning and Restoring..." -ForegroundColor Yellow
dotnet clean --verbosity quiet
dotnet restore --verbosity quiet

# 2. Test
if (-not $SkipTests) {
    Write-Host "`n[2/5] Running Tests..." -ForegroundColor Yellow
    $testResult = dotnet test src\Tests\AegisLink.Tests.csproj --verbosity normal
    if ($LASTEXITCODE -ne 0) {
        Write-Error "TESTS FAILED. Use -SkipTests to bypass if this is an environment issue."
    }
    Write-Host "Tests Passed." -ForegroundColor Green
} else {
    Write-Host "`n[2/5] SKIPPING TESTS..." -ForegroundColor DarkGray
}

# 3. Publish
Write-Host "`n[3/5] Publishing Self-Contained Binary..." -ForegroundColor Yellow
$publishDir = "dist\v$Version"
if (Test-Path $publishDir) { Remove-Item "dist" -Recurse -Force }

dotnet publish src\App\AegisLink.App.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $publishDir `
    --verbosity quiet

if (-not (Test-Path "$publishDir\AegisLink.App.exe")) {
    Write-Error "PUBLISH FAILED. Exe not found."
}
Write-Host "Build Output: $publishDir\AegisLink.App.exe" -ForegroundColor Green

# 4. Package
Write-Host "`n[4/5] Packaging Artifacts..." -ForegroundColor Yellow
$zipPath = "dist\AegisLink-v$Version-win-x64.zip"
Compress-Archive -Path "$publishDir\*" -DestinationPath $zipPath -Force
Write-Host "Artifact Created: $zipPath" -ForegroundColor Green

# 5. Git Tag (Optional)
if ($Tag) {
    Write-Host "`n[5/5] Tagging Release..." -ForegroundColor Yellow
    git tag -a "v$Version" -m "Release v$Version"
    Write-Host "Git Tag v$Version created." -ForegroundColor Green
}

Write-Host "`n>>> RELEASE COMPLETE <<<" -ForegroundColor Cyan
