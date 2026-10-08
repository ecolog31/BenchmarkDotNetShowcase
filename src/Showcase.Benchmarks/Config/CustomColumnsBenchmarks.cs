using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.Configuration;

/// <summary>
/// [Config(typeof(CustomColumnsConfig))]: власна конфігурація з колонками Median, P95, Min, Max
/// і власною колонкою BudgetColumn («Budget 1,000 ns»).
/// Чого очікувати: короткий рядок — OK у колонці бюджету; довгий — «xN» (у скільки разів перевищено);
/// P95 ≥ Median ≥ Min — видно «хвіст» розподілу, який середнє приховує.
/// </summary>
[Config(typeof(CustomColumnsConfig))]
public class CustomColumnsBenchmarks
{
    private string _short = "";
    private string _long = "";

    [GlobalSetup]
    public void Setup()
    {
        _short = new string('x', 100);
        _long = new string('x', 100_000);
    }

    [Benchmark]
    public string ShortToUpper() => _short.ToUpperInvariant();

    [Benchmark]
    public string LongToUpper() => _long.ToUpperInvariant();
}
