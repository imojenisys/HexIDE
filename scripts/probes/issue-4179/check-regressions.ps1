# Fixed regression slice for mutation-check.ps1 -TestScript. The first argument is the build project
# required by that helper; each case here selects the actual regression path for #4179 / oracle PR #4350.
# Build the changed Runtime or Desktop assembly with the helper first. These implementation-only mutations
# keep the already-built host APIs stable; test builds resolve and copy the fresh dependency outputs.
$ErrorActionPreference = 'Stop'
$regressionRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$testRunner = Join-Path $regressionRoot 'scripts/test.ps1'
$cases = @(
    @{ Project = 'IDE/HexIDE.Runtime.Tests'; Args = @('--filter-class', '*NegativeSizeLoadTests'); Minimum = 4 },
    @{ Project = 'IDE/HexIDE.Runtime.Integration.Tests'; Args = @('--filter-class', '*NegativeSizeRunTests'); Minimum = 3 },
    @{ Project = 'IDE/HexIDE.Integration.Tests'; Args = @('--filter-method', '*Negative_saved_sizes_run_as_zero'); Minimum = 2 },
    @{ Project = 'IDE/HexIDE.Integration.Tests'; Args = @('--filter-class', '*StartupFormFailsToLoadTests'); Minimum = 3 },
    @{ Project = 'IDE/HexIDE.Tests'; Args = @('--filter-class', '*NegativeSizeSaveTests'); Minimum = 12 },
    @{ Project = 'IDE/HexIDE.Tests'; Args = @('--filter-class', '*DesignerSizeTests'); Minimum = 16 }
)
$failed = $false
Push-Location $regressionRoot
try {
    foreach ($case in $cases) {
        $caseArgs = $case.Args + @('--minimum-expected-tests', [string]$case.Minimum)
        if ($case.Project.Contains('Integration.Tests')) {
            $caseArgs += @('--hangdump', '--hangdump-timeout', '20m')
        }
        & pwsh -NoProfile -File $testRunner $case.Project '-p:BuildProjectReferences=false' '--' @caseArgs
        if ($LASTEXITCODE -ne 0) { $failed = $true }
    }
} finally { Pop-Location }
if ($failed) { exit 1 }
exit 0
