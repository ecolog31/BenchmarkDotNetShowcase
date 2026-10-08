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
| 01_Basics | `MinimalBenchmark` | мінімальний `[Benchmark]` + `[GlobalSetup]` |
| 01_Basics | `BaselineRatioBenchmarks` | `Baseline = true`, колонки Ratio / RatioSD |
| 01_Basics | `CategoriesBenchmarks` | `[BenchmarkCategory]`, `[GroupBenchmarksBy(ByCategory)]`, `[CategoriesColumn]`, окремий baseline у кожній категорії |
| 01_Basics | `OperationsPerInvokeBenchmarks` | `OperationsPerInvoke`: час на одну операцію в пакетному методі |
| 01_Basics | `OrderingAndRankBenchmarks` | `[Orderer]`, `[RankColumn]`, `[LogicalGroupColumn]`, `[GroupBenchmarksBy(ByParams)]` |
| 01_Basics | `CommonPitfallsBenchmarks` | dead code elimination, згортання констант, відкладений LINQ, `Consumer` |

## Результати

Нижче — реальні виводи короткого запуску (`--job short`: 3 прогрівання, 3 ітерації),
тому Error великий; для висновків потрібен повний запуск без `--job short`.

Середовище:

```
BenchmarkDotNet v0.15.8, Windows 11 (10.0.26300.9457)
AMD Ryzen 5 5600 3.50GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.401
  [Host]   : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
```

### 01_Basics

**BaselineRatioBenchmarks** — сума 10 000 int. `foreach` швидший за `for` по полю,
векторизований `Enumerable.Sum` — найшвидший:

| Method      | Mean       | Error       | StdDev    | Ratio | RatioSD |
|------------ |-----------:|------------:|----------:|------:|--------:|
| ForLoop     | 4,646.7 ns | 1,915.27 ns | 104.98 ns |  1.00 |    0.03 |
| ForeachLoop | 2,674.4 ns | 1,666.80 ns |  91.36 ns |  0.58 |    0.02 |
| LinqSum     |   648.8 ns |    30.69 ns |   1.68 ns |  0.14 |    0.00 |

**CategoriesBenchmarks** — дві незалежні групи, Ratio рахується всередині кожної:

| Method       | Categories | Mean          | Error         | StdDev        | Ratio | RatioSD |
|------------- |----------- |--------------:|--------------:|--------------:|------:|--------:|
| LinearSearch | Search     |     259.04 ns |      29.06 ns |      1.593 ns |  1.00 |    0.01 |
| BinarySearch | Search     |      17.17 ns |      30.11 ns |      1.651 ns |  0.07 |    0.01 |
|              |            |               |               |               |       |         |
| ArraySort    | Sort       | 332,237.51 ns | 214,846.43 ns | 11,776.458 ns |  1.00 |    0.04 |
| LinqOrderBy  | Sort       | 551,615.58 ns |  14,114.48 ns |    773.662 ns |  1.66 |    0.05 |

**OperationsPerInvokeBenchmarks** — обидва рядки показують час одного `Math.Sqrt`; у пакетному
методі додаються накопичення суми й керування циклом:

| Method      | Mean     | Error     | StdDev    | Ratio |
|------------ |---------:|----------:|----------:|------:|
| SingleSqrt  | 3.237 ns | 0.0858 ns | 0.0047 ns |  1.00 |
| BatchedSqrt | 4.731 ns | 0.3291 ns | 0.0180 ns |  1.46 |

**OrderingAndRankBenchmarks** — сортування від найшвидшого, ранги в межах кожної групи `Count`.
На 1 000 елементів алгоритм Кернігана вдвічі швидший за наївний цикл, а на 100 000 вже ні:
передбачувач переходів «запам'ятовує» малий набір даних, але не великий:

| Method           | Count  | Mean           | Error         | StdDev       | Rank | LogicalGroup   |
|----------------- |------- |---------------:|--------------:|-------------:|-----:|--------------- |
| HardwarePopCount | 1000   |       236.4 ns |      18.10 ns |      0.99 ns |    1 | [Count=1000]   |
| Kernighan        | 1000   |     5,570.1 ns |      93.56 ns |      5.13 ns |    2 | [Count=1000]   |
| NaiveLoop        | 1000   |    12,254.6 ns |   2,270.66 ns |    124.46 ns |    3 | [Count=1000]   |
|                  |        |                |               |              |      |                |
| HardwarePopCount | 100000 |    23,104.5 ns |   4,628.09 ns |    253.68 ns |    1 | [Count=100000] |
| NaiveLoop        | 100000 | 1,045,495.5 ns | 255,013.02 ns | 13,978.12 ns |    2 | [Count=100000] |
| Kernighan        | 100000 | 1,202,930.4 ns | 196,993.01 ns | 10,797.85 ns |    2 | [Count=100000] |

**CommonPitfallsBenchmarks** — неправильні методи дають ≈ 0 ns, і BenchmarkDotNet сам про це попереджає:

| Method                     | Mean          | Error         | StdDev     | Median        | Ratio   | RatioSD |
|--------------------------- |--------------:|--------------:|-----------:|--------------:|--------:|--------:|
| Wrong_ResultDiscarded      |     0.0606 ns |     0.5179 ns |  0.0284 ns |     0.0758 ns |   0.018 |    0.01 |
| Wrong_ConstantFolding      |     0.0000 ns |     0.0000 ns |  0.0000 ns |     0.0000 ns |   0.000 |    0.00 |
| Right_ReturnResult         |     3.3379 ns |     0.2343 ns |  0.0128 ns |     3.3357 ns |   1.000 |    0.00 |
| Right_LazyLinqMaterialized |   943.0713 ns |   413.7080 ns | 22.6767 ns |   946.7402 ns | 282.534 |    5.96 |
| Right_LazyLinqConsumed     | 1,196.3843 ns | 1,817.1134 ns | 99.6021 ns | 1,160.3543 ns | 358.424 |   25.87 |

```
ZeroMeasurement
  CommonPitfallsBenchmarks.Wrong_ResultDiscarded: ShortRun -> The method duration is indistinguishable from the empty method duration
  CommonPitfallsBenchmarks.Wrong_ConstantFolding: ShortRun -> The method duration is indistinguishable from the empty method duration
```

Відкладений LINQ-запит (`IEnumerable` з `Where`/`Select`) повернути з бенчмарка взагалі не вийде:
валідатор зупиняє запуск з помилкою
`returns a deferred execution result (IEnumerable<Int32>). You need to either change the method declaration to return a materialized result or consume it on your own.`
