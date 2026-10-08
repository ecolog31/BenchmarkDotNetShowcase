using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using Perfolizer.Mathematics.OutlierDetection;

namespace Showcase.Benchmarks.Configuration;

/// <summary>
/// Обробка викидів (outliers) — характеристика job-а OutlierMode:
/// RemoveUpper (за замовчуванням) — відкидає аномально довгі виміри (GC, переключення потоків);
/// DontRemove — враховує все; RemoveAll / RemoveLower — інші варіанти.
/// Те саме для всього класу — атрибут [Outliers(OutlierMode.DontRemove)].
/// Метод навмисно іноді робить «паузу» (Thread.SpinWait кожен 500-й виклик — приблизно кожна 30-та ітерація), щоб з'явилися викиди.
/// Ratio тут не має сенсу (порівнюємо обробку тих самих вимірів), тому baseline немає.
/// Чого очікувати: в DontRemove-job вище Mean і StdDev; у RemoveUpper BenchmarkDotNet пише
/// «Outliers ... N outliers were removed» під таблицею.
/// </summary>
[Config(typeof(OutliersConfig))]
public class OutliersBenchmarks
{
    private int _calls;

    [Benchmark]
    public int WorkWithOccasionalSpike()
    {
        if (++_calls % 500 == 0)
            Thread.SpinWait(200_000);
        int sum = 0;
        for (int i = 0; i < 1_000; i++)
            sum += i ^ _calls;
        return sum;
    }

    private sealed class OutliersConfig : ManualConfig
    {
        public OutliersConfig()
        {
            // Мала кількість викликів на ітерацію — щоб «пауза» потрапляла в окремі ітерації, а не усереднювалась.
            Job baseJob = Job.Default.WithInvocationCount(16).WithUnrollFactor(1).WithIterationCount(60);
            AddJob(baseJob.WithId("RemoveUpper").WithOutlierMode(OutlierMode.RemoveUpper));
            AddJob(baseJob.WithId("DontRemove").WithOutlierMode(OutlierMode.DontRemove));
        }
    }
}
