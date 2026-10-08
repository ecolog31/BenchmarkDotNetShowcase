using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.Setup;

/// <summary>
/// [IterationSetup] / [IterationCleanup]: виконуються перед / після КОЖНОЇ ітерації (а не кожного виклику!).
/// Потрібні, коли метод змінює дані (тут — сортує масив на місці) і кожна ітерація має починати з нуля.
/// Застереження щодо точності: щоб setup справді передував кожному виклику, потрібен [InvocationCount(1)];
/// тоді ітерація — це один виклик, і для короткого коду таймер не має достатньої роздільної здатності
/// (BenchmarkDotNet попереджає MinIterationTime, якщо ітерація коротша за 100 мс). Тому масив великий — 2 000 000 елементів.
/// Альтернатива без IterationSetup — копіювати дані всередині методу (CloneInsideMethod),
/// але тоді вартість копіювання входить у вимір.
/// Чого очікувати: час однаковий у межах шуму (копія 8 МБ — близько 1% від сортування), але CloneInsideMethod алокує 8 МБ;
/// у SortWithIterationSetup Allocated ≈ 0, бо масив готується поза виміром.
/// </summary>
[MemoryDiagnoser]
[InvocationCount(1)]
public class IterationSetupBenchmarks
{
    private const int Size = 2_000_000;
    private int[] _original = [];
    private int[] _working = [];

    [GlobalSetup]
    public void GlobalSetup()
    {
        var random = new Random(42);
        _original = Enumerable.Range(0, Size).Select(_ => random.Next()).ToArray();
        _working = new int[Size];
    }

    [IterationSetup(Target = nameof(SortWithIterationSetup))]
    public void RestoreUnsortedData() => _original.CopyTo(_working, 0);

    [IterationCleanup(Target = nameof(SortWithIterationSetup))]
    public void VerifySorted()
    {
        // Перевірка поза виміром: дані справді відсортовані.
        if (_working[0] > _working[^1])
            throw new InvalidOperationException("Масив не відсортовано");
    }

    [Benchmark(Baseline = true)]
    public int SortWithIterationSetup()
    {
        Array.Sort(_working);
        return _working[0];
    }

    [Benchmark]
    public int CloneInsideMethod()
    {
        var copy = (int[])_original.Clone();
        Array.Sort(copy);
        return copy[0];
    }
}
