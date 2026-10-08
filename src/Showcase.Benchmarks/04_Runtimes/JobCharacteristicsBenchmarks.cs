using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

namespace Showcase.Benchmarks.Runtimes;

/// <summary>
/// Ручне налаштування характеристик Job:
/// WithLaunchCount — скільки окремих процесів; WithWarmupCount — ітерацій прогріву;
/// WithIterationCount — виміряних ітерацій; WithInvocationCount — викликів методу в одній ітерації;
/// WithUnrollFactor — скільки викликів розгорнуто в циклі всередині ітерації (InvocationCount має ділитися на нього);
/// WithPlatform — розрядність процесу (тут явно x64; Arm64 — тільки на відповідній машині).
/// Чого очікувати: Mean у обох job-ів близький, але «Quick» має значно більший Error (мало вимірів),
/// а «Precise» — вужчий довірчий інтервал ціною довшого запуску.
/// </summary>
[Config(typeof(JobCharacteristicsConfig))]
public class JobCharacteristicsBenchmarks
{
    private int[] _data = [];

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        _data = Enumerable.Range(0, 1_000).Select(_ => random.Next()).ToArray();
    }

    [Benchmark]
    public int[] SortCopy()
    {
        var copy = (int[])_data.Clone();
        Array.Sort(copy);
        return copy;
    }

    private sealed class JobCharacteristicsConfig : ManualConfig
    {
        public JobCharacteristicsConfig()
        {
            AddJob(Job.Default
                .WithId("Quick")
                .WithPlatform(Platform.X64)
                .WithLaunchCount(1)
                .WithWarmupCount(1)
                .WithIterationCount(3)
                .WithInvocationCount(1_000)
                .WithUnrollFactor(1)
                .AsBaseline());

            AddJob(Job.Default
                .WithId("Precise")
                .WithPlatform(Platform.X64)
                .WithLaunchCount(2)
                .WithWarmupCount(10)
                .WithIterationCount(30)
                .WithInvocationCount(16_000) // ≈ 120 мс на ітерацію — вище за рекомендований мінімум 100 мс
                .WithUnrollFactor(4));
        }
    }
}
