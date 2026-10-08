using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.Memory;

/// <summary>
/// [MemoryDiagnoser]: скільки коштує LINQ у пам'яті. Задача — сума квадратів парних чисел з 10 000.
/// Чого очікувати: цикл — 0 B; Where/Select/Sum — кілька об'єктів-ітераторів (≈ сотня байт)
/// і виклики через делегати; ToList() посередині ланцюжка додає алокацію цілого списку.
/// </summary>
[MemoryDiagnoser]
public class LinqVsLoopBenchmarks
{
    private int[] _numbers = [];

    [GlobalSetup]
    public void Setup() => _numbers = Enumerable.Range(0, 10_000).ToArray();

    [Benchmark(Baseline = true)]
    public long ForLoop()
    {
        long sum = 0;
        foreach (int n in _numbers)
            if (n % 2 == 0)
                sum += (long)n * n;
        return sum;
    }

    [Benchmark]
    public long LinqChain() => _numbers.Where(n => n % 2 == 0).Select(n => (long)n * n).Sum();

    [Benchmark]
    public long LinqWithToList() => _numbers.Where(n => n % 2 == 0).ToList().Select(n => (long)n * n).Sum();
}
