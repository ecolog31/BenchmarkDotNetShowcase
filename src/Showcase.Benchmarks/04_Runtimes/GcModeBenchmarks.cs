using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace Showcase.Benchmarks.Runtimes;

/// <summary>
/// Режими GC як характеристики Job: WithGcServer / WithGcConcurrent.
/// Те саме для всього класу можна задати атрибутами [GcServer(true)] і [GcConcurrent(false)] —
/// але атрибут застосовується до всіх job-ів, а тут потрібне порівняння, тому три job-и в ManualConfig.
/// Навантаження — паралельне створення багатьох короткоживучих об'єктів з усіх ядер.
/// Чого очікувати: Server GC (окрема купа і GC-потік на кожне ядро) швидший при паралельних алокаціях,
/// але з'їдає більше пам'яті; Concurrent on/off на такому навантаженні впливає мало
/// (фонова збірка стосується Gen2, а тут майже все вмирає в Gen0).
/// </summary>
[Config(typeof(GcModeConfig))]
[MemoryDiagnoser]
public class GcModeBenchmarks
{
    private const int ObjectsPerIteration = 200_000;

    [Benchmark]
    public long ParallelAllocations()
    {
        long total = 0;
        Parallel.For(0, Environment.ProcessorCount,
            localInit: () => 0L,
            body: (_, _, local) =>
            {
                // Масиви зберігаються в «вікно» з 256 елементів: так вони гарантовано потрапляють у купу.
                // Без цього .NET 10 (escape analysis) розміщує new byte[64] у стеку — і GC не має що збирати.
                var window = new byte[256][];
                for (int i = 0; i < ObjectsPerIteration / Environment.ProcessorCount; i++)
                {
                    var item = new byte[64];
                    window[i & 255] = item;
                    local += item.Length;
                }
                return local;
            },
            localFinally: local => Interlocked.Add(ref total, local));
        return total;
    }

    private sealed class GcModeConfig : ManualConfig
    {
        public GcModeConfig()
        {
            AddJob(Job.Default.WithId("Workstation").WithGcServer(false).AsBaseline());
            AddJob(Job.Default.WithId("Server").WithGcServer(true));
            AddJob(Job.Default.WithId("WorkstationNonConcurrent").WithGcServer(false).WithGcConcurrent(false));
        }
    }
}
