# BenchmarkDotNet Showcase

Демонстраційний C#-проєкт, що показує можливості [BenchmarkDotNet](https://benchmarkdotnet.org/) 0.15.8
на Windows x64 з порівнянням .NET 8, 9 і 10.

## Структура

| Шлях | Призначення |
|---|---|
| `src/Showcase.Benchmarks/` | консольний проєкт з бенчмарками (`BenchmarkSwitcher` + меню) |
| `src/Showcase.Library/` | код, який бенчмаркається |
| `scripts/` | допоміжні скрипти |

## Вимоги

- .NET SDK 8, 9 і 10 (проєкт збирається під `net8.0;net9.0;net10.0`)
- Для NativeAOT: Visual Studio з набором **Desktop development with C++**, а папка
  `C:\Program Files (x86)\Microsoft Visual Studio\Installer` має бути в `PATH`
- `[HardwareCounters]`, `[EtwProfiler]`, `[NativeMemoryProfiler]`: запуск терміналу від адміністратора

## Як запускати

Тільки в **Release** і без дебагера. З папки `src/Showcase.Benchmarks`:

```powershell
# інтерактивне меню
dotnet run -c Release -f net10.0

# список усіх бенчмарків
dotnet run -c Release -f net10.0 -- --list flat

# один розділ, короткий запуск
dotnet run -c Release -f net10.0 -- --filter "*Basics*" --job short
```

`-f net10.0` обов'язковий: проєкт націлений на кілька фреймворків, і `dotnet run` без нього не стартує.
Результати зберігаються в `BenchmarkDotNet.Artifacts/` (не комітяться).

## Фічі → класи

| Розділ | Клас | Що показує |
|---|---|---|
| _заповнюється по мірі реалізації розділів_ | | |
