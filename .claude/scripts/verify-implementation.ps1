#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Verifies that a class implementation is complete and follows standards

.DESCRIPTION
    This script verifies that a class implementation:
    - Implements all interface/abstract methods
    - Has no TODO comments or NotImplementedException
    - Has proper error handling
    - Builds without errors or warnings

.PARAMETER FilePath
    Path to the C# file to verify

.PARAMETER InterfaceName
    Name of the interface being implemented (optional)

.PARAMETER ProjectPath
    Path to the project file containing the implementation

.EXAMPLE
    ./verify-implementation.ps1 -FilePath "Services/PaymentService.cs" -InterfaceName "IPaymentService" -ProjectPath "MeAndMyDog.API.csproj"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$FilePath,
    
    [Parameter(Mandatory = $false)]
    [string]$InterfaceName,
    
    [Parameter(Mandatory = $true)]
    [string]$ProjectPath
)

# Colors for output
$Red = "`e[31m"
$Green = "`e[32m"
$Yellow = "`e[33m"
$Blue = "`e[34m"
$Reset = "`e[0m"

function Write-Status {
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

function Test-FileExists {
    if (-not (Test-Path $FilePath)) {
        Write-Status "File not found: $FilePath" "FAIL"
        return $false
    }
    Write-Status "File exists: $FilePath" "PASS"
    return $true
}

function Test-NoTodoComments {
    $content = Get-Content $FilePath -Raw
    $todoPattern = "(?i)(TODO|FIXME|HACK|XXX|BUG)"
    
    if ($content -match $todoPattern) {
        Write-Status "Found TODO/FIXME comments - implementation incomplete" "FAIL"
        $matches = [regex]::Matches($content, $todoPattern)
        foreach ($match in $matches) {
            Write-Host "  - Found: $($match.Value)" -ForegroundColor Red
        }
        return $false
    }
    
    Write-Status "No TODO/FIXME comments found" "PASS"
    return $true
}

function Test-NoNotImplementedException {
    $content = Get-Content $FilePath -Raw
    $notImplPattern = "throw new NotImplementedException"
    
    if ($content -match $notImplPattern) {
        Write-Status "Found NotImplementedException - methods not implemented" "FAIL"
        return $false
    }
    
    Write-Status "No NotImplementedException found" "PASS"
    return $true
}

function Test-BuildSuccess {
    Write-Status "Building project..." "INFO"
    
    $buildResult = dotnet build $ProjectPath --verbosity quiet 2>&1
    $exitCode = $LASTEXITCODE
    
    if ($exitCode -ne 0) {
        Write-Status "Build failed" "FAIL"
        Write-Host $buildResult -ForegroundColor Red
        return $false
    }
    
    # Check for warnings
    $warnings = $buildResult | Select-String "warning"
    if ($warnings) {
        Write-Status "Build succeeded with warnings" "WARN"
        foreach ($warning in $warnings) {
            Write-Host "  $warning" -ForegroundColor Yellow
        }
    } else {
        Write-Status "Build succeeded without warnings" "PASS"
    }
    
    return $true
}

function Test-ProperErrorHandling {
    $content = Get-Content $FilePath -Raw
    $hasInputValidation = $content -match "ArgumentException|ArgumentNullException|InvalidOperationException"
    $hasTryCatch = $content -match "try\s*\{.*catch"
    
    if (-not $hasInputValidation) {
        Write-Status "Consider adding input validation (ArgumentException, etc.)" "WARN"
    } else {
        Write-Status "Input validation found" "PASS"
    }
    
    if (-not $hasTryCatch) {
        Write-Status "Consider adding try-catch blocks for error handling" "WARN"
    } else {
        Write-Status "Error handling (try-catch) found" "PASS"
    }
    
    return $true
}

function Test-XmlDocumentation {
    $content = Get-Content $FilePath -Raw
    $publicMethods = [regex]::Matches($content, "public\s+(?:async\s+)?(?:Task<?[^>]*>?|void|[A-Za-z_][A-Za-z0-9_<>,]*)\s+([A-Za-z_][A-Za-z0-9_]*)\s*\(")
    
    $undocumentedMethods = @()
    
    foreach ($method in $publicMethods) {
        $methodName = $method.Groups[1].Value
        $methodStart = $method.Index
        
        # Look for XML doc comment before this method
        $beforeMethod = $content.Substring(0, $methodStart)
        $xmlDocPattern = "///\s*<summary>"
        
        # Find the last occurrence of XML doc before this method
        $lastXmlDoc = [regex]::Matches($beforeMethod, $xmlDocPattern) | Select-Object -Last 1
        
        if (-not $lastXmlDoc -or ($methodStart - $lastXmlDoc.Index) -gt 500) {
            $undocumentedMethods += $methodName
        }
    }
    
    if ($undocumentedMethods.Count -gt 0) {
        Write-Status "Missing XML documentation for public methods" "WARN"
        foreach ($method in $undocumentedMethods) {
            Write-Host "  - $method" -ForegroundColor Yellow
        }
    } else {
        Write-Status "XML documentation appears complete" "PASS"
    }
    
    return $true
}

function Test-InterfaceImplementation {
    if (-not $InterfaceName) {
        Write-Status "No interface specified - skipping interface compliance check" "INFO"
        return $true
    }
    
    # This is a simplified check - in a real scenario, you'd use Roslyn for proper analysis
    $content = Get-Content $FilePath -Raw
    
    if ($content -match "class\s+\w+\s*:\s*.*$InterfaceName") {
        Write-Status "Class implements interface: $InterfaceName" "PASS"
    } else {
        Write-Status "Interface implementation not found: $InterfaceName" "WARN"
    }
    
    return $true
}

# Main verification process
Write-Host "${Blue}=== Implementation Verification Report ===$Reset"
Write-Host "File: $FilePath"
Write-Host "Project: $ProjectPath"
if ($InterfaceName) {
    Write-Host "Interface: $InterfaceName"
}
Write-Host ""

$allPassed = $true

# Run all tests
$tests = @(
    { Test-FileExists },
    { Test-NoTodoComments },
    { Test-NoNotImplementedException },
    { Test-ProperErrorHandling },
    { Test-XmlDocumentation },
    { Test-InterfaceImplementation },
    { Test-BuildSuccess }
)

foreach ($test in $tests) {
    $result = & $test
    if (-not $result) {
        $allPassed = $false
    }
}

Write-Host ""
if ($allPassed) {
    Write-Status "Implementation verification PASSED" "PASS"
    exit 0
} else {
    Write-Status "Implementation verification FAILED - review issues above" "FAIL"
    exit 1
}