using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

namespace Showcase.Benchmarks.Diagnosers;

/// <summary>
/// [EventPipeProfiler]: кросплатформний профайлер на базі EventPipe — без прав адміністратора.
/// Записує трасу *.nettrace і конвертує її у *.speedscope.json (відкривається на https://www.speedscope.app).
/// Профіль CpuSampling — семпли стеків CPU; є також GcVerbose, GcCollect, Jit.
/// Приклад: метод з двома «гарячими» частинами — пошук простих чисел і сортування.
/// Чого очікувати: на flame graph у speedscope видно, яку частку часу забирає кожна з частин.
/// </summary>
[EventPipeProfiler(EventPipeProfile.CpuSampling)]
public class EventPipeProfilerBenchmarks
{
    private int[] _data = [];

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        _data = Enumerable.Range(0, 20_000).Select(_ => random.Next()).ToArray();
    }

    [Benchmark]
    public int CountPrimesThenSort()
    {
        int primes = CountPrimes(50_000);
        var copy = (int[])_data.Clone();
        Array.Sort(copy);
        return primes + copy[0];
    }

    private static int CountPrimes(int limit)
    {
        int count = 0;
        for (int n = 2; n < limit; n++)
        {
            bool isPrime = true;
            for (int d = 2; d * d <= n; d++)
                if (n % d == 0) { isPrime = false; break; }
            if (isPrime) count++;
        }
        return count;
    }
}
