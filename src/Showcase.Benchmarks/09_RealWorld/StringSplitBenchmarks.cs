using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.RealWorld;

/// <summary>
/// Розбір CSV-рядка з 50 полів і сума числових значень: string.Split vs ReadOnlySpan&lt;char&gt; з IndexOf.
/// Split створює масив і окремий рядок на кожне поле; Span-версія лише «нарізає» вихідний рядок без копій
/// і розбирає числа через int.Parse(ReadOnlySpan&lt;char&gt;).
/// Чого очікувати: Span-версія майже вдвічі швидша і з Allocated 0 B; Split — ~2 КБ на виклик (масив + 50 рядків).
/// </summary>
[MemoryDiagnoser]
public class StringSplitBenchmarks
{
    private string _line = "";

    [GlobalSetup]
    public void Setup() => _line = string.Join(',', Enumerable.Range(0, 50).Select(i => (i * 37).ToString()));

    [Benchmark(Baseline = true)]
    public int StringSplit()
    {
        int sum = 0;
        foreach (string field in _line.Split(','))
            sum += int.Parse(field);
        return sum;
    }

    [Benchmark]
    public int SpanIndexOf()
    {
        ReadOnlySpan<char> remaining = _line;
        int sum = 0;
        while (true)
        {
            int comma = remaining.IndexOf(',');
            if (comma < 0)
            {
                sum += int.Parse(remaining);
                return sum;
            }
            sum += int.Parse(remaining[..comma]);
            remaining = remaining[(comma + 1)..];
        }
    }
}
