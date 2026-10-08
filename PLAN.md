# План: демонстраційний проєкт BenchmarkDotNet

Мета: C#-проєкт, що демонструє можливості BenchmarkDotNet.
Середовище: **Windows, .NET 10, C# (latest), x64**.

> Це керівний документ для Claude Code. Реалізовувати розділ за розділом, після кожного розділу:
> зібрати проєкт, прогнати короткий запуск (`--job short --filter *<Розділ>*`), оновити `README.md`.

---

## 0. Структура рішення

```
BenchmarkDotNetShowcase/
├── BenchmarkDotNetShowcase.sln
├── CLAUDE.md
├── PLAN.md
├── README.md                       # таблиця «фіча → клас → що показує» + як запускати
├── src/
│   ├── Showcase.Benchmarks/        # консольний проєкт (запуск тільки в Release)
│   │   ├── Program.cs              # BenchmarkSwitcher + інтерактивне меню
│   │   ├── Config/                 # власні конфігурації, колонки, експортери
│   │   ├── 01_Basics/
│   │   ├── 02_Parameters/
│   │   ├── 03_Memory/
│   │   ├── 04_Runtimes/
│   │   ├── 05_Diagnosers/
│   │   ├── 06_Setup/
│   │   ├── 07_Async/
│   │   ├── 08_Advanced/
│   │   ├── 09_RealWorld/
│   │   └── 10_DotNet10Features/
│   └── Showcase.Library/           # код, який бенчмаркається (версії, порівняння збірок)
├── scripts/                        # запуск, порівняння результатів, підготовка Windows
└── .github/workflows/              # CI (опційно, розділ 11)
```

## 1. Налаштування проєкту

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>
    <Nullable>enable</Nullable>
    <LangVersion>latest</LangVersion>
    <PlatformTarget>x64</PlatformTarget>
    <Optimize>true</Optimize>
    <DebugType>pdbonly</DebugType>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="BenchmarkDotNet" Version="*" />
  </ItemGroup>
</Project>
```

- Використовувати **останню стабільну 0.15.x** версію BenchmarkDotNet (потрібна підтримка `RuntimeMoniker.Net10_0` і `NativeAot10_0`). Перевірити номер на NuGet.
- Якщо діагностики Windows (ETW, HardwareCounters, Inlining) не знайдено — додати пакет `BenchmarkDotNet.Diagnostics.Windows`.
- Для порівняння рантаймів мають бути встановлені SDK/рантайми .NET 8, 9, 10.
- Запуск тільки `dotnet run -c Release` або зібраний exe, **без дебагера**.

## 2. Вимоги Windows для діагностик

| Можливість | Вимоги |
|---|---|
| `[HardwareCounters]` (branch mispredictions, cache misses) | запуск від адміністратора, не у віртуальній машині |
| `[EtwProfiler]` | адміністратор |
| `[NativeMemoryProfiler]` | адміністратор |
| `[InliningDiagnoser]`, `[TailCallDiagnoser]` | пакет `BenchmarkDotNet.Diagnostics.Windows` |
| `[DisassemblyDiagnoser]` | працює без додаткових вимог |
| `NativeAot10_0` | C++ workload у Visual Studio (Desktop development with C++) |
| `NetFramework48` | Developer Pack .NET Framework 4.8 (опційно) |
| `Mono` | окреме встановлення (можна пропустити) |

---

## 3. Розділи реалізації

### 01_Basics
- Мінімальний бенчмарк: `[Benchmark]`, `BenchmarkRunner.Run<T>()`
- `[Benchmark(Baseline = true)]` + колонки Ratio / RatioSD
- `[BenchmarkCategory]` + `[GroupBenchmarksBy]` + `[CategoriesColumn]`
- `[Benchmark(OperationsPerInvoke = N)]`
- `[Orderer]`, `SummaryOrderPolicy`, `[RankColumn]`, `[LogicalGroupColumn]`
- Типові пастки: dead code elimination, константне згортання, повернення значення з методу, `Consumer` / `Blackhole`

### 02_Parameters
- `[Params]` (enum, bool, рядки)
- `[ParamsSource]` (складні об'єкти, `IEnumerable<T>`)
- `[ParamsAllValues]`
- `[Arguments]`, `[ArgumentsSource]` на методах
- Декартів добуток параметрів
- Приклад: `List<T>` vs `T[]` vs `Span<T>` на розмірах 10 / 1 000 / 100 000

### 03_Memory
- `[MemoryDiagnoser]`: Allocated, Gen0/Gen1/Gen2
- `[ThreadingDiagnoser]`: Lock Contentions, Completed Work Items
- `[NativeMemoryProfiler]` (Windows, адміністратор)
- `[ExceptionDiagnoser]`: вартість винятків
- Порівняння: конкатенація `string` vs `StringBuilder` vs `string.Create` vs `Span<char>`; `class` vs `struct` vs `record struct`; LINQ vs цикл; `ArrayPool<T>`

### 04_Runtimes
- `[SimpleJob(RuntimeMoniker.Net80, baseline: true)]`, `Net90`, `Net10_0`, `NativeAot10_0`, `Net48` (опційно)
- `[MultiJob]` / кілька `Job` у `ManualConfig`
- Змінні середовища через `WithEnvironmentVariable`: `DOTNET_TieredPGO`, `DOTNET_TieredCompilation`, `DOTNET_ReadyToRun`
- GC: Server vs Workstation, Concurrent (`[GcServer]`, `[GcConcurrent]`)
- Platform: x64 (Arm64 тільки якщо є така машина)
- `Job.Default.AsBaseline()`
- Стратегії: `RunStrategy.Throughput`, `ColdStart`, `Monitoring`
- Налаштування: `WithWarmupCount`, `WithIterationCount`, `WithLaunchCount`, `WithInvocationCount`, `WithUnrollFactor`

### 05_Diagnosers
- `[DisassemblyDiagnoser]` (asm, IL, C#, Source; формати GitHub / Html / Intel)
- `[HardwareCounters]`: BranchMispredictions, CacheMisses
- `[EtwProfiler]`
- `[EventPipeProfiler]`
- `[InliningDiagnoser]`, `[TailCallDiagnoser]`
- `[DotTraceDiagnoser]` / `[DotMemoryDiagnoser]` (тільки якщо встановлено JetBrains)
- Приклади: branch prediction (відсортований vs невідсортований масив), інлайнінг, SIMD (`Vector<T>`), bounds-check elimination

### 06_Setup
- `[GlobalSetup]`, `[GlobalCleanup]`
- `[IterationSetup]`, `[IterationCleanup]` (з застереженням щодо точності)
- Targeted setup: `[GlobalSetup(Target = nameof(...))]`
- Конструктор vs `GlobalSetup`
- Приклад: підготовка даних, серіалізація/десеріалізація файлу

### 07_Async
- `Task`, `ValueTask`, `Task<T>`
- `async/await` vs пряме повернення `Task`, `ConfigureAwait(false)`
- `Parallel.For` vs PLINQ vs `Task.WhenAll`
- Вимірювання з `[ThreadingDiagnoser]`

### Config/ (конфігурація, колонки, експорт)
- Власний `ManualConfig` / `IConfig`, `[Config(typeof(...))]`
- Fluent-API: `DefaultConfig.Instance.AddJob(...)`, `ConfigOptions`
- `[ShortRunJob]`, `[DryJob]`, `[InProcess]`
- Колонки: власний `IColumn`, `StatisticColumn` (Median, P95, Min, Max)
- Експортери: Markdown (GitHub), HTML, CSV, JSON, XML, AsciiDoc, `[RPlotExporter]`
- Власний `ILogger`
- Валідатори: `[Validators]`, `ExecutionValidator`, `JitOptimizationsValidator`
- `[Outliers]` (RemoveUpper / DontRemove)
- `[StatisticalTestColumn]` (Mann–Whitney)

### 08_Advanced
- Порівняння версій NuGet-пакета: `[NuGetReference]` / `WithNuGet("Newtonsoft.Json", "12.0.3")` vs `13.0.3`
- Generic-класи: `[GenericTypeArguments]`
- `static`-методи як бенчмарки
- Збереження й порівняння результатів між запусками: `BenchmarkDotNet.Artifacts`, `--statisticalTest 5%`, `--join`
- CLI: `--filter`, `--list flat`, `--list tree`, `--disasm`, `--memory`, `--runtimes`, `--exporters`, `--artifacts`
- Зовнішні процеси (cold start програми)

### 09_RealWorld
- Серіалізація: `System.Text.Json` vs `Newtonsoft.Json` vs `MessagePack`
- Колекції: `Dictionary` vs `FrozenDictionary` vs `ImmutableDictionary`; `HashSet` vs `List.Contains`
- Рядки: `Split` vs `Span`; `Regex` (interpreted / compiled / `GeneratedRegex`)
- Хешування: MD5 / SHA256 / xxHash
- Логування: `LoggerMessage` source generator vs інтерполяція
- EF Core / Dapper (in-memory SQLite)
- `HttpClient`: повторне використання

### 10_DotNet10Features
Порівняння **.NET 8 → 9 → 10** з `[MemoryDiagnoser]` і колонкою `Ratio`:
- Stack allocation (escape analysis): малі масиви/об'єкти, що не «втікають» з методу; `Allocated` має падати до 0 B порівняно з .NET 8
- Девіртуалізація делегатів та інтерфейсів
- Покращений інлайнінг і PGO (через `[DisassemblyDiagnoser]`)
- Покращення циклів у JIT: loop inversion, bounds-check elimination на `Span<T>` і масивах
- SIMD: `Vector<T>`, `Vector128/256/512` (перевірити `Vector512.IsHardwareAccelerated`)
- BCL: `string.Join`, `Contains`, `SearchValues`, `FrozenDictionary`, `GeneratedRegex`
- `System.Threading.Lock` замість `lock(object)`
- `params ReadOnlySpan<T>` (C# 13): без алокацій
- Ключове слово `field` (C# 14): синтаксис без накладних витрат

> Конкретні цифри залежать від процесора й версії рантайму. Перед демонстрацією прогнати все локально й зберегти результати як запасний варіант.

---

## 4. Інтеграції (опційно)

- `BenchmarkSwitcher.FromAssembly(...).Run(args)` + інтерактивне меню
- CI: GitHub Actions (запуск + публікація артефактів / Markdown у summary)
- Порівняння результатів між комітами (regression detection) через JSON + скрипт
- Docker-запуск для відтворюваності

---

## 5. Підготовка Windows перед демо

- Схема живлення **High performance** (або Ultimate Performance); ноутбук від мережі
- Закрити браузер, месенджери, зайві IDE
- Додати папку проєкту й `BenchmarkDotNet.Artifacts` у виключення Windows Defender
- Вимкнути фонові оновлення та індексацію на час запуску
- Перевірити блок `Environment` у виводі BenchmarkDotNet (попередження про power plan / antivirus)
- Для `[HardwareCounters]`, `[EtwProfiler]`, `[NativeMemoryProfiler]` запускати термінал від адміністратора

## 6. Порядок запуску для демонстрації

1. Лайв: `[ShortRunJob]` або `--job short`, щоб не чекати хвилинами
2. Повні результати зберегти заздалегідь у `BenchmarkDotNet.Artifacts` (HTML + Markdown + JSON)
3. Для лайву обрати 3–4 виразні бенчмарки: Baseline/Ratio, `MemoryDiagnoser`, `DisassemblyDiagnoser`, порівняння .NET 8 vs 10
4. Окремі класи запускати через меню `BenchmarkSwitcher` та `--filter`

## 7. README.md (має містити)

- Таблиця «фіча → клас → що показує»
- Як читати вивід: Mean, Error, StdDev, Median, Ratio, Gen0, Allocated
- Типові помилки: Debug-збірка, запуск під дебагером, нестабільне середовище
- Приклади результатів і графіків

## 8. Порядок реалізації

1. Каркас рішення, `Program.cs` з `BenchmarkSwitcher`, порожні папки, README
2. 01_Basics → 02_Parameters → 03_Memory
3. 04_Runtimes
4. 05_Diagnosers
5. Config/ (конфігурації, колонки, експортери) + 06_Setup + 07_Async
6. 08_Advanced + 09_RealWorld
7. 10_DotNet10Features
8. CI-інтеграція, скрипти, фінальний README
