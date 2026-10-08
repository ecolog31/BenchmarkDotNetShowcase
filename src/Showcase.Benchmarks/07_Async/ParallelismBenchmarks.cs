using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.Async;

/// <summary>
/// Паралельне CPU-навантаження трьома способами + [ThreadingDiagnoser] (Completed Work Items, Lock Contentions).
/// Задача — порахувати прості числа до 200 000, розбивши діапазон на шматки.
/// Чого очікувати: усі паралельні варіанти в кілька разів швидші за послідовний (≈ кількість ядер);
/// між собою близькі. Completed Work Items: у Task.WhenAll — приблизно кількість шматків,
/// у Parallel.For і PLINQ — стільки, скільки робочих елементів створив планувальник.
/// </summary>
[ThreadingDiagnoser]
[MemoryDiagnoser]
public class ParallelismBenchmarks
{
    private const int Limit = 200_000;
    private const int Chunks = 64;
    private const int ChunkSize = Limit / Chunks;

    [Benchmark(Baseline = true)]
    public int Sequential() => CountPrimes(0, Limit);

    [Benchmark]
    public int ParallelFor()
    {
        int total = 0;
        Parallel.For(0, Chunks, chunk =>
            Interlocked.Add(ref total, CountPrimes(chunk * ChunkSize, (chunk + 1) * ChunkSize)));
        return total;
    }

    [Benchmark]
    public int Plinq() =>
        Enumerable.Range(0, Chunks)
            .AsParallel()
            .Sum(chunk => CountPrimes(chunk * ChunkSize, (chunk + 1) * ChunkSize));

    [Benchmark]
    public async Task<int> TaskWhenAll()
    {
        var tasks = Enumerable.Range(0, Chunks)
            .Select(chunk => Task.Run(() => CountPrimes(chunk * ChunkSize, (chunk + 1) * ChunkSize)));
        int[] results = await Task.WhenAll(tasks);
        return results.Sum();
    }

    private static int CountPrimes(int from, int to)
    {
        int count = 0;
        for (int n = Math.Max(from, 2); n < to; n++)
        {
            bool isPrime = true;
            for (int d = 2; d * d <= n; d++)
                if (n % d == 0) { isPrime = false; break; }
            if (isPrime) count++;
        }
        return count;
    }
}
