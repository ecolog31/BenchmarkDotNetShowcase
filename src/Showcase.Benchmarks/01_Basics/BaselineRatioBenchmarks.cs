using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.Basics;

/// <summary>
/// [Benchmark(Baseline = true)] додає колонки Ratio і RatioSD: швидкість кожного методу
/// відносно базового (Ratio 1.00). Три способи порахувати суму масиву з 10 000 int.
/// Чого очікувати: foreach може бути швидшим за for — for тут читає поле _numbers в умові
/// та індексації, тож JIT може не прибрати перевірки меж, а foreach копіює посилання в локальну
/// змінну (перевірка — через DisassemblyDiagnoser у 05_Diagnosers);
/// Enumerable.Sum на масиві в .NET 8+ векторизований, тож найшвидший.
/// </summary>
public class BaselineRatioBenchmarks
{
    private int[] _numbers = [];

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        _numbers = Enumerable.Range(0, 10_000).Select(_ => random.Next(100)).ToArray();
    }

    [Benchmark(Baseline = true)]
    public int ForLoop()
    {
        int sum = 0;
        for (int i = 0; i < _numbers.Length; i++)
            sum += _numbers[i];
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
    public int LinqSum() => _numbers.Sum();
}
