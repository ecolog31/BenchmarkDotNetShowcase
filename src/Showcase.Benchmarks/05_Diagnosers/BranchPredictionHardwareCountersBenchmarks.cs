using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

namespace Showcase.Benchmarks.Diagnosers;

/// <summary>
/// [HardwareCounters]: лічильники продуктивності процесора (PMC) через ETW —
/// тут BranchMispredictions і BranchInstructions на одну операцію.
/// Класичний приклад: однаковий код і однакові дані, але відсортовані vs випадкові.
/// Чого очікувати: на випадкових даних ~50% переходів передбачено неправильно (тисячі BranchMispredictions/op)
/// і код у кілька разів повільніший; на відсортованих — майже нуль промахів.
///
/// ⚠ Потребує запуску від АДМІНІСТРАТОРА і фізичної машини (у VM лічильники зазвичай недоступні).
/// На процесорах AMD Windows надає не всі PMC-джерела — якщо лічильник недоступний, BenchmarkDotNet про це напише.
///   dotnet run -c Release -f net10.0 -- --filter "*HardwareCounters*" --job short
/// </summary>
[HardwareCounters(HardwareCounter.BranchMispredictions, HardwareCounter.BranchInstructions)]
public class BranchPredictionHardwareCountersBenchmarks
{
    private const int Size = 32_768;
    private int[] _sorted = [];
    private int[] _unsorted = [];

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        _unsorted = Enumerable.Range(0, Size).Select(_ => random.Next(256)).ToArray();
        _sorted = _unsorted.Order().ToArray();
    }

    [Benchmark(Baseline = true)]
    public long SortedData() => SumAboveThreshold(_sorted);

    [Benchmark]
    public long UnsortedData() => SumAboveThreshold(_unsorted);

    private static long SumAboveThreshold(int[] data)
    {
        long sum = 0;
        foreach (int value in data)
            if (value >= 128)
                sum += value;
        return sum;
    }
}
