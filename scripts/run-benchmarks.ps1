<#
.SYNOPSIS
    Запуск бенчмарків розділу з правильними параметрами (Release, net10.0, без дебагера).

.EXAMPLE
    ./scripts/run-benchmarks.ps1 -Filter '*Memory*'
    ./scripts/run-benchmarks.ps1 -Filter '*Runtimes*' -NoShortJob      # класи з власними job-ами
    ./scripts/run-benchmarks.ps1 -Filter '*HardwareCounters*'           # потребує терміналу адміністратора
    ./scripts/run-benchmarks.ps1 -Filter '*' -Exporters json            # повний JSON для compare-results.ps1
#>
param(
    [Parameter(Mandatory)] [string[]] $Filter,
    # Не додавати --job short (для класів, де job-и задані в коді: 04_Runtimes, 10_DotNet10Features тощо).
    [switch] $NoShortJob,
    # Додаткові експортери BenchmarkDotNet (--exporters), наприклад json, html, csv.
    [string[]] $Exporters = @(),
    # Куди складати результати (--artifacts).
    [string] $ArtifactsPath = ''
)

$ErrorActionPreference = 'Stop'
$adminFilters = '*HardwareCounters*', '*EtwProfiler*', '*NativeMemory*', '*InliningDiagnoser*', '*TailCallDiagnoser*'
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
    [Security.Principal.WindowsBuiltInRole]::Administrator)

foreach ($f in $Filter) {
    if (-not $isAdmin -and ($adminFilters | Where-Object { $f -like $_ -or $_ -like $f })) {
        Write-Warning "Фільтр '$f' зачіпає бенчмарки, що потребують прав адміністратора (ETW). Без них валідатор зупинить запуск."
    }
}

$arguments = @('run', '-c', 'Release', '-f', 'net10.0', '--', '--filter') + $Filter
if (-not $NoShortJob) { $arguments += @('--job', 'short') }
if ($Exporters.Count -gt 0) { $arguments += @('--exporters') + $Exporters }
if ($ArtifactsPath) { $arguments += @('--artifacts', $ArtifactsPath) }

Push-Location (Join-Path $PSScriptRoot '..\src\Showcase.Benchmarks')
try {
    Write-Host "dotnet $($arguments -join ' ')"
    & dotnet @arguments
    exit $LASTEXITCODE
}
finally {
    Pop-Location
}
