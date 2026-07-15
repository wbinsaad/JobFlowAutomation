[CmdletBinding()]
param(
    [switch]$VerifyNoChanges
)

$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $PSScriptRoot

$solutionPath = Join-Path `
    $projectRoot `
    "JobFlowAutomation.slnx"

$coverageSettingsPath = Join-Path `
    $projectRoot `
    "coverlet.runsettings"

$testResultsPath = Join-Path `
    $projectRoot `
    "TestResults"

function Invoke-DotNet {
    param(
        [Parameter(Mandatory)]
        [string[]] $Arguments
    )

    Write-Host ""
    Write-Host "> dotnet $($Arguments -join ' ')" -ForegroundColor Cyan

    & dotnet @Arguments

    if ($LASTEXITCODE -ne 0) {
        throw "The dotnet command failed with exit code $LASTEXITCODE."
    }
}

Push-Location $projectRoot

try {
    Write-Host ""
    Write-Host "Starting project quality checks..." -ForegroundColor Green

    # Step 1: Restore NuGet packages.
    Invoke-DotNet @(
        "restore",
        $solutionPath
    )

    # Step 2: Format the code or verify its formatting.
    if ($VerifyNoChanges) {
        Write-Host ""
        Write-Host "Checking code formatting..." -ForegroundColor Yellow
    
        Invoke-DotNet @(
            "format",
            $solutionPath,
            "--verify-no-changes",
            "--no-restore",
            "--verbosity",
            "normal"
        )
    }
    else {
        Write-Host ""
        Write-Host "Applying formatting fixes..." -ForegroundColor Yellow
    
        Invoke-DotNet @(
            "format",
            $solutionPath,
            "--no-restore"
        )
    }

    # Step 3: Build the complete solution.
    Write-Host ""
    Write-Host "Building the solution..." -ForegroundColor Yellow

    Invoke-DotNet @(
        "build",
        $solutionPath,
        "--configuration",
        "Release",
        "--no-restore"
    )

    # Delete results from the previous run.
    if (Test-Path $testResultsPath) {
        Remove-Item `
            $testResultsPath `
            -Recurse `
            -Force
    }

    # Step 4: Run all tests and collect coverage.
    Write-Host ""
    Write-Host "Running tests with code coverage..." -ForegroundColor Yellow

    Invoke-DotNet @(
        "test",
        $solutionPath,
        "--configuration",
        "Release",
        "--no-build",
        "--settings",
        $coverageSettingsPath,
        "--collect",
        "XPlat Code Coverage",
        "--results-directory",
        $testResultsPath,
        "--logger",
        "console;verbosity=normal"
    )

    Write-Host ""
    Write-Host "All quality checks passed." -ForegroundColor Green
    Write-Host "Coverage results: $testResultsPath" -ForegroundColor Green
}
finally {
    Pop-Location
}