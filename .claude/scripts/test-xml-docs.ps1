#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Tests XML documentation script on a small subset of files

.DESCRIPTION
    Runs the XML documentation script on a limited set of files to verify:
    - Script functionality
    - Build stability
    - Documentation quality
    
.PARAMETER TestFile
    Specific file to test (optional)
#>

param(
    [Parameter(Mandatory = $false)]
    [string]$TestFile
)

$Green = "`e[32m"
$Red = "`e[31m"
$Yellow = "`e[33m"
$Blue = "`e[34m"
$Reset = "`e[0m"

function Write-TestStatus {
    param([string]$Message, [string]$Status)
    
    $StatusColor = switch ($Status) {
        "PASS" { $Green }
        "FAIL" { $Red }
        "WARN" { $Yellow }
        "INFO" { $Blue }
        default { $Reset }
    }
    
    Write-Host "[$StatusColor$Status$Reset] $Message"
}

Write-Host "${Blue}=== XML Documentation Test Suite ===${Reset}"
Write-Host ""

# Test 1: Verify script exists and is executable
if (-not (Test-Path "./.claude/scripts/add-xml-documentation.ps1")) {
    Write-TestStatus "XML documentation script not found" "FAIL"
    exit 1
}
Write-TestStatus "XML documentation script found" "PASS"

# Test 2: Initial build verification
Write-TestStatus "Verifying initial build state..." "INFO"
$buildResult = dotnet build MeAndMyDog.sln --verbosity quiet 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-TestStatus "Initial build failed - cannot proceed with XML documentation" "FAIL"
    Write-Host $buildResult -ForegroundColor Red
    exit 1
}
Write-TestStatus "Initial build successful" "PASS"

# Test 3: Run dry run on a small subset
if ($TestFile) {
    $testTarget = $TestFile
} else {
    # Find a small test file
    $testTarget = Get-ChildItem -Path "src" -Filter "*.cs" -Recurse | 
                  Where-Object { 
                      $_.Name -notmatch '\.g\.cs$|\.designer\.cs$|AssemblyInfo\.cs$' -and
                      $_.DirectoryName -notmatch '\\obj\\|\\bin\\' -and
                      $_.Length -lt 10KB
                  } | 
                  Select-Object -First 1 -ExpandProperty FullName
}

if (-not $testTarget) {
    Write-TestStatus "No suitable test file found" "FAIL"
    exit 1
}

Write-TestStatus "Running dry run on: $testTarget" "INFO"

# Run the script in dry run mode
$scriptResult = & "./.claude/scripts/add-xml-documentation.ps1" -SourcePath $testTarget -ProjectPath "src/API/MeAndMyDog.API/MeAndMyDog.API.csproj" -DryRun

if ($LASTEXITCODE -eq 0) {
    Write-TestStatus "Dry run completed successfully" "PASS"
} else {
    Write-TestStatus "Dry run failed" "FAIL"
    exit 1
}

# Test 4: Test actual modification on a copy
Write-TestStatus "Testing actual modification..." "INFO"

# Create a backup
$backupFile = "$testTarget.backup"
Copy-Item $testTarget $backupFile

try {
    # Run the script for real on the single file
    $scriptResult = & "./.claude/scripts/add-xml-documentation.ps1" -SourcePath $testTarget -ProjectPath "src/API/MeAndMyDog.API/MeAndMyDog.API.csproj" -BatchSize 1
    
    if ($LASTEXITCODE -eq 0) {
        Write-TestStatus "XML documentation added successfully" "PASS"
        
        # Verify build still works
        $buildResult = dotnet build MeAndMyDog.sln --verbosity quiet 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-TestStatus "Build verification after XML documentation: SUCCESS" "PASS"
        } else {
            Write-TestStatus "Build verification after XML documentation: FAILED" "FAIL"
            Write-Host $buildResult -ForegroundColor Red
        }
    } else {
        Write-TestStatus "XML documentation script failed" "FAIL"
    }
}
finally {
    # Restore the original file
    Move-Item $backupFile $testTarget -Force
    Write-TestStatus "Original file restored" "INFO"
}

Write-Host ""
Write-TestStatus "Test suite completed" "INFO"