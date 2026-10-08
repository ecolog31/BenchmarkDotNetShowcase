using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;

namespace Showcase.Benchmarks.Configuration;

/// <summary>
/// Fluent-API конфігурації: беремо DefaultConfig.Instance (стандартні логери, експортери, валідатори)
/// і дописуємо до нього: AddJob, AddDiagnoser, AddColumn, WithOrderer, WithOptions(ConfigOptions...).
/// ConfigOptions — прапорці поведінки: JoinSummary (одна таблиця на всі класи), DisableLogFile,
/// StopOnFirstError, KeepBenchmarkFiles тощо. Тут — StopOnFirstError і DisableLogFile.
/// Той самий об'єкт можна передати в BenchmarkRunner.Run&lt;T&gt;(config) або BenchmarkSwitcher.Run(args, config).
/// Чого очікувати: ShortRun-job з колонками Rank і Allocated, рядки від найшвидшого.
/// </summary>
[Config(typeof(FluentConfig))]
public class FluentConfigBenchmarks
{
    private readonly int[] _numbers = Enumerable.Range(0, 1_000).ToArray();

    [Benchmark]
    public int[] ToArrayCopy() => _numbers.ToArray();

    [Benchmark]
    public List<int> ToListCopy() => _numbers.ToList();

    [Benchmark]
    public HashSet<int> ToHashSetCopy() => _numbers.ToHashSet();

    private sealed class FluentConfig : ManualConfig
    {
        public FluentConfig()
        {
            Add(DefaultConfig.Instance
                .AddJob(Job.ShortRun)
                .AddDiagnoser(MemoryDiagnoser.Default)
                .AddColumn(RankColumn.Arabic)
                .WithOrderer(new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest))
                .WithOptions(ConfigOptions.StopOnFirstError | ConfigOptions.DisableLogFile));
        }
    }
}
