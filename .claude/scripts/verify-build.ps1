# Build Verification Script for Me and My Dog Platform
param(
    [string]$Project = "all",
    [switch]$FixErrors = $false
)

$ErrorActionPreference = "Continue"
$buildSuccess = $true
$errors = @()

Write-Host "Starting build verification..." -ForegroundColor Cyan

# Clean previous build artifacts
if (Test-Path "BuildResults.txt") {
    Remove-Item "BuildResults.txt"
}

# Function to build a project
function Build-Project {
    param([string]$ProjectPath, [string]$ProjectName)
    
    Write-Host "`nBuilding $ProjectName..." -ForegroundColor Yellow
    $output = dotnet build $ProjectPath --no-restore 2>&1
    $exitCode = $LASTEXITCODE
    
    if ($exitCode -ne 0) {
        Write-Host "Build FAILED for $ProjectName" -ForegroundColor Red
        $script:buildSuccess = $false
        $script:errors += @{
            Project = $ProjectName
            Path = $ProjectPath
            Output = $output | Out-String
        }
        $output | Out-File -Append "BuildResults.txt"
    } else {
        Write-Host "Build SUCCEEDED for $ProjectName" -ForegroundColor Green
    }
    
    return $exitCode -eq 0
}

# Restore packages first
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore MeAndMyDog.sln --verbosity quiet

# Build projects based on parameter
switch ($Project) {
    "all" {
        Build-Project "src/BuildingBlocks/MeAndMyDog.SharedKernel/MeAndMyDog.SharedKernel.csproj" "SharedKernel"
        Build-Project "src/BuildingBlocks/MeAndMyDog.BlobStorage/MeAndMyDog.BlobStorage.csproj" "BlobStorage"
        Build-Project "src/API/MeAndMyDog.API/MeAndMyDog.API.csproj" "API"
        Build-Project "src/Web/MeAndMyDog.WebApp/MeAndMyDog.WebApp.csproj" "WebApp"
    }
    "api" {
        Build-Project "src/API/MeAndMyDog.API/MeAndMyDog.API.csproj" "API"
    }
    "web" {
        Build-Project "src/Web/MeAndMyDog.WebApp/MeAndMyDog.WebApp.csproj" "WebApp"
    }
    "shared" {
        Build-Project "src/BuildingBlocks/MeAndMyDog.SharedKernel/MeAndMyDog.SharedKernel.csproj" "SharedKernel"
        Build-Project "src/BuildingBlocks/MeAndMyDog.BlobStorage/MeAndMyDog.BlobStorage.csproj" "BlobStorage"
    }
}

# Summary
Write-Host "`n========== Build Summary ==========" -ForegroundColor Cyan
if ($buildSuccess) {
    Write-Host "All builds SUCCEEDED!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "Build FAILED with errors:" -ForegroundColor Red
    foreach ($error in $errors) {
        Write-Host "`nProject: $($error.Project)" -ForegroundColor Yellow
        Write-Host "Path: $($error.Path)" -ForegroundColor Gray
        
        # Extract just the error messages
        $errorLines = $error.Output -split "`n" | Where-Object { $_ -match "error CS\d+" -or $_ -match "error MSB\d+" }
        $errorLines | ForEach-Object { Write-Host $_ -ForegroundColor Red }
    }
    
    if ($FixErrors) {
        Write-Host "`nAttempting to fix common errors..." -ForegroundColor Yellow
        # Add automated fix attempts here
        # For now, just output suggestions
        Write-Host "Common fixes:" -ForegroundColor Cyan
        Write-Host "- Missing types: Check if all interfaces are implemented"
        Write-Host "- Namespace issues: Verify using statements"
        Write-Host "- Missing packages: Run 'dotnet restore'"
    }
    
    Write-Host "`nDetailed errors saved to: BuildResults.txt" -ForegroundColor Gray
    exit 1
}