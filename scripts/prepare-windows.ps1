<#
.SYNOPSIS
    Підготовка Windows до запуску бенчмарків (PLAN.md, розділ 5). Без параметрів лише ПЕРЕВІРЯЄ стан і нічого не змінює.

.EXAMPLE
    ./scripts/prepare-windows.ps1            # перевірка
    ./scripts/prepare-windows.ps1 -Apply     # увімкнути High performance; з правами адміністратора — ще й виключення Defender
#>
param([switch] $Apply)

$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
    [Security.Principal.WindowsBuiltInRole]::Administrator)
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$highPerformance = '8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c'

# 1. Схема живлення
$active = (powercfg /getactivescheme) -join ' '
Write-Host "Схема живлення: $active"
if ($active -notmatch $highPerformance) {
    if ($Apply) { powercfg /setactive $highPerformance; Write-Host '  -> увімкнено High performance' }
    else { Write-Warning '  Рекомендовано High performance: ./scripts/prepare-windows.ps1 -Apply' }
}
# (BenchmarkDotNet і сам перемикає схему на час запуску — див. «Setup power plan» у виводі.)

# 2. Живлення від мережі (для ноутбуків)
$battery = Get-CimInstance -ClassName Win32_Battery -ErrorAction SilentlyContinue
if ($battery -and $battery.BatteryStatus -ne 2) { Write-Warning 'Ноутбук працює від батареї — підключіть до мережі.' }

# 3. Виключення Windows Defender для папки проєкту (згенеровані exe й артефакти)
try {
    $exclusions = (Get-MpPreference -ErrorAction Stop).ExclusionPath
    if ($exclusions -contains $repoRoot) { Write-Host "Defender: '$repoRoot' уже у виключеннях." }
    elseif ($Apply -and $isAdmin) { Add-MpPreference -ExclusionPath $repoRoot; Write-Host "Defender: додано виключення '$repoRoot'." }
    else { Write-Warning "Defender: додайте виключення (адміністратор): Add-MpPreference -ExclusionPath '$repoRoot'" }
}
catch { Write-Host 'Defender: стан недоступний (потрібні права адміністратора або інший антивірус).' }

# 4. Права адміністратора для ETW-діагностик
if ($isAdmin) { Write-Host 'Термінал запущено від адміністратора: HardwareCounters / EtwProfiler / NativeMemoryProfiler доступні.' }
else { Write-Host 'Без прав адміністратора: HardwareCounters / EtwProfiler / NativeMemoryProfiler / Inlining / TailCall не запустяться.' }

# 5. Нагадування
Write-Host 'Перед запуском: закрийте браузер, месенджери, зайві IDE; призупиніть оновлення та індексацію.'
