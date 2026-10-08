using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.RealWorld;

/// <summary>
/// Перевірка «чи є елемент у колекції»: List&lt;T&gt;.Contains (лінійний пошук) vs HashSet&lt;T&gt;.Contains (хеш).
/// На малих колекціях лінійний пошук може бути не гіршим (немає хешування), на великих — програє на порядки.
/// Чого очікувати: при Count = 8 різниця невелика (List може навіть вигравати для int);
/// при Count = 10 000 HashSet у сотні-тисячі разів швидший.
/// </summary>
public class HashSetVsListBenchmarks
{
    private List<int> _list = [];
    private HashSet<int> _set = [];
    private int[] _probes = [];

    [Params(8, 10_000)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        _list = Enumerable.Range(0, Count).Select(i => i * 3).ToList();
        _set = [.. _list];
        // Половина значень присутня, половина — ні.
        _probes = Enumerable.Range(0, 100).Select(_ => random.Next(Count * 6)).ToArray();
    }

    [Benchmark(Baseline = true)]
    public int ListContains()
    {
        int found = 0;
        foreach (int probe in _probes)
            if (_list.Contains(probe)) found++;
        return found;
    }

    [Benchmark]
    public int HashSetContains()
    {
        int found = 0;
        foreach (int probe in _probes)
            if (_set.Contains(probe)) found++;
        return found;
    }
}
