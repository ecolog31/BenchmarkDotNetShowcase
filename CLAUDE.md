# BenchmarkDotNet Showcase

Демонстраційний C#-проєкт, що показує можливості BenchmarkDotNet.
Повний план і структура папок: **PLAN.md** (дотримуйся його).

## Середовище
- ОС: Windows
- .NET 10 (також порівнюємо з .NET 8 і 9), C# latest, платформа x64
- BenchmarkDotNet: остання стабільна 0.15.x (перевір версію на NuGet перед додаванням)

## Правила роботи
- Бенчмарки запускати **тільки в Release** і без дебагера: `dotnet run -c Release -f net10.0 -- --filter "*<Клас>*" --job short`
- Реалізовувати **по одному розділу за раз** згідно з PLAN.md; після розділу: зібрати проєкт, прогнати короткий запуск, оновити `README.md`
- Не вигадувати результати бенчмарків і цифри в README: вставляти тільки реально отримані виводи
- Перед написанням коду для нових можливостей BenchmarkDotNet перевіряти актуальний API в документації або в пакеті

## Стиль коду
- Кожен клас бенчмарка має короткий XML-коментар: що саме він демонструє і чого очікувати
- Один клас бенчмарка = одна ідея; назви класів і методів описові
- `Nullable` увімкнено; без зайвих залежностей, крім потрібних для конкретного розділу
- Повертати результат з бенчмарк-методів (або використовувати `Blackhole`), щоб уникнути dead code elimination
- Дані для бенчмарків готувати в `[GlobalSetup]`, а не всередині виміряного методу

## Діагностики Windows
- `[HardwareCounters]`, `[EtwProfiler]`, `[NativeMemoryProfiler]` потребують запуску від **адміністратора**; залишай про це коментар у класі
- Якщо діагностики Windows недоступні, додати пакет `BenchmarkDotNet.Diagnostics.Windows`
- `NativeAot10_0` потребує C++ workload у Visual Studio; `Net48` і `Mono` опційні

## Корисні команди
- Запускати з папки `src/Showcase.Benchmarks`; `-f net10.0` обов'язковий (проєкт multi-target)
- Простори імен розділів без цифр: `Showcase.Benchmarks.Basics`, `Showcase.Benchmarks.Memory` тощо (папки лишаються `01_Basics`); для `Config/` — `Showcase.Benchmarks.Configuration` (namespace `Config` перекрив би атрибут `[Config]`), для `10_DotNet10Features` — `Showcase.Benchmarks.DotNet10`
- `--job short` з CLI ДОДАЄ job до job-ів класу, а не замінює їх: класи з власними job-ами (04_Runtimes, 10_DotNet10Features, NuGetVersions, ColdStart, JobAttributes) запускати без `--job`
- NativeAOT-job у BenchmarkDotNet 0.15.8 з multi-target проєктом потребує `.WithMsBuildArguments("/p:TargetFramework=net10.0")` (див. `RuntimeComparisonBenchmarks`)
- Список бенчмарків: `dotnet run -c Release -f net10.0 -- --list flat`
- Усі бенчмарки розділу: `dotnet run -c Release -f net10.0 -- --filter "*Memory*"`
- Результати зберігаються в `BenchmarkDotNet.Artifacts/` (не комітити; додати в `.gitignore`)

## Git
- Коміт після кожного завершеного розділу з повідомленням на кшталт `feat: 03_Memory benchmarks`
