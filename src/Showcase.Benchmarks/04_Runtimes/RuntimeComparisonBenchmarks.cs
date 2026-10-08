using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

namespace Showcase.Benchmarks.Runtimes;

/// <summary>
/// Один бенчмарк — кілька рантаймів: кожен [SimpleJob(RuntimeMoniker.X)] збирає й запускає код
/// під окремим .NET. baseline: true робить .NET 8 точкою відліку для колонки Ratio.
/// NativeAOT компілює бенчмарк заздалегідь у нативний exe (потрібен C++ workload у Visual Studio;
/// збірка триває 1–2 хвилини). Його job задано в NativeAotConfig, а не атрибутом — див. коментар там.
/// Net48 тут свідомо немає: для нього весь проєкт мав би збиратися ще й під net48,
/// а код інших розділів використовує API, яких у .NET Framework немає (Span, string.Create тощо).
/// Чого очікувати: .NET 9/10 швидші за .NET 8 на LINQ і словниках; NativeAOT без Dynamic PGO
/// повільніший за JIT у стабільному режимі. На LINQ різниця особливо велика (у наших вимірах ~7×) —
/// імовірно, тому що для NativeAOT System.Linq збирається в оптимізованому за розміром варіанті
/// без спеціалізованих ітераторів.
/// </summary>
[SimpleJob(RuntimeMoniker.Net80, baseline: true)]
[SimpleJob(RuntimeMoniker.Net90)]
[SimpleJob(RuntimeMoniker.Net10_0)]
[Config(typeof(NativeAotConfig))]
[MemoryDiagnoser]
[HideColumns("Job", "Arguments", "Error", "StdDev", "RatioSD")]
public class RuntimeComparisonBenchmarks
{
    private int[] _numbers = [];
    private Dictionary<string, int> _dictionary = [];
    private string[] _keys = [];

    [GlobalSetup]
    public void Setup()
    {
        _numbers = Enumerable.Range(0, 10_000).ToArray();
        _keys = Enumerable.Range(0, 1_000).Select(i => $"key-{i}").ToArray();
        _dictionary = _keys.ToDictionary(k => k, k => k.Length);
    }

    [Benchmark]
    public int LinqWhereSelectSum() => _numbers.Where(n => n % 3 == 0).Select(n => n * 2).Sum();

    [Benchmark]
    public int DictionaryLookups()
    {
        int sum = 0;
        foreach (string key in _keys)
            sum += _dictionary[key];
        return sum;
    }

    /// <summary>
    /// Обхід бага BenchmarkDotNet 0.15.8: NativeAOT-toolchain не запускає AOT-компіляцію, якщо проєкт
    /// з бенчмарками має кілька TargetFrameworks (у publish лишається звичайний JIT-застосунок,
    /// а BenchmarkDotNet пише «Executable ... not found»). Глобальна властивість TargetFramework=net10.0
    /// робить наш проєкт однофреймворковим саме для цієї збірки. Атрибут [SimpleJob] не вміє MSBuild-аргументи,
    /// тому job задано тут.
    /// </summary>
    private sealed class NativeAotConfig : ManualConfig
    {
        public NativeAotConfig() =>
            AddJob(Job.Default
                .WithRuntime(NativeAotRuntime.Net10_0)
                .WithMsBuildArguments("/p:TargetFramework=net10.0")
                .WithId("NativeAOT10"));
    }
}
