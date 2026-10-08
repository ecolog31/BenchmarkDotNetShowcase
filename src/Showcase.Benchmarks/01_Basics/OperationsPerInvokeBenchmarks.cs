using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.Basics;

/// <summary>
/// [Benchmark(OperationsPerInvoke = N)]: метод виконує N однакових операцій за виклик,
/// а BenchmarkDotNet ділить виміряний час на N і показує вартість однієї операції.
/// Корисно для дуже дешевих операцій, де накладні витрати виклику порівнянні з самою роботою.
/// Чого очікувати: обидва рядки — у наносекундах на ОДИН Math.Sqrt (без OperationsPerInvoke
/// BatchedSqrt показав би приблизно в 1000 разів більше). Точного збігу не буде: у циклі
/// додаються накопичення суми, читання з масиву та керування циклом, тобто це вже трохи інший код.
/// </summary>
public class OperationsPerInvokeBenchmarks
{
    private const int Operations = 1_000;
    private double[] _values = [];

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        _values = Enumerable.Range(0, Operations).Select(_ => random.NextDouble() * 1_000).ToArray();
    }

    [Benchmark(Baseline = true)]
    public double SingleSqrt() => Math.Sqrt(_values[0]);

    [Benchmark(OperationsPerInvoke = Operations)]
    public double BatchedSqrt()
    {
        double sum = 0;
        for (int i = 0; i < _values.Length; i++)
            sum += Math.Sqrt(_values[i]);
        return sum;
    }
}
