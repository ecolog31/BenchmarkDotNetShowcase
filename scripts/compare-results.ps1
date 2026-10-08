<#
.SYNOPSIS
    Порівнює два набори результатів BenchmarkDotNet (JSON-експорт: --exporters json дає *-report-full-compressed.json)
    і показує зміну Mean для кожного бенчмарка. Корисно для виявлення регресій між комітами.

.DESCRIPTION
    1. На «старому» коміті:  ./scripts/run-benchmarks.ps1 -Filter '*Basics*' -Exporters json -ArtifactsPath ../../results/base
    2. На «новому» коміті:   ./scripts/run-benchmarks.ps1 -Filter '*Basics*' -Exporters json -ArtifactsPath ../../results/head
    3. Порівняння:           ./scripts/compare-results.ps1 -Baseline results/base -Current results/head -Threshold 10
    Скрипт повертає код 1, якщо хоча б один бенчмарк повільніший більше ніж на Threshold відсотків (для CI).
    Для статистично строгого порівняння в межах одного запуску є вбудований --statisticalTest 5%.
#>
param(
    [Parameter(Mandatory)] [string] $Baseline,
    [Parameter(Mandatory)] [string] $Current,
    [double] $Threshold = 10
)

$ErrorActionPreference = 'Stop'

function Read-Results([string] $path) {
    $files = Get-ChildItem -Path $path -Recurse -Filter '*-report*.json'
    if (-not $files) { throw "У '$path' не знайдено JSON-звітів BenchmarkDotNet (*-report*.json)." }
    $map = @{}
    foreach ($file in $files) {
        $report = Get-Content $file.FullName -Raw | ConvertFrom-Json
        foreach ($b in $report.Benchmarks) {
            if ($null -ne $b.Statistics) { $map[$b.FullName] = [double] $b.Statistics.Mean }
        }
    }
    return $map
}

$base = Read-Results $Baseline
$head = Read-Results $Current
$regressions = 0

$rows = foreach ($name in ($base.Keys + $head.Keys | Sort-Object -Unique)) {
    $old = $base[$name]; $new = $head[$name]
    if ($null -eq $old -or $null -eq $new) {
        [pscustomobject]@{ Benchmark = $name; 'Base, ns' = $old; 'Current, ns' = $new; 'Change, %' = $null; Verdict = 'only in one run' }
        continue
    }
    $change = ($new - $old) / $old * 100
    $verdict = if ($change -gt $Threshold) { $regressions++; 'SLOWER' } elseif ($change -lt -$Threshold) { 'faster' } else { 'same' }
    [pscustomobject]@{
        Benchmark     = $name
        'Base, ns'    = [math]::Round($old, 2)
        'Current, ns' = [math]::Round($new, 2)
        'Change, %'   = [math]::Round($change, 1)
        Verdict       = $verdict
    }
}

$rows | Format-Table -AutoSize
if ($regressions -gt 0) {
    Write-Warning "$regressions бенчмарк(ів) повільніші більше ніж на $Threshold%."
    exit 1
}
