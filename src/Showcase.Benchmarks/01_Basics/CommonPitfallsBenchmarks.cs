using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

namespace Showcase.Benchmarks.Basics;

/// <summary>
/// Типові пастки мікробенчмарків і як їх уникнути.
/// Методи з префіксом Wrong_ навмисно написані неправильно.
/// Чого очікувати:
/// - Wrong_ResultDiscarded: результат не використовується, JIT може викинути обчислення
///   (dead code elimination) — час ≈ 0, BenchmarkDotNet попереджає про ZeroMeasurement;
/// - Wrong_ConstantFolding: аргумент — константа, JIT обчислює результат під час компіляції — теж ≈ 0;
/// - Right_ReturnResult: вхід з поля, результат повертається — реальна вартість Math.Sqrt;
/// - LINQ: повернути з бенчмарка відкладений запит (IEnumerable з Where/Select) не вийде —
///   валідатор BenchmarkDotNet зупиняє запуск з помилкою «returns a deferred execution result»,
///   бо виміряно було б лише створення запиту. Тому два коректні варіанти:
///   Right_LazyLinqMaterialized (ToArray — плюс алокація масиву) і
///   Right_LazyLinqConsumed (Consumer, аналог Blackhole — перебирає елементи без збереження).
/// </summary>
public class CommonPitfallsBenchmarks
{
    private readonly Consumer _consumer = new();
    private double _input;
    private int[] _numbers = [];

    [GlobalSetup]
    public void Setup()
    {
        _input = 12_345.678;
        _numbers = Enumerable.Range(0, 1_000).ToArray();
    }

    [Benchmark]
    public void Wrong_ResultDiscarded() => Math.Sqrt(_input);

    [Benchmark]
    public double Wrong_ConstantFolding() => Math.Sqrt(12_345.678);

    [Benchmark(Baseline = true)]
    public double Right_ReturnResult() => Math.Sqrt(_input);

    [Benchmark]
    public int[] Right_LazyLinqMaterialized() => _numbers.Where(n => n % 3 == 0).Select(n => n * 2).ToArray();

    [Benchmark]
    public void Right_LazyLinqConsumed() => _numbers.Where(n => n % 3 == 0).Select(n => n * 2).Consume(_consumer);
}
