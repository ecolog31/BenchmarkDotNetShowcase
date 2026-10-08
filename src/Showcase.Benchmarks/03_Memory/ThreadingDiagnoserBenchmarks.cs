using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.Memory;

/// <summary>
/// [ThreadingDiagnoser]: колонки Completed Work Items (скільки завдань виконав ThreadPool)
/// і Lock Contentions (скільки разів потік чекав на зайнятий lock) на одну операцію.
/// Задача — паралельно збільшити спільний лічильник 100 000 разів трьома способами.
/// Чого очікувати: lock — велика кількість Lock Contentions і найповільніше;
/// Interlocked — 0 contentions, але потоки все одно змагаються за одну кеш-лінію;
/// локальні суми на потік (Parallel.For з localInit) — найшвидше, синхронізація лише в кінці.
/// </summary>
[ThreadingDiagnoser]
public class ThreadingDiagnoserBenchmarks
{
    private const int Iterations = 100_000;
    private readonly object _sync = new();
    private long _counter;

    [Benchmark(Baseline = true)]
    public long SharedCounterWithLock()
    {
        _counter = 0;
        Parallel.For(0, Iterations, _ =>
        {
            lock (_sync)
                _counter++;
        });
        return _counter;
    }

    [Benchmark]
    public long SharedCounterWithInterlocked()
    {
        _counter = 0;
        Parallel.For(0, Iterations, _ => Interlocked.Increment(ref _counter));
        return _counter;
    }

    [Benchmark]
    public long ThreadLocalSums()
    {
        long total = 0;
        Parallel.For(0, Iterations,
            localInit: () => 0L,
            body: (_, _, local) => local + 1,
            localFinally: local => Interlocked.Add(ref total, local));
        return total;
    }
}
