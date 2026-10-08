using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Validators;

namespace Showcase.Benchmarks.Configuration;

/// <summary>
/// Валідатори запускаються ДО вимірів і можуть зупинити запуск:
/// ExecutionValidator — один раз виконує кожен метод і перевіряє, що він не кидає виняток;
/// ReturnValueValidator — перевіряє, що всі бенчмарки класу повертають ОДНАКОВЕ значення
/// (зручно, коли порівнюємо кілька реалізацій одного алгоритму — заодно перевіряється коректність);
/// JitOptimizationsValidator — зупиняє запуск, якщо збірка без оптимізацій (Debug).
/// FailOnError — помилка валідатора зупиняє запуск (а не лише попередження).
/// Чого очікувати: усі три реалізації повертають 4950 — валідатори мовчать, таблиця звичайна.
/// Якщо «зламати» одну реалізацію, запуск зупиниться з повідомленням ReturnValueValidator.
/// </summary>
[Config(typeof(StrictValidationConfig))]
public class ValidatorsBenchmarks
{
    private readonly int[] _numbers = Enumerable.Range(0, 100).ToArray();

    [Benchmark(Baseline = true)]
    public int SumWithLoop()
    {
        int sum = 0;
        foreach (int n in _numbers)
            sum += n;
        return sum;
    }

    [Benchmark]
    public int SumWithLinq() => _numbers.Sum();

    [Benchmark]
    public int SumWithFormula() => _numbers.Length * (_numbers.Length - 1) / 2;

    private sealed class StrictValidationConfig : ManualConfig
    {
        public StrictValidationConfig()
        {
            AddValidator(ExecutionValidator.FailOnError);
            AddValidator(ReturnValueValidator.FailOnError);
            AddValidator(JitOptimizationsValidator.FailOnError);
        }
    }
}
