using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.Configuration;

/// <summary>
/// [InProcess]: бенчмарк виконується в тому ж процесі, що й BenchmarkDotNet, без генерації та збірки
/// окремого проєкту. Швидший старт; корисно, коли окремий процес неможливий (наприклад, обмеження середовища
/// або антивірус блокує згенеровані exe). Мінуси: немає ізоляції (стан процесу, JIT-налаштування хоста),
/// не можна порівнювати рантайми.
/// [InProcess] додає окремий job: з --job short у таблиці буде два рядки — InProcess і звичайний ShortRun в окремому процесі.
/// Чого очікувати: job InProcess з Toolchain = InProcessEmitToolchain; цифри близькі до запуску в окремому процесі.
/// </summary>
[InProcess]
public class InProcessBenchmarks
{
    private readonly int[] _numbers = Enumerable.Range(0, 10_000).ToArray();

    [Benchmark]
    public int ArrayIndexOf() => Array.IndexOf(_numbers, 9_999);
}
