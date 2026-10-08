using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.DotNet10;

/// <summary>
/// params ReadOnlySpan&lt;T&gt; (C# 13): виклик Sum(1, 2, 3, 4) з params int[] створює масив у купі на кожен виклик,
/// а з params ReadOnlySpan&lt;int&gt; компілятор розміщує аргументи в стеку (inline array) — без алокацій.
/// Це можливість компілятора, а не рантайму, тож працює однаково на .NET 8, 9 і 10 (код зібрано з LangVersion latest).
/// Чого очікувати: ParamsArray — 40 B Allocated на всіх рантаймах (на .NET 10 escape analysis може це прибрати);
/// ParamsReadOnlySpan — 0 B і швидше.
/// </summary>
[Config(typeof(DotNetVersionsConfig))]
public class ParamsSpanBenchmarks
{
    private int _a = 1, _b = 2, _c = 3, _d = 4;

    [Benchmark(Baseline = true)]
    public int ParamsArray() => SumArray(_a, _b, _c, _d);

    [Benchmark]
    public int ParamsReadOnlySpan() => SumSpan(_a, _b, _c, _d);

    private static int SumArray(params int[] values)
    {
        int sum = 0;
        foreach (int v in values)
            sum += v;
        return sum;
    }

    private static int SumSpan(params ReadOnlySpan<int> values)
    {
        int sum = 0;
        foreach (int v in values)
            sum += v;
        return sum;
    }
}
