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

## Бенчмарки для адміністратора

Ці класи використовують ETW-сесію ядра Windows. Без прав адміністратора валідатор зупиняє запуск з помилкою
`Must be elevated (Admin) to use ETW Kernel Session`. Запускати з терміналу «Run as administrator»,
з папки `src/Showcase.Benchmarks`:

```powershell
dotnet run -c Release -f net10.0 -- --filter "*NativeMemoryBenchmarks*" "*HardwareCounters*" "*EtwProfiler*" "*InliningDiagnoser*" "*TailCallDiagnoser*" --job short
```

| Клас | Що з'явиться у виводі |
|---|---|
| `NativeMemoryBenchmarks` | колонки Allocated native memory / Native memory leak |
| `BranchPredictionHardwareCountersBenchmarks`, `CacheMissesHardwareCountersBenchmarks` | колонки BranchMispredictions/Op, BranchInstructions/Op, CacheMisses/Op (на AMD Windows може не надати частину лічильників) |
| `EtwProfilerBenchmarks` | файл `*.etl` у `BenchmarkDotNet.Artifacts` для PerfView / Windows Performance Analyzer |
| `InliningDiagnoserBenchmarks`, `TailCallDiagnoserBenchmarks` | текстовий звіт JIT про інлайнінг / хвостові виклики під таблицею |

## Фічі → класи

| Розділ | Клас | Що показує |
|---|---|---|
| 01_Basics | `MinimalBenchmark` | мінімальний `[Benchmark]` + `[GlobalSetup]` |
| 01_Basics | `BaselineRatioBenchmarks` | `Baseline = true`, колонки Ratio / RatioSD |
| 01_Basics | `CategoriesBenchmarks` | `[BenchmarkCategory]`, `[GroupBenchmarksBy(ByCategory)]`, `[CategoriesColumn]`, окремий baseline у кожній категорії |
| 01_Basics | `OperationsPerInvokeBenchmarks` | `OperationsPerInvoke`: час на одну операцію в пакетному методі |
| 01_Basics | `OrderingAndRankBenchmarks` | `[Orderer]`, `[RankColumn]`, `[LogicalGroupColumn]`, `[GroupBenchmarksBy(ByParams)]` |
| 01_Basics | `CommonPitfallsBenchmarks` | dead code elimination, згортання констант, відкладений LINQ, `Consumer` |
| 02_Parameters | `ParamsCartesianBenchmarks` | `[Params]` з enum, bool, string; декартів добуток (3 × 2 × 2 = 12 рядків) |
| 02_Parameters | `ParamsSourceBenchmarks` | `[ParamsSource]` зі складними об'єктами та `ToString()` для таблиці |
| 02_Parameters | `ParamsAllValuesBenchmarks` | `[ParamsAllValues]` — усі значення enum автоматично |
| 02_Parameters | `ArgumentsBenchmarks` | `[Arguments]` на методі |
| 02_Parameters | `ArgumentsSourceBenchmarks` | `[ArgumentsSource]` з об'єктами-кейсами |
| 02_Parameters | `CollectionsBySizeBenchmarks` | `List<T>` vs `T[]` vs `Span<T>` на 10 / 1 000 / 100 000 |
| 03_Memory | `StringBuildingBenchmarks` | `[MemoryDiagnoser]`: `+=` vs `StringBuilder` vs `string.Create` vs `stackalloc Span<char>` |
| 03_Memory | `ClassVsStructBenchmarks` | алокації `class` vs `struct` vs `record struct` |
| 03_Memory | `LinqVsLoopBenchmarks` | алокації ітераторів LINQ і проміжного `ToList()` |
| 03_Memory | `ArrayPoolBenchmarks` | `ArrayPool<T>` vs `new`; Large Object Heap і Gen2 |
| 03_Memory | `ThreadingDiagnoserBenchmarks` | `[ThreadingDiagnoser]`: Lock Contentions, Completed Work Items |
| 03_Memory | `ExceptionCostBenchmarks` | `[ExceptionDiagnoser]`: ціна винятку vs `TryParse` |
| 03_Memory | `NativeMemoryBenchmarks` | `[NativeMemoryProfiler]`: native-алокації та витік (**адміністратор**) |
| 04_Runtimes | `RuntimeComparisonBenchmarks` | `[SimpleJob(RuntimeMoniker...)]`: .NET 8 / 9 / 10 / NativeAOT 10 в одній таблиці |
| 04_Runtimes | `JitSettingsBenchmarks` | кілька Job у `ManualConfig`, `WithEnvironmentVariable` (TieredPGO, TieredCompilation, ReadyToRun), `AsBaseline()` |
| 04_Runtimes | `GcModeBenchmarks` | `WithGcServer` / `WithGcConcurrent` (аналоги `[GcServer]`, `[GcConcurrent]`) |
| 04_Runtimes | `RunStrategyBenchmarks` | `RunStrategy.Throughput` / `ColdStart` / `Monitoring` |
| 04_Runtimes | `JobCharacteristicsBenchmarks` | `WithLaunchCount`, `WithWarmupCount`, `WithIterationCount`, `WithInvocationCount`, `WithUnrollFactor`, `WithPlatform` |
| 05_Diagnosers | `BoundsCheckDisassemblyBenchmarks` | `[DisassemblyDiagnoser]` (Intel-синтаксис, C#-код поруч, експорт GitHub/HTML/diff): bounds-check elimination і loop cloning |
| 05_Diagnosers | `SimdDisassemblyBenchmarks` | скалярний цикл vs `Vector<T>` vs `Vector256` + asm з ymm-інструкціями |
| 05_Diagnosers | `EventPipeProfilerBenchmarks` | `[EventPipeProfiler(CpuSampling)]` → `*.speedscope.json` (без адміністратора) |
| 05_Diagnosers | `BranchPredictionHardwareCountersBenchmarks` | `[HardwareCounters]`: BranchMispredictions, відсортовані vs випадкові дані (**адміністратор**) |
| 05_Diagnosers | `CacheMissesHardwareCountersBenchmarks` | `[HardwareCounters]`: CacheMisses, обхід матриці по рядках vs по стовпцях (**адміністратор**) |
| 05_Diagnosers | `EtwProfilerBenchmarks` | `[EtwProfiler]` → `*.etl` для PerfView / WPA (**адміністратор**) |
| 05_Diagnosers | `InliningDiagnoserBenchmarks` | `[InliningDiagnoser]`: рішення JIT про інлайнінг (**адміністратор**) |
| 05_Diagnosers | `TailCallDiagnoserBenchmarks` | `[TailCallDiagnoser]`: хвостові виклики x64 JIT (**адміністратор**) |
| Config | `CustomColumnsBenchmarks` + `CustomColumnsConfig` | власний `ManualConfig`, `[Config(typeof(...))]`, `StatisticColumn` (Median, P95, Min, Max), власний `IColumn` |
| Config | `FluentConfigBenchmarks` | fluent-API: `DefaultConfig.Instance.AddJob(...)...WithOptions(ConfigOptions...)` |
| Config | `JobAttributesBenchmarks` | `[DryJob]`, `[ShortRunJob]` |
| Config | `InProcessBenchmarks` | `[InProcess]` — без окремого процесу |
| Config | `ExportersBenchmarks` | Markdown (GitHub), HTML, CSV, JSON, XML, AsciiDoc, `[RPlotExporter]` |
| Config | `CustomLoggerBenchmarks` | власний `ILogger` (файл з часовими мітками) |
| Config | `ValidatorsBenchmarks` | `ExecutionValidator`, `ReturnValueValidator`, `JitOptimizationsValidator` |
| Config | `OutliersBenchmarks` | `OutlierMode.RemoveUpper` vs `DontRemove` |
| Config | `StatisticalTestBenchmarks` | `[StatisticalTestColumn("5%")]` (Mann–Whitney) |
| 06_Setup | `FileSerializationSetupBenchmarks` | `[GlobalSetup]` / `[GlobalCleanup]`: тимчасовий файл, серіалізація |
| 06_Setup | `IterationSetupBenchmarks` | `[IterationSetup]` / `[IterationCleanup]` з `Target`, `[InvocationCount(1)]` |
| 06_Setup | `TargetedSetupBenchmarks` | `[GlobalSetup(Targets = [...])]` — окремий setup для окремих методів |
| 06_Setup | `ConstructorVsGlobalSetupBenchmarks` | конструктор виконується до присвоєння `[Params]` |
| 07_Async | `TaskVsValueTaskBenchmarks` | `Task<T>` vs `ValueTask<T>` vs `Task` при синхронному завершенні |
| 07_Async | `AsyncOverheadBenchmarks` | `async/await` vs пряме повернення `Task`, `ConfigureAwait(false)` |
| 07_Async | `ParallelismBenchmarks` | `Parallel.For` vs PLINQ vs `Task.WhenAll` + `[ThreadingDiagnoser]` |

## Як читати вивід

| Колонка | Що означає |
|---|---|
| **Mean** | середній час однієї операції (після відкидання викидів) |
| **Error** | половина 99.9% довірчого інтервалу: справжнє середнє з високою ймовірністю в межах Mean ± Error. Великий Error — мало вимірів або шумне середовище |
| **StdDev** | стандартне відхилення вимірів — наскільки «стрибають» окремі ітерації |
| **Median** | медіана; з'являється, коли розподіл несиметричний — тоді вона чесніша за Mean |
| **Ratio** / **RatioSD** | у скільки разів повільніше (>1) або швидше (<1) за baseline, і розкид цього відношення |
| **Gen0 / Gen1 / Gen2** | кількість збірок GC відповідного покоління **на 1000 операцій** |
| **Allocated** | байти в керованій купі на одну операцію (`-` = 0 B) |
| **Alloc Ratio** | Allocated відносно baseline |
| **Rank** | ранг з урахуванням статистичної значущості: однаковий ранг — різниця не доведена |

Під таблицею BenchmarkDotNet пише попередження: `ZeroMeasurement` (метод не відрізнити від порожнього —
найчастіше dead code elimination), `MinIterationTime` (ітерація коротша за 100 мс — мало точності),
`MultimodalDistribution` (кілька «горбів» у розподілі), `Outliers` (скільки вимірів відкинуто).

## Типові помилки

- **Debug-збірка.** `JitOptimizationsValidator` зупиняє запуск: збирати тільки `-c Release`.
- **Запуск під дебагером** (F5 у Visual Studio) — JIT вимикає оптимізації; запускати з терміналу або Ctrl+F5.
- **Нестабільне середовище**: ноутбук від батареї, режим енергозбереження, браузер і IDE у фоні, антивірус,
  що сканує згенеровані exe. Див. `scripts/prepare-windows.ps1` і блок `Environment` у виводі.
- **Результат не використовується** — JIT викидає код (див. `CommonPitfallsBenchmarks`).
- **Підготовка даних усередині виміряного методу** — замість `[GlobalSetup]` (див. 06_Setup).
- **Залежні від `[Params]` дані в конструкторі** — параметри ще не присвоєні (див. `ConstructorVsGlobalSetupBenchmarks`).
- **`--job short` для класів із власними job-ами** — CLI-job додається, а не замінює їх.
- **Висновки за короткого запуску.** `--job short` — для швидкої перевірки; для цифр, які показують іншим, — повний запуск.

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

### 02_Parameters

**ParamsCartesianBenchmarks** — 12 комбінацій трьох `[Params]`. `InvariantCulture` на довгому рядку,
що відрізняється регістром, у ~2000 разів дорожчий за `Ordinal`:

| Method       | Comparison        | SameCase | Length | Mean         | Error       | StdDev     |
|------------- |------------------ |--------- |------- |-------------:|------------:|-----------:|
| StringEquals | InvariantCulture  | False    | long   | 4,044.148 ns | 272.5747 ns | 14.9407 ns |
| StringEquals | InvariantCulture  | False    | short  |   111.615 ns |   2.3363 ns |  0.1281 ns |
| StringEquals | InvariantCulture  | True     | long   |   453.803 ns |  19.1528 ns |  1.0498 ns |
| StringEquals | InvariantCulture  | True     | short  |    22.664 ns |   0.3840 ns |  0.0210 ns |
| StringEquals | Ordinal           | False    | long   |     2.099 ns |   0.0627 ns |  0.0034 ns |
| StringEquals | Ordinal           | False    | short  |     2.093 ns |   0.2353 ns |  0.0129 ns |
| StringEquals | Ordinal           | True     | long   |    27.091 ns |   0.2166 ns |  0.0119 ns |
| StringEquals | Ordinal           | True     | short  |     2.411 ns |   0.3008 ns |  0.0165 ns |
| StringEquals | OrdinalIgnoreCase | False    | long   |    87.132 ns |  18.9864 ns |  1.0407 ns |
| StringEquals | OrdinalIgnoreCase | False    | short  |     4.621 ns |   1.3672 ns |  0.0749 ns |
| StringEquals | OrdinalIgnoreCase | True     | long   |    54.817 ns |  19.4840 ns |  1.0680 ns |
| StringEquals | OrdinalIgnoreCase | True     | short  |     3.698 ns |   0.0605 ns |  0.0033 ns |

**ParamsSourceBenchmarks** — `Array.Sort` на різних «формах» даних (колонка Shape — це `ToString()` об'єкта):

| Method    | Shape           | Mean      | Error     | StdDev   |
|---------- |---------------- |----------:|----------:|---------:|
| ArraySort | FewUnique-10000 |  61.03 us | 17.651 us | 0.968 us |
| ArraySort | Random-10000    | 320.27 us | 25.540 us | 1.400 us |
| ArraySort | Sawtooth-10000  | 137.42 us |  7.539 us | 0.413 us |
| ArraySort | Sorted-10000    |  38.45 us |  5.173 us | 0.284 us |

**ParamsAllValuesBenchmarks** — усі значення enum без ручного переліку. Розгалуження виграє на
передбачуваних даних і програє на випадкових; branchless-варіант стабільний:

| Method     | Pattern     | Mean      | Error     | StdDev    | Ratio | RatioSD |
|----------- |------------ |----------:|----------:|----------:|------:|--------:|
| Branchy    | Random      | 20.656 us | 1.1821 us | 0.0648 us |  1.00 |    0.00 |
| Branchless | Random      | 14.836 us | 0.2793 us | 0.0153 us |  0.72 |    0.00 |
|            |             |           |           |           |       |         |
| Branchy    | Sorted      | 11.570 us | 7.9172 us | 0.4340 us |  1.00 |    0.05 |
| Branchless | Sorted      | 14.862 us | 0.5143 us | 0.0282 us |  1.29 |    0.04 |
|            |             |           |           |           |       |         |
| Branchy    | Alternating | 11.262 us | 0.4601 us | 0.0252 us |  1.00 |    0.00 |
| Branchless | Alternating | 14.849 us | 0.2774 us | 0.0152 us |  1.32 |    0.00 |
|            |             |           |           |           |       |         |
| Branchy    | AllSame     |  8.768 us | 0.3168 us | 0.0174 us |  1.00 |    0.00 |
| Branchless | AllSame     | 14.870 us | 0.6975 us | 0.0382 us |  1.70 |    0.00 |

**ArgumentsBenchmarks** — парне число відсіюється одразу, тому `ZeroMeasurement`:

| Method  | number  | Mean        | Error      | StdDev    |
|-------- |-------- |------------:|-----------:|----------:|
| IsPrime | 7919    |  56.5047 ns |  0.6276 ns | 0.0344 ns |
| IsPrime | 104729  | 216.1415 ns |  8.3298 ns | 0.4566 ns |
| IsPrime | 1000000 |   0.0000 ns |  0.0000 ns | 0.0000 ns |
| IsPrime | 1000003 | 673.4804 ns | 13.8972 ns | 0.7618 ns |

**ArgumentsSourceBenchmarks** — пошук підрядка, ordinal vs культурний (~30× на довгому тексті):

| Method                  | searchCase | Mean         | Error       | StdDev     | Ratio | RatioSD |
|------------------------ |----------- |-------------:|------------:|-----------:|------:|--------:|
| IndexOfOrdinal          | AtEnd      |   297.972 ns |  97.6923 ns |  5.3548 ns |  1.00 |    0.02 |
| IndexOfInvariantCulture | AtEnd      | 9,117.468 ns | 536.3960 ns | 29.4017 ns | 30.60 |    0.48 |
|                         |            |              |             |            |       |         |
| IndexOfOrdinal          | AtStart    |     4.231 ns |   0.1306 ns |  0.0072 ns |  1.00 |    0.00 |
| IndexOfInvariantCulture | AtStart    |    14.388 ns |   1.0297 ns |  0.0564 ns |  3.40 |    0.01 |
|                         |            |              |             |            |       |         |
| IndexOfOrdinal          | InMiddle   |   161.235 ns |  79.3205 ns |  4.3478 ns |  1.00 |    0.03 |
| IndexOfInvariantCulture | InMiddle   | 4,610.623 ns | 296.6716 ns | 16.2616 ns | 28.61 |    0.68 |
|                         |            |              |             |            |       |         |
| IndexOfOrdinal          | Missing    |   300.195 ns |  58.9114 ns |  3.2291 ns |  1.00 |    0.01 |
| IndexOfInvariantCulture | Missing    | 9,077.551 ns |  98.4889 ns |  5.3985 ns | 30.24 |    0.28 |

**CollectionsBySizeBenchmarks** — `List<T>` стабільно ~2× повільніший за масив, `Span<T>` ≈ масив:

| Method   | Size   | Mean          | Error         | StdDev      | Ratio | RatioSD |
|--------- |------- |--------------:|--------------:|------------:|------:|--------:|
| SumArray | 10     |      2.473 ns |     0.1324 ns |   0.0073 ns |  1.00 |    0.00 |
| SumList  | 10     |      4.974 ns |     0.9983 ns |   0.0547 ns |  2.01 |    0.02 |
| SumSpan  | 10     |      2.487 ns |     0.0792 ns |   0.0043 ns |  1.01 |    0.00 |
|          |        |               |               |             |       |         |
| SumArray | 1000   |    235.468 ns |    88.1310 ns |   4.8308 ns |  1.00 |    0.02 |
| SumList  | 1000   |    459.814 ns |    14.6857 ns |   0.8050 ns |  1.95 |    0.03 |
| SumSpan  | 1000   |    288.086 ns | 1,258.0679 ns |  68.9589 ns |  1.22 |    0.25 |
|          |        |               |               |             |       |         |
| SumArray | 100000 | 22,712.191 ns |   668.0544 ns |  36.6183 ns |  1.00 |    0.00 |
| SumList  | 100000 | 45,396.820 ns | 1,459.5633 ns |  80.0036 ns |  2.00 |    0.00 |
| SumSpan  | 100000 | 22,881.637 ns | 5,591.3734 ns | 306.4820 ns |  1.01 |    0.01 |

### 03_Memory

**StringBuildingBenchmarks** — `+=` на 100 елементах алокує 33 КБ проти 600 B у `string.Create`/`stackalloc`.
Перша версія `string.Create` рахувала довжину через `Math.Log10` і була повільнішою за `StringBuilder`
(727 ns на Count=100); після заміни на цілочисельний підрахунок цифр — 362 ns. Вимірюйте і допоміжний код теж.

| Method         | Count | Mean        | Error       | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------- |------ |------------:|------------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| Concatenation  | 10    |   109.01 ns |    26.97 ns |  1.478 ns |  1.00 |    0.02 | 0.0421 |      - |     704 B |        1.00 |
| StringBuilder  | 10    |    59.67 ns |    34.11 ns |  1.870 ns |  0.55 |    0.02 | 0.0162 |      - |     272 B |        0.39 |
| StringCreate   | 10    |    30.07 ns |    12.21 ns |  0.669 ns |  0.28 |    0.01 | 0.0038 |      - |      64 B |        0.09 |
| StackallocSpan | 10    |    48.33 ns |    10.43 ns |  0.572 ns |  0.44 |    0.01 | 0.0038 |      - |      64 B |        0.09 |
|                |       |             |             |           |       |         |        |        |           |             |
| Concatenation  | 100   | 2,022.07 ns | 1,083.17 ns | 59.372 ns |  1.00 |    0.04 | 2.0218 |      - |   33824 B |        1.00 |
| StringBuilder  | 100   |   413.47 ns |    48.00 ns |  2.631 ns |  0.20 |    0.01 | 0.1225 | 0.0005 |    2056 B |        0.06 |
| StringCreate   | 100   |   361.75 ns |    34.48 ns |  1.890 ns |  0.18 |    0.00 | 0.0358 |      - |     600 B |        0.02 |
| StackallocSpan | 100   |   299.37 ns |    69.12 ns |  3.789 ns |  0.15 |    0.00 | 0.0358 |      - |     600 B |        0.02 |

**ClassVsStructBenchmarks** — масив структур у 4 рази менший і в 3 рази швидший; `record struct` = `struct`:

| Method       | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------- |---------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| Class        | 3.308 us | 1.7231 us | 0.0944 us |  1.00 |    0.04 | 1.9150 | 0.2098 |  31.27 KB |        1.00 |
| Struct       | 1.035 us | 0.2569 us | 0.0141 us |  0.31 |    0.01 | 0.4787 |      - |   7.84 KB |        0.25 |
| RecordStruct | 1.074 us | 0.2136 us | 0.0117 us |  0.32 |    0.01 | 0.4787 |      - |   7.84 KB |        0.25 |

**LinqVsLoopBenchmarks** — ланцюжок LINQ алокує лише 104 B ітераторів, але `ToList()` посередині — 20 КБ:

| Method         | Mean      | Error     | StdDev    | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------- |----------:|----------:|----------:|------:|-------:|-------:|----------:|------------:|
| ForLoop        |  3.648 us | 0.1076 us | 0.0059 us |  1.00 |      - |      - |         - |          NA |
| LinqChain      | 10.316 us | 0.4940 us | 0.0271 us |  2.83 |      - |      - |     104 B |          NA |
| LinqWithToList | 14.316 us | 0.8284 us | 0.0454 us |  3.92 | 1.1902 | 0.0763 |   20176 B |          NA |

**ArrayPoolBenchmarks** — масив 128 КБ потрапляє в Large Object Heap: однакові Gen0 = Gen1 = Gen2 означають,
що кожна збірка — повна. Пул прибирає алокації повністю і прискорює в 23 рази:

| Method       | Size   | Mean         | Error        | StdDev     | Ratio | RatioSD | Gen0    | Gen1    | Gen2    | Allocated | Alloc Ratio |
|------------- |------- |-------------:|-------------:|-----------:|------:|--------:|--------:|--------:|--------:|----------:|------------:|
| NewArray     | 1024   |     28.75 ns |    14.972 ns |   0.821 ns |  1.00 |    0.03 |  0.0626 |       - |       - |    1048 B |        1.00 |
| RentFromPool | 1024   |     14.46 ns |     0.395 ns |   0.022 ns |  0.50 |    0.01 |       - |       - |       - |         - |        0.00 |
|              |        |              |              |            |       |         |         |         |         |           |             |
| NewArray     | 131072 | 30,593.15 ns | 3,478.870 ns | 190.689 ns |  1.00 |    0.01 | 41.6260 | 41.6260 | 41.6260 |  131110 B |        1.00 |
| RentFromPool | 131072 |  1,328.47 ns |   147.787 ns |   8.101 ns |  0.04 |    0.00 |       - |       - |       - |         - |        0.00 |

**ThreadingDiagnoserBenchmarks** — 100 000 інкрементів із `Parallel.For`; локальні суми в 185 разів швидші за `lock`:

| Method                       | Mean        | Error     | StdDev    | Ratio | Completed Work Items | Lock Contentions |
|----------------------------- |------------:|----------:|----------:|------:|---------------------:|-----------------:|
| SharedCounterWithLock        | 4,988.76 us | 221.07 us | 12.118 us | 1.000 |              18.4766 |          31.0156 |
| SharedCounterWithInterlocked | 1,898.04 us | 114.54 us |  6.279 us | 0.380 |              12.6191 |                - |
| ThreadLocalSums              |    27.03 us |  12.05 us |  0.661 us | 0.005 |              11.9592 |           0.0001 |

**ExceptionCostBenchmarks** — один виняток коштує ~1.8 µs і 480 B:

| Method         | Input | Mean         | Error      | StdDev    | Ratio  | RatioSD | Gen0   | Exceptions | Allocated | Alloc Ratio |
|--------------- |------ |-------------:|-----------:|----------:|-------:|--------:|-------:|-----------:|----------:|------------:|
| TryParse       | 12345 |     6.995 ns |  0.5703 ns | 0.0313 ns |   1.00 |    0.01 |      - |          - |         - |          NA |
| ParseWithCatch | 12345 |     7.424 ns |  0.3543 ns | 0.0194 ns |   1.06 |    0.00 |      - |          - |         - |          NA |
|                |       |              |            |           |        |         |        |            |           |             |
| TryParse       | 12x45 |     8.164 ns |  0.9748 ns | 0.0534 ns |   1.00 |    0.01 |      - |          - |         - |          NA |
| ParseWithCatch | 12x45 | 1,771.886 ns | 77.0907 ns | 4.2256 ns | 217.04 |    1.31 | 0.0286 |     1.0000 |     480 B |          NA |

**NativeMemoryBenchmarks** — потребує адміністратора. Без нього валідатор зупиняє запуск:
`Must be elevated (Admin) to use ETW Kernel Session (required for Hardware Counters and EtwProfiler).`
Результати — після запуску з терміналу адміністратора (див. розділ «Бенчмарки для адміністратора»).

### 04_Runtimes

Класи цього розділу задають job-и самі, тому запускаються **без** `--job short` (CLI-job додався б четвертим):
`dotnet run -c Release -f net10.0 -- --filter "*Runtimes*"`. Повний розділ — ~9 хвилин, з них 1–2 — збірка NativeAOT.

**RuntimeComparisonBenchmarks** — словник на .NET 10 на 41% швидший за .NET 8; LINQ найшвидший на .NET 9.
NativeAOT програє JIT (немає Dynamic PGO), а на LINQ — у 7 разів:

| Method             | Runtime        | Mean       | Ratio | Allocated | Alloc Ratio |
|------------------- |--------------- |-----------:|------:|----------:|------------:|
| LinqWhereSelectSum | .NET 10.0      |  11.551 us |  0.79 |     104 B |        1.00 |
| LinqWhereSelectSum | .NET 8.0       |  14.690 us |  1.00 |     104 B |        1.00 |
| LinqWhereSelectSum | .NET 9.0       |  10.402 us |  0.71 |     104 B |        1.00 |
| LinqWhereSelectSum | NativeAOT 10.0 | 104.807 us |  7.14 |     144 B |        1.38 |
|                    |                |            |       |           |             |
| DictionaryLookups  | .NET 10.0      |   4.981 us |  0.59 |         - |          NA |
| DictionaryLookups  | .NET 8.0       |   8.407 us |  1.00 |         - |          NA |
| DictionaryLookups  | .NET 9.0       |   8.209 us |  0.98 |         - |          NA |
| DictionaryLookups  | NativeAOT 10.0 |  10.486 us |  1.25 |         - |          NA |

> **NativeAOT і BenchmarkDotNet 0.15.8.** Якщо проєкт бенчмарків має кілька `TargetFrameworks`, NativeAOT-toolchain
> мовчки публікує звичайний JIT-застосунок і падає з `Executable ... not found` (результат `NA`).
> Обхід — job з `.WithMsBuildArguments("/p:TargetFramework=net10.0")` (див. `NativeAotConfig` у класі).
> Також NativeAOT потребує C++ workload у Visual Studio і не любить дуже довгі шляхи (MSB3030).

**JitSettingsBenchmarks** — виклик через інтерфейс: без Dynamic PGO (`TieredPGO=0`) або без tiered compilation
JIT не девіртуалізує виклик — у 2.4–2.7 раза повільніше. ReadyToRun на стабільну швидкість не впливає:

| Method               | Job          | EnvironmentVariables       | Mean       | Ratio |
|--------------------- |------------- |--------------------------- |-----------:|------:|
| SumAreasViaInterface | Default      | Empty                      |   742.2 ns |  1.00 |
| SumAreasViaInterface | NoPGO        | DOTNET_TieredPGO=0         | 2,030.4 ns |  2.74 |
| SumAreasViaInterface | NoReadyToRun | DOTNET_ReadyToRun=0        |   687.6 ns |  0.93 |
| SumAreasViaInterface | NoTiering    | DOTNET_TieredCompilation=0 | 1,777.7 ns |  2.40 |

**GcModeBenchmarks** — 16.8 МБ паралельних алокацій. Server GC робить значно менше збірок (Gen0 598 проти 1057,
Gen1 87 проти 776 на 1000 операцій) і трохи швидший. Перша версія цього бенчмарка показувала 4 КБ замість 16.8 МБ:
.NET 10 розмістив `new byte[64]` у стеку (escape analysis), тож довелося «втримувати» масиви — див. 10_DotNet10Features:

| Method              | Job                      | Concurrent | Server | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0      | Gen1     | Gen2   | Allocated | Alloc Ratio |
|-------------------- |------------------------- |----------- |------- |---------:|---------:|---------:|------:|--------:|----------:|---------:|-------:|----------:|------------:|
| ParallelAllocations | Server                   | True       | True   | 801.0 us | 18.44 us | 53.80 us |  0.94 |    0.06 |  597.6563 |  86.9141 |      - |  16.81 MB |        1.00 |
| ParallelAllocations | Workstation              | True       | False  | 853.4 us | 10.30 us |  9.63 us |  1.00 |    0.02 | 1056.6406 | 776.3672 | 1.9531 |  16.81 MB |        1.00 |
| ParallelAllocations | WorkstationNonConcurrent | False      | False  | 824.4 us |  8.03 us |  7.12 us |  0.97 |    0.01 | 1056.6406 | 794.9219 | 1.9531 |  16.81 MB |        1.00 |

**RunStrategyBenchmarks** — серіалізація System.Text.Json: стабільний виклик 358 ns, а холодний (новий процес) — ~38 ms,
у 100 000 разів більше. У Monitoring видно те саме: медіана 20.6 µs, але Max 35 ms — перша ітерація:

| Method          | Job        | IterationCount | LaunchCount | RunStrategy | UnrollFactor | Mean            | Error            | StdDev          | Median          | Min             | Max             |
|---------------- |----------- |--------------- |------------ |------------ |------------- |----------------:|-----------------:|----------------:|----------------:|----------------:|----------------:|
| SerializeToJson | ColdStart  | 1              | 5           | ColdStart   | 1            | 38,196,580.0 ns | 14,256,109.79 ns | 3,702,266.45 ns | 39,047,300.0 ns | 32,018,100.0 ns | 41,537,300.0 ns |
| SerializeToJson | Monitoring | 20             | Default     | Monitoring  | 1            |  1,769,630.0 ns |  6,786,816.37 ns | 7,815,707.82 ns |     20,650.0 ns |      8,800.0 ns | 34,974,800.0 ns |
| SerializeToJson | Throughput | Default        | Default     | Throughput  | 16           |        358.3 ns |          3.10 ns |         2.59 ns |        357.7 ns |        355.0 ns |        365.1 ns |

**JobCharacteristicsBenchmarks** — той самий код: «Quick» (3 ітерації) дає Error 28 µs при Mean 9.5 µs,
«Precise» (2 процеси × 30 ітерацій) — 0.14 µs:

| Method   | Job     | InvocationCount | IterationCount | LaunchCount | UnrollFactor | WarmupCount | Mean     | Error      | StdDev    | Ratio | RatioSD |
|--------- |-------- |---------------- |--------------- |------------ |------------- |------------ |---------:|-----------:|----------:|------:|--------:|
| SortCopy | Precise | 16000           | 30             | 2           | 4            | 10          | 8.030 us |  0.1413 us | 0.2980 us |  0.85 |    0.11 |
| SortCopy | Quick   | 1000            | 3              | 1           | 1            | 1           | 9.547 us | 28.0563 us | 1.5379 us |  1.02 |    0.19 |

### 05_Diagnosers

`[DotTraceDiagnoser]` / `[DotMemoryDiagnoser]` не додано: за планом — лише якщо встановлено JetBrains-інструменти (їх немає).
Класи для адміністратора описано в розділі «Бенчмарки для адміністратора» — їхні результати тут з'являться після запуску.

**BoundsCheckDisassemblyBenchmarks** — колонка Code Size додається `[DisassemblyDiagnoser]` автоматично:

| Method                    | Mean     | Error     | StdDev    | Ratio | Code Size |
|-------------------------- |---------:|----------:|----------:|------:|----------:|
| LocalArrayToLength        | 2.584 us | 0.2491 us | 0.0137 us |  1.00 |      30 B |
| LocalArrayToSeparateCount | 2.607 us | 0.4037 us | 0.0221 us |  1.01 |      88 B |
| FieldArray                | 4.582 us | 0.1498 us | 0.0082 us |  1.77 |      64 B |

Що видно в згенерованому `*-asm.md` (.NET 10.0.12):
- `LocalArrayToLength` — у циклі лише `add ecx,[rax]` / `add rax,4`, жодної перевірки меж;
- `LocalArrayToSeparateCount` — **loop cloning**: перед циклом одне порівняння `cmp r10d,ecx` (довжина vs count),
  далі та сама «швидка» копія циклу; повільна копія з `CORINFO_HELP_RNGCHKFAIL` — про запас. Тому швидкість однакова,
  а коду втричі більше;
- `FieldArray` — у кожній ітерації `cmp edx,[r8+8]` / `jae` (перевірка меж) і `cmp [rcx+8],edx` (довжина перечитується
  з поля) — у 1.77 раза повільніше. Це пояснює, чому `for` по полю програв `foreach` у 01_Basics.

**SimdDisassemblyBenchmarks** — Ryzen 5 5600: `Vector<float>.Count = 8, Vector256.IsHardwareAccelerated = True,
Vector512.IsHardwareAccelerated = False` (AVX2 без AVX-512). Векторні версії у 8 разів швидші — ширина вектора 8 float:

| Method            | Mean       | Error     | StdDev   | Ratio | Code Size |
|------------------ |-----------:|----------:|---------:|------:|----------:|
| Scalar            | 2,759.2 ns | 220.75 ns | 12.10 ns |  1.00 |      32 B |
| VectorT           |   350.8 ns |   7.55 ns |  0.41 ns |  0.13 |     195 B |
| Vector256Explicit |   328.7 ns |  54.46 ns |  2.99 ns |  0.12 |     137 B |

**EventPipeProfilerBenchmarks** — таблиця звичайна, головне — файл
`BenchmarkDotNet.Artifacts/Showcase.Benchmarks.Diagnosers.EventPipeProfilerBenchmarks.CountPrimesThenSort-<дата>.speedscope.json`,
який відкривається на https://www.speedscope.app як flame graph:

| Method              | Mean     | Error     | StdDev    |
|-------------------- |---------:|----------:|----------:|
| CountPrimesThenSort | 2.255 ms | 0.1458 ms | 0.0080 ms |

### Config (конфігурація, колонки, експорт)

Простір імен — `Showcase.Benchmarks.Configuration`: namespace `...Config` перекрив би атрибут `[Config]` в усьому проєкті.
`JobAttributes`, `FluentConfig` і `Outliers` мають власні job-и і запускаються без `--job short`.

**CustomColumnsBenchmarks** — статистичні колонки й власна колонка «Budget 1,000 ns». Перша версія колонки
показувала `x46,1` — форматування бралося з української культури; тепер — InvariantCulture:

| Method       | Mean         | Error        | StdDev    | Median       | Min          | Max          | P95          | Budget 1,000 ns |
|------------- |-------------:|-------------:|----------:|-------------:|-------------:|-------------:|-------------:|---------------- |
| ShortToUpper |     20.90 ns |     5.167 ns |  0.283 ns |     20.97 ns |     20.58 ns |     21.14 ns |     21.12 ns | OK              |
| LongToUpper  | 44,827.56 ns | 1,794.360 ns | 98.355 ns | 44,829.55 ns | 44,728.22 ns | 44,924.90 ns | 44,915.37 ns | x44.8           |

**FluentConfigBenchmarks** — ShortRun + MemoryDiagnoser + Rank + сортування, зібрані fluent-API. Rank 1 в обох
перших рядках — різниця між `ToList` і `ToArray` статистично не доведена:

| Method        | Mean       | Error    | StdDev   | Rank | Gen0   | Allocated |
|-------------- |-----------:|---------:|---------:|-----:|-------:|----------:|
| ToListCopy    |   123.8 ns | 104.8 ns |  5.74 ns |    1 | 0.2422 |   3.96 KB |
| ToArrayCopy   |   134.0 ns | 245.4 ns | 13.45 ns |    1 | 0.2404 |   3.93 KB |
| ToHashSetCopy | 5,233.3 ns | 731.6 ns | 40.10 ns |    2 | 1.0605 |  17.35 KB |

**JobAttributesBenchmarks** — `[DryJob]` показує один холодний виклик (з JIT) — у 160 разів більше за ShortRun:

| Method           | Job      | IterationCount | RunStrategy | UnrollFactor | WarmupCount | Mean       | Error    | StdDev    |
|----------------- |--------- |--------------- |------------ |------------- |------------ |-----------:|---------:|----------:|
| SumOfSquareRoots | Dry      | 1              | ColdStart   | 1            | 1           | 754.800 us |       NA | 0.0000 us |
| SumOfSquareRoots | ShortRun | 3              | Default     | 16           | 3           |   4.753 us | 1.004 us | 0.0550 us |

**InProcessBenchmarks** — з `--job short` атрибут `[InProcess]` додає власний job, тож рядків два; результати збігаються:

| Method       | Job       | Toolchain              | IterationCount | LaunchCount | WarmupCount | Mean     | Error     | StdDev  |
|------------- |---------- |----------------------- |--------------- |------------ |------------ |---------:|----------:|--------:|
| ArrayIndexOf | InProcess | InProcessEmitToolchain | Default        | Default     | Default     | 476.0 ns |   5.25 ns | 4.91 ns |
| ArrayIndexOf | ShortRun  | Default                | 3              | 1           | 3           | 478.1 ns | 122.23 ns | 6.70 ns |

**ExportersBenchmarks** — у `BenchmarkDotNet.Artifacts/results` з'явилися `-report-github.md`, `-report.html`, `-report.csv`,
`-measurements.csv`, `-report-full.json`, `-report-brief.xml`, `-report.asciidoc`. R не встановлено, тому RPlot лише попередив:
`RPlotExporter couldn't find Rscript.exe in your PATH and no R_HOME environment variable is defined`.

| Method               | Mean      | Error    | StdDev   | Ratio | RatioSD |
|--------------------- |----------:|---------:|---------:|------:|--------:|
| SortOrdinal          |  78.27 ns | 33.37 ns | 1.829 ns |  1.00 |    0.03 |
| SortInvariantCulture | 217.35 ns | 30.66 ns | 1.680 ns |  2.78 |    0.06 |

**CustomLoggerBenchmarks** — логер пише `BenchmarkDotNet.Artifacts/elapsed-time.log`; логери діють на весь запуск, тому
у файл потрапив вивід усіх 13 бенчмарків групи. Фрагмент:

```
[+00:00.000] // Validating benchmarks:
[+00:00.539] // ***** BenchmarkRunner: Start   *****
[+00:00.539] // ***** Found 13 benchmark(s) in total *****
[+00:19.820] WorkloadActual   1: 33554432 op, 813281600.00 ns, 24.2377 ns/op
```

Перша версія логера відкривала файл у конструкторі й ламала **інші** запуски: `BenchmarkSwitcher` створює конфігурації
всіх класів під час пошуку бенчмарків, навіть відфільтрованих. Тепер файл відкривається при першому записі.

**ValidatorsBenchmarks** — усі три реалізації повертають 4950, тож `ReturnValueValidator` пропускає запуск.
`SumWithFormula` — `ZeroMeasurement` (кілька інструкцій):

| Method         | Mean       | Error     | StdDev    | Ratio |
|--------------- |-----------:|----------:|----------:|------:|
| SumWithLoop    | 31.7480 ns | 4.3353 ns | 0.2376 ns | 1.000 |
| SumWithLinq    |  7.1575 ns | 2.0832 ns | 0.1142 ns | 0.225 |
| SumWithFormula |  0.0123 ns | 0.1867 ns | 0.0102 ns | 0.000 |

**OutliersBenchmarks** — метод іноді «зависає» на ~600 µs. `DontRemove` враховує все (Mean 17 µs, StdDev 79 µs),
`RemoveUpper` відкидає 8 викидів (Mean 4.3 µs, StdDev 0.35 µs). Медіани однакові — вона стійка до викидів:

| Method                  | Job         | OutlierMode | Mean      | Error      | StdDev     | Median   |
|------------------------ |------------ |------------ |----------:|-----------:|-----------:|---------:|
| WorkWithOccasionalSpike | DontRemove  | DontRemove  | 17.275 us | 35.4185 us | 79.2185 us | 4.375 us |
| WorkWithOccasionalSpike | RemoveUpper | RemoveUpper |  4.298 us |  0.1688 us |  0.3487 us | 4.431 us |

```
Outliers
  OutliersBenchmarks.WorkWithOccasionalSpike: RemoveUpper -> 8 outliers were removed, 18 outliers were detected (3.54 us..3.82 us, 4.74 us..633.47 us)
```

**StatisticalTestBenchmarks** — тест Манна–Вітні з порогом 5%. Несподіванка: `Aggregate` з делегатом — **Same**
(Dynamic PGO девіртуалізує делегат), а векторизований `Sum` — **Faster**:

| Method        | Mean      | Error     | StdDev   | Ratio | MannWhitney(5%) |
|-------------- |----------:|----------:|---------:|------:|---------------- |
| ForLoop       | 260.17 ns | 12.125 ns | 0.665 ns |  1.00 | Baseline        |
| ForeachLoop   | 260.56 ns | 31.395 ns | 1.721 ns |  1.00 | Same            |
| LinqAggregate | 258.05 ns | 28.867 ns | 1.582 ns |  0.99 | Same            |
| LinqSum       |  68.41 ns |  5.912 ns | 0.324 ns |  0.26 | Faster          |

### 06_Setup

**ConstructorVsGlobalSetupBenchmarks** — у лозі видно порядок викликів: `// Constructor: Size = 0`, потім
`// GlobalSetup: Size = 1000`. Масив з конструктора порожній — звідси `ZeroMeasurement`:

| Method                 | Size   | Mean           | Error         | StdDev     |
|----------------------- |------- |---------------:|--------------:|-----------:|
| SumDataFromConstructor | 1000   |      0.2278 ns |     0.1878 ns |  0.0103 ns |
| SumDataFromGlobalSetup | 1000   |    233.5918 ns |     4.2789 ns |  0.2345 ns |
| SumDataFromConstructor | 100000 |      0.2443 ns |     0.2637 ns |  0.0145 ns |
| SumDataFromGlobalSetup | 100000 | 22,883.9915 ns | 1,475.6029 ns | 80.8828 ns |

**FileSerializationSetupBenchmarks** — файл створюється в `[GlobalSetup]` і видаляється в `[GlobalCleanup]`;
читання з `FileStream` додає 70%:

| Method               | Mean     | Error     | StdDev   | Ratio | RatioSD | Gen0    | Gen1    | Allocated | Alloc Ratio |
|--------------------- |---------:|----------:|---------:|------:|--------:|--------:|--------:|----------:|------------:|
| DeserializeFromBytes | 431.2 us |  34.71 us |  1.90 us |  1.00 |    0.01 | 25.8789 | 11.7188 | 422.84 KB |        1.00 |
| DeserializeFromFile  | 730.7 us | 185.54 us | 10.17 us |  1.69 |    0.02 | 25.3906 | 10.7422 | 423.08 KB |        1.00 |

**IterationSetupBenchmarks** — сортування 2 млн елементів на місці. З 1 млн ітерація тривала ~50 мс і BenchmarkDotNet
попереджав `MinIterationTime`; з 2 млн (~105 мс) попередження зникло. Час однаковий, різниця — в Allocated:

| Method                 | Mean     | Error    | StdDev  | Ratio | RatioSD | Allocated | Alloc Ratio |
|----------------------- |---------:|---------:|--------:|------:|--------:|----------:|------------:|
| SortWithIterationSetup | 105.2 ms | 39.59 ms | 2.17 ms |  1.00 |    0.03 |         - |          NA |
| CloneInsideMethod      | 107.4 ms | 51.94 ms | 2.85 ms |  1.02 |    0.03 | 8000024 B |          NA |

**TargetedSetupBenchmarks** — у лозі перед `DictionaryLookup` і `FrozenDictionaryLookup` виконується лише
`SetupHashBased`, а перед `SortedDictionaryLookup` — лише `SetupSorted`:

| Method                 | Mean       | Error      | StdDev    | Ratio | RatioSD |
|----------------------- |-----------:|-----------:|----------:|------:|--------:|
| DictionaryLookup       |   4.998 us |  0.8970 us | 0.0492 us |  1.00 |    0.01 |
| FrozenDictionaryLookup |   5.308 us |  0.5759 us | 0.0316 us |  1.06 |    0.01 |
| SortedDictionaryLookup | 224.279 us | 34.3414 us | 1.8824 us | 44.88 |    0.50 |

### 07_Async

**TaskVsValueTaskBenchmarks** — `Task<int>` з довільним результатом алокує 72 B на виклик; `ValueTask<int>` і `Task` — 0 B:

| Method            | Mean      | Error      | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------ |----------:|-----------:|----------:|------:|--------:|-------:|----------:|------------:|
| TaskOfT           | 11.371 ns | 15.7601 ns | 0.8639 ns |  1.00 |    0.09 | 0.0043 |      72 B |        1.00 |
| ValueTaskOfT      |  7.886 ns |  2.1672 ns | 0.1188 ns |  0.70 |    0.04 |      - |         - |        0.00 |
| TaskWithoutResult |  7.255 ns |  0.9548 ns | 0.0523 ns |  0.64 |    0.04 |      - |         - |        0.00 |

**AsyncOverheadBenchmarks** — пряме повернення `Task` економить половину алокацій (немає state machine обгортки);
`ConfigureAwait(false)` без SynchronizationContext нічого не дає:

| Method                        | Mean     | Error    | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------------ |---------:|---------:|--------:|------:|-------:|----------:|------------:|
| AsyncAwaitWrapper             | 606.3 ns | 95.87 ns | 5.26 ns |  1.00 | 0.0105 |     191 B |        1.00 |
| DirectTaskReturn              | 582.9 ns | 44.35 ns | 2.43 ns |  0.96 | 0.0057 |      96 B |        0.50 |
| AsyncAwaitConfigureAwaitFalse | 596.6 ns | 98.61 ns | 5.41 ns |  0.98 | 0.0114 |     199 B |        1.04 |

**ParallelismBenchmarks** — 6 ядер / 12 потоків: усі паралельні варіанти в 4.4–5.6 раза швидші. `Task.WhenAll` — рівно
64 Completed Work Items (64 шматки), `Parallel.For` і PLINQ — ~11–12 (планувальник групує роботу):

| Method      | Mean      | Error     | StdDev    | Ratio | Completed Work Items | Lock Contentions | Allocated | Alloc Ratio |
|------------ |----------:|----------:|----------:|------:|---------------------:|-----------------:|----------:|------------:|
| Sequential  | 10.072 ms | 1.7663 ms | 0.0968 ms |  1.00 |                    - |                - |         - |          NA |
| ParallelFor |  2.269 ms | 0.1338 ms | 0.0073 ms |  0.23 |              11.9609 |           0.0039 |    4106 B |          NA |
| Plinq       |  1.964 ms | 0.0557 ms | 0.0031 ms |  0.20 |              11.0000 |                - |    7220 B |          NA |
| TaskWhenAll |  1.797 ms | 0.0561 ms | 0.0031 ms |  0.18 |              64.0000 |           0.0117 |   12536 B |          NA |
