using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.Configuration;

/// <summary>
/// [StatisticalTestColumn]: перевіряє тест Манна–Вітні, чи статистично значуща різниця з baseline,
/// з порогом (тут 5%). Колонка показує Faster / Slower / Same — замість «на око» порівнювати Mean.
/// Приклад: цикли for/foreach, LINQ Aggregate з делегатом і векторизований LINQ Sum.
/// Чого очікувати: ForeachLoop — Same; LinqSum — Faster (SIMD). LinqAggregate на .NET 10 теж Same:
/// Dynamic PGO девіртуалізує та інлайнить делегат — «на око» здавалося б, що LINQ мусить бути повільнішим,
/// а статистичний тест показує, що різниці немає.
/// </summary>
[StatisticalTestColumn("5%")]
public class StatisticalTestBenchmarks
{
    private readonly int[] _numbers = Enumerable.Range(0, 1_000).ToArray();

    [Benchmark(Baseline = true)]
    public int ForLoop()
    {
        int[] numbers = _numbers;
        int sum = 0;
        for (int i = 0; i < numbers.Length; i++)
            sum += numbers[i];
        return sum;
    }

    [Benchmark]
    public int ForeachLoop()
    {
        int sum = 0;
        foreach (int n in _numbers)
            sum += n;
        return sum;
    }

    [Benchmark]
    public int LinqAggregate() => _numbers.Aggregate(0, (acc, n) => acc + n);

    [Benchmark]
    public int LinqSum() => _numbers.Sum();
}
