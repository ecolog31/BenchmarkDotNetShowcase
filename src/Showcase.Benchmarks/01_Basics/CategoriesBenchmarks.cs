using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace Showcase.Benchmarks.Basics;

/// <summary>
/// [BenchmarkCategory] + [GroupBenchmarksBy(ByCategory)] + [CategoriesColumn]:
/// кілька незалежних порівнянь в одному класі, кожне зі своїм baseline.
/// Ratio рахується всередині категорії, таблиця розділена на групи.
/// Чого очікувати: «Search» — бінарний пошук на порядки швидший за лінійний;
/// «Sort» — Array.Sort швидший за LINQ OrderBy і не створює зайвих алокацій.
/// Також можна запустити тільки одну категорію: --anyCategories Search
/// </summary>
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class CategoriesBenchmarks
{
    private const int Size = 10_000;
    private int[] _sorted = [];
    private int[] _unsorted = [];
    private int _target;

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        _unsorted = Enumerable.Range(0, Size).Select(_ => random.Next()).ToArray();
        _sorted = _unsorted.Order().ToArray();
        _target = _sorted[Size * 3 / 4];
    }

    [Benchmark(Baseline = true), BenchmarkCategory("Search")]
    public int LinearSearch() => Array.IndexOf(_sorted, _target);

    [Benchmark, BenchmarkCategory("Search")]
    public int BinarySearch() => Array.BinarySearch(_sorted, _target);

    // Обидва методи сортують копію, щоб не зіпсувати вхідні дані для наступних ітерацій;
    // вартість копіювання однакова для обох і входить у вимір.
    [Benchmark(Baseline = true), BenchmarkCategory("Sort")]
    public int[] ArraySort()
    {
        var copy = (int[])_unsorted.Clone();
        Array.Sort(copy);
        return copy;
    }

    [Benchmark, BenchmarkCategory("Sort")]
    public int[] LinqOrderBy() => _unsorted.OrderBy(x => x).ToArray();
}
