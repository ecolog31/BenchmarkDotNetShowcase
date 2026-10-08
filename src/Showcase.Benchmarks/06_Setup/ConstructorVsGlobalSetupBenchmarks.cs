using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.Setup;

/// <summary>
/// Конструктор vs [GlobalSetup]: поширена помилка — готувати дані в конструкторі, залежні від [Params].
/// BenchmarkDotNet створює екземпляр класу ДО того, як присвоює значення [Params]-властивостям,
/// тому в конструкторі Size ще дорівнює 0. GlobalSetup викликається вже після присвоєння параметрів.
/// Чого очікувати: SumDataFromConstructor — ≈ 0 ns (масив порожній!) і однаковий для всіх Size;
/// SumDataFromGlobalSetup — час росте з Size. Повідомлення в лозі показують значення Size у момент виклику.
/// </summary>
public class ConstructorVsGlobalSetupBenchmarks
{
    private readonly int[] _fromConstructor;
    private int[] _fromGlobalSetup = [];

    [Params(1_000, 100_000)]
    public int Size { get; set; }

    public ConstructorVsGlobalSetupBenchmarks()
    {
        Console.WriteLine($"// Constructor: Size = {Size}");
        _fromConstructor = Enumerable.Range(0, Size).ToArray();
    }

    [GlobalSetup]
    public void Setup()
    {
        Console.WriteLine($"// GlobalSetup: Size = {Size}");
        _fromGlobalSetup = Enumerable.Range(0, Size).ToArray();
    }

    [Benchmark]
    public long SumDataFromConstructor() => Sum(_fromConstructor);

    [Benchmark]
    public long SumDataFromGlobalSetup() => Sum(_fromGlobalSetup);

    private static long Sum(int[] data)
    {
        long sum = 0;
        foreach (int value in data)
            sum += value;
        return sum;
    }
}
