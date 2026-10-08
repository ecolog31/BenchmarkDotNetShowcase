using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

namespace Showcase.Benchmarks.DotNet10;

/// <summary>
/// Спільна конфігурація розділу: той самий код на .NET 8 (baseline), .NET 9 і .NET 10,
/// [MemoryDiagnoser] і колонка Ratio. Базовий job — ShortRun, щоб увесь розділ проходив за хвилини;
/// для фінальних цифр замініть на Job.Default.
/// Запускати без --job short (job-и вже задані тут).
/// </summary>
public class DotNetVersionsConfig : ManualConfig
{
    public DotNetVersionsConfig()
    {
        AddJob(Job.ShortRun.WithRuntime(CoreRuntime.Core80).WithId(".NET 8").AsBaseline());
        AddJob(Job.ShortRun.WithRuntime(CoreRuntime.Core90).WithId(".NET 9"));
        AddJob(Job.ShortRun.WithRuntime(CoreRuntime.Core10_0).WithId(".NET 10"));
        AddDiagnoser(MemoryDiagnoser.Default);
        AddColumnProvider(DefaultColumnProviders.Instance);
        HideColumns(Column.Job, Column.Error, Column.RatioSD, Column.AllocRatio);
    }
}

/// <summary>Варіант для можливостей, яких немає в .NET 8: порівняння лише .NET 9 (baseline) і .NET 10.</summary>
public class DotNet9And10Config : ManualConfig
{
    public DotNet9And10Config()
    {
        AddJob(Job.ShortRun.WithRuntime(CoreRuntime.Core90).WithId(".NET 9").AsBaseline());
        AddJob(Job.ShortRun.WithRuntime(CoreRuntime.Core10_0).WithId(".NET 10"));
        AddDiagnoser(MemoryDiagnoser.Default);
        HideColumns(Column.Job, Column.Error, Column.RatioSD, Column.AllocRatio);
    }
}
