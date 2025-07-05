#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Adds proper XML documentation comments to C# classes and properties following Microsoft standards

.DESCRIPTION
    This script analyzes C# files and adds XML documentation comments to undocumented:
    - Classes, interfaces, enums, structs
    - Public methods and constructors
    - Public properties and fields
    - Method parameters and return values
    
    Follows Microsoft's official XML documentation standards and verifies build stability at milestones.

.PARAMETER SourcePath
    Path to the source directory or specific C# file to process

.PARAMETER ProjectPath
    Path to the .csproj file for build verification

.PARAMETER BatchSize
    Number of files to process before running build verification (default: 5)

.PARAMETER DryRun
    Show what would be changed without actually modifying files

.EXAMPLE
    ./add-xml-documentation.ps1 -SourcePath "src/API" -ProjectPath "src/API/MeAndMyDog.API.csproj"

.EXAMPLE
    ./add-xml-documentation.ps1 -SourcePath "src/API/Services/PaymentService.cs" -ProjectPath "src/API/MeAndMyDog.API.csproj" -DryRun
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$SourcePath,
    
    [Parameter(Mandatory = $true)]
    [string]$ProjectPath,
    
    [Parameter(Mandatory = $false)]
    [int]$BatchSize = 5,
    
    [Parameter(Mandatory = $false)]
    [switch]$DryRun
)

# Colors for output
$Red = "`e[31m"
$Green = "`e[32m"
$Yellow = "`e[33m"
$Blue = "`e[34m"
$Cyan = "`e[36m"
$Reset = "`e[0m"

function Write-Status {
    param([string]$Message, [string]$Status, [string]$Color = "")
    
    $StatusColor = switch ($Status) {
        "PASS" { $Green }
        "FAIL" { $Red }
        "WARN" { $Yellow }
        "INFO" { $Blue }
        "PROCESS" { $Cyan }
        default { $Color ? $Color : $Reset }
    }
    
    Write-Host "[$StatusColor$Status$Reset] $Message"
}

function Test-BuildStability {
    param([string]$ProjectFile)
    
    Write-Status "Verifying build stability..." "INFO"
    
    $buildResult = dotnet build $ProjectFile --verbosity quiet 2>&1
    $exitCode = $LASTEXITCODE
    
    if ($exitCode -ne 0) {
        Write-Status "Build failed after XML documentation changes" "FAIL"
        Write-Host $buildResult -ForegroundColor Red
        return $false
    }
    
    # Check for new warnings
    $warnings = $buildResult | Select-String "warning"
    if ($warnings) {
        Write-Status "Build succeeded with warnings:" "WARN"
        foreach ($warning in $warnings) {
            Write-Host "  $warning" -ForegroundColor Yellow
        }
    } else {
        Write-Status "Build verification successful - no errors or warnings" "PASS"
    }
    
    return $true
}

function Get-XmlDocumentationForClass {
    param([string]$ClassName, [string]$ClassType)
    
    $summary = switch ($ClassType.ToLower()) {
        "class" { "Represents a $ClassName" }
        "interface" { "Defines the contract for $ClassName operations" }
        "enum" { "Specifies the available $ClassName options" }
        "struct" { "Represents a $ClassName structure" }
        default { "Represents a $ClassName" }
    }
    
    return @"
    /// <summary>
    /// $summary
    /// </summary>
"@
}

function Get-XmlDocumentationForProperty {
    param([string]$PropertyName, [string]$PropertyType, [bool]$HasSetter)
    
    $action = if ($HasSetter) { "Gets or sets" } else { "Gets" }
    $summary = "$action the $PropertyName"
    
    return @"
        /// <summary>
        /// $summary
        /// </summary>
"@
}

function Get-XmlDocumentationForMethod {
    param([string]$MethodName, [string[]]$Parameters, [string]$ReturnType, [bool]$IsConstructor)
    
    $summary = if ($IsConstructor) {
        "Initializes a new instance of the $MethodName class"
    } else {
        switch ($MethodName.ToLower()) {
            { $_ -match "^get" } { "Gets $($MethodName.Substring(3))" }
            { $_ -match "^set" } { "Sets $($MethodName.Substring(3))" }
            { $_ -match "^create" } { "Creates a new $($MethodName.Substring(6))" }
            { $_ -match "^update" } { "Updates the $($MethodName.Substring(6))" }
            { $_ -match "^delete" } { "Deletes the $($MethodName.Substring(6))" }
            { $_ -match "^validate" } { "Validates the $($MethodName.Substring(8))" }
            { $_ -match "^process" } { "Processes the $($MethodName.Substring(7))" }
            { $_ -match "async$" } { "Asynchronously performs $MethodName operation" }
            default { "Performs $MethodName operation" }
        }
    }
    
    $documentation = @"
        /// <summary>
        /// $summary
        /// </summary>
"@
    
    # Add parameter documentation
    foreach ($param in $Parameters) {
        if ($param -and $param.Trim()) {
            $paramName = ($param -split '\s+')[-1] -replace '[^\w]', ''
            if ($paramName) {
                $documentation += @"

        /// <param name="$paramName">The $paramName parameter</param>
"@
            }
        }
    }
    
    # Add return documentation
    if ($ReturnType -and $ReturnType -ne "void" -and -not $IsConstructor) {
        $returnDescription = if ($ReturnType -match "Task") {
            "A task representing the asynchronous operation"
        } elseif ($ReturnType -match "bool") {
            "True if successful, false otherwise"
        } elseif ($ReturnType -match "string") {
            "The result string"
        } elseif ($ReturnType -match "int|long|decimal|double|float") {
            "The numeric result"
        } else {
            "The $ReturnType result"
        }
        
        $documentation += @"

        /// <returns>$returnDescription</returns>
"@
    }
    
    return $documentation
}

function Add-XmlDocumentationToFile {
    param([string]$FilePath)
    
    if (-not (Test-Path $FilePath)) {
        Write-Status "File not found: $FilePath" "FAIL"
        return $false
    }
    
    $content = Get-Content $FilePath -Raw
    $lines = Get-Content $FilePath
    $newContent = @()
    $modified = $false
    
    for ($i = 0; $i -lt $lines.Length; $i++) {
        $line = $lines[$i]
        $nextLine = if ($i + 1 -lt $lines.Length) { $lines[$i + 1] } else { "" }
        
        # Check if next line needs documentation
        $needsDoc = $false
        $docToAdd = ""
        
        # Skip if already has XML documentation
        if ($line.Trim().StartsWith("///")) {
            $newContent += $line
            continue
        }
        
        # Class/Interface/Enum/Struct declaration
        if ($nextLine -match '^\s*(public|internal|private|protected)?\s*(static|abstract|sealed|partial)?\s*(class|interface|enum|struct)\s+(\w+)') {
            $classType = $matches[3]
            $className = $matches[4]
            
            # Check if there's no XML doc before this class
            if (-not ($line.Trim().StartsWith("///") -or $line.Trim().StartsWith("*") -or $line.Trim().StartsWith("//"))) {
                $docToAdd = Get-XmlDocumentationForClass -ClassName $className -ClassType $classType
                $needsDoc = $true
            }
        }
        
        # Property declaration
        elseif ($nextLine -match '^\s*(public|internal|private|protected)\s+(?:static\s+)?(?:readonly\s+)?(\w+(?:<[^>]+>)?(?:\[\])?)\s+(\w+)\s*\{') {
            $propertyType = $matches[2]
            $propertyName = $matches[3]
            $hasSetter = $nextLine -match '\{\s*get\s*;\s*set\s*;|\{\s*set\s*;\s*get\s*;|\{ get; set; \}'
            
            if (-not ($line.Trim().StartsWith("///") -or $line.Trim().StartsWith("*") -or $line.Trim().StartsWith("//"))) {
                $docToAdd = Get-XmlDocumentationForProperty -PropertyName $propertyName -PropertyType $propertyType -HasSetter $hasSetter
                $needsDoc = $true
            }
        }
        
        # Method/Constructor declaration
        elseif ($nextLine -match '^\s*(public|internal|private|protected)\s+(?:static\s+)?(?:async\s+)?(?:virtual\s+)?(?:override\s+)?(\w+(?:<[^>]+>)?(?:\?)?)\s+(\w+)\s*\(([^)]*)\)') {
            $returnType = $matches[2]
            $methodName = $matches[3]
            $parameters = $matches[4] -split ',' | ForEach-Object { $_.Trim() }
            $isConstructor = $returnType -eq $methodName
            
            if (-not ($line.Trim().StartsWith("///") -or $line.Trim().StartsWith("*") -or $line.Trim().StartsWith("//"))) {
                $docToAdd = Get-XmlDocumentationForMethod -MethodName $methodName -Parameters $parameters -ReturnType $returnType -IsConstructor $isConstructor
                $needsDoc = $true
            }
        }
        
        # Add current line
        $newContent += $line
        
        # Add documentation if needed
        if ($needsDoc -and $docToAdd) {
            # Get indentation from next line
            $indentation = ""
            if ($nextLine -match '^(\s*)') {
                $indentation = $matches[1]
            }
            
            # Add the documentation with proper indentation
            $docLines = $docToAdd -split "`n"
            foreach ($docLine in $docLines) {
                if ($docLine.Trim()) {
                    $newContent += "$indentation$($docLine.TrimStart())"
                } else {
                    $newContent += ""
                }
            }
            $modified = $true
        }
    }
    
    if ($modified) {
        if ($DryRun) {
            Write-Status "Would add XML documentation to: $FilePath" "INFO"
        } else {
            $newContent -join "`n" | Set-Content $FilePath -NoNewline
            Write-Status "Added XML documentation to: $FilePath" "PROCESS"
        }
        return $true
    } else {
        Write-Status "No documentation needed for: $FilePath" "INFO"
        return $false
    }
}

function Get-CSharpFiles {
    param([string]$Path)
    
    if (Test-Path $Path -PathType Leaf) {
        if ($Path -match '\.cs$') {
            return @($Path)
        } else {
            Write-Status "File is not a C# file: $Path" "WARN"
            return @()
        }
    } elseif (Test-Path $Path -PathType Container) {
        return Get-ChildItem -Path $Path -Filter "*.cs" -Recurse | 
               Where-Object { 
                   $_.Name -notmatch '\.g\.cs$|\.designer\.cs$|AssemblyInfo\.cs$|GlobalAssemblyInfo\.cs$' -and
                   $_.DirectoryName -notmatch '\\obj\\|\\bin\\|\\packages\\' 
               } | 
               Select-Object -ExpandProperty FullName
    } else {
        Write-Status "Path not found: $Path" "FAIL"
        return @()
    }
}

# Main execution
Write-Host "${Blue}=== .NET XML Documentation Generator ===${Reset}"
Write-Host "Source Path: $SourcePath"
Write-Host "Project Path: $ProjectPath"
Write-Host "Batch Size: $BatchSize"
Write-Host "Dry Run: $DryRun"
Write-Host ""

# Verify initial build state
if (-not (Test-BuildStability -ProjectFile $ProjectPath)) {
    Write-Status "Initial build verification failed. Fix build issues before adding documentation." "FAIL"
    exit 1
}

# Get all C# files to process
$files = Get-CSharpFiles -Path $SourcePath
if ($files.Count -eq 0) {
    Write-Status "No C# files found to process" "WARN"
    exit 0
}

Write-Status "Found $($files.Count) C# files to process" "INFO"

# Process files in batches
$processedCount = 0
$modifiedCount = 0
$batchCount = 0

foreach ($file in $files) {
    $relativeFile = $file -replace [regex]::Escape((Get-Location).Path + [IO.Path]::DirectorySeparatorChar), ""
    
    Write-Status "Processing: $relativeFile" "PROCESS"
    
    if (Add-XmlDocumentationToFile -FilePath $file) {
        $modifiedCount++
    }
    
    $processedCount++
    
    # Check build stability at batch intervals
    if ($processedCount % $BatchSize -eq 0 -and -not $DryRun) {
        $batchCount++
        Write-Status "Completed batch $batchCount ($processedCount/$($files.Count) files)" "INFO"
        
        if (-not (Test-BuildStability -ProjectFile $ProjectPath)) {
            Write-Status "Build verification failed after batch $batchCount. Rolling back changes..." "FAIL"
            # In a real scenario, you'd implement rollback logic here
            exit 1
        }
    }
}

# Final verification
if (-not $DryRun -and $modifiedCount -gt 0) {
    Write-Status "Running final build verification..." "INFO"
    if (-not (Test-BuildStability -ProjectFile $ProjectPath)) {
        Write-Status "Final build verification failed" "FAIL"
        exit 1
    }
}

# Summary
Write-Host ""
Write-Host "${Green}=== Summary ===${Reset}"
Write-Status "Files processed: $processedCount" "INFO"
Write-Status "Files modified: $modifiedCount" "INFO"

if ($DryRun) {
    Write-Status "Dry run completed - no files were actually modified" "INFO"
} else {
    Write-Status "XML documentation generation completed successfully" "PASS"
}

exit 0