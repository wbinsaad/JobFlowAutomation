[CmdletBinding()]
param(
    [switch] $VerifyNoChanges
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$projectRoot = Split-Path -Parent $PSScriptRoot
$solutionPath = Join-Path $projectRoot "JobFlowAutomation.slnx"
$coverageSettingsPath = Join-Path $projectRoot "coverlet.runsettings"
$testResultsPath = Join-Path $projectRoot "TestResults"
$coverageReportPath = Join-Path $projectRoot "CoverageReport"
$coverageInputPattern = Join-Path $testResultsPath "**/coverage.cobertura.xml"

function Assert-RequiredFile {
    param(
        [Parameter(Mandatory)]
        [string] $Path
    )

    if (-not (Test-Path -Path $Path -PathType Leaf)) {
        throw "Required file was not found: $Path"
    }
}

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

Assert-RequiredFile -Path $solutionPath
Assert-RequiredFile -Path $coverageSettingsPath

Push-Location $projectRoot

try {
    Write-Host ""
    Write-Host "Starting project quality checks..." -ForegroundColor Green

    Write-Host ""
    Write-Host "Restoring .NET tools..." -ForegroundColor Yellow

    Invoke-DotNet @(
        "tool",
        "restore"
    )

    Write-Host ""
    Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow

    Invoke-DotNet @(
        "restore",
        $solutionPath
    )

    if ($VerifyNoChanges) {
        Write-Host ""
        Write-Host "Verifying code formatting..." -ForegroundColor Yellow

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
        Write-Host "Applying code formatting..." -ForegroundColor Yellow

        Invoke-DotNet @(
            "format",
            $solutionPath,
            "--no-restore"
        )
    }

    Write-Host ""
    Write-Host "Building the solution..." -ForegroundColor Yellow

    $buildArguments = @(
        "build",
        $solutionPath,
        "--configuration",
        "Release",
        "--no-restore"
    )

    if ($env:GITHUB_ACTIONS -eq "true") {
        $buildArguments += "-p:ContinuousIntegrationBuild=true"
    }

    Invoke-DotNet $buildArguments

    foreach ($generatedPath in @($testResultsPath, $coverageReportPath)) {
        if (Test-Path $generatedPath) {
            Remove-Item -Path $generatedPath -Recurse -Force
        }
    }

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
        "console;verbosity=normal",
        "--logger",
        "trx;LogFilePrefix=test-results"
    )

    Write-Host ""
    Write-Host "Generating the merged coverage report..." -ForegroundColor Yellow

    Invoke-DotNet @(
        "reportgenerator",
        "-reports:$coverageInputPattern",
        "-targetdir:$coverageReportPath",
        "-reporttypes:Html;Cobertura;MarkdownSummaryGithub;TextSummary",
        "-title:JobFlowAutomation Code Coverage"
    )

    $textSummaryPath = Join-Path $coverageReportPath "Summary.txt"

    if (Test-Path $textSummaryPath) {
        Write-Host ""
        Get-Content -Path $textSummaryPath
    }

    Write-Host ""
    Write-Host "All quality checks passed." -ForegroundColor Green
    Write-Host "Test results: $testResultsPath" -ForegroundColor Green
    Write-Host "Coverage report: $coverageReportPath" -ForegroundColor Green
}
finally {
    Pop-Location
}
