using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.DotNet10;

/// <summary>
/// Девіртуалізація інтерфейсів і делегатів між версіями JIT.
/// ArrayViaIEnumerable: foreach по int[], приведеному до IEnumerable&lt;int&gt; — у .NET 8 це виклики через інтерфейс
/// і алокація енумератора; .NET 10 вміє девіртуалізувати енумератор масиву і розмістити його в стеку.
/// ReadOnlyListIndexer: доступ через IReadOnlyList&lt;int&gt; до масиву.
/// DelegateCall: виклик статичної лямбди через Func у циклі (guarded devirtualization делегатів через PGO).
/// Чого очікувати: ArrayViaIEnumerable на .NET 10 значно швидший і з Allocated 0 B (на .NET 8 — 32 B);
/// інші два — поступове прискорення від версії до версії.
/// </summary>
[Config(typeof(DotNetVersionsConfig))]
public class DevirtualizationBenchmarks
{
    private int[] _array = [];
    private IEnumerable<int> _asEnumerable = [];
    private IReadOnlyList<int> _asReadOnlyList = [];
    private readonly Func<int, int> _double = static x => x * 2;

    [GlobalSetup]
    public void Setup()
    {
        _array = Enumerable.Range(0, 1_000).ToArray();
        _asEnumerable = _array;
        _asReadOnlyList = _array;
    }

    [Benchmark]
    public int ArrayViaIEnumerable()
    {
        int sum = 0;
        foreach (int value in _asEnumerable)
            sum += value;
        return sum;
    }

    [Benchmark]
    public int ReadOnlyListIndexer()
    {
        IReadOnlyList<int> list = _asReadOnlyList;
        int sum = 0;
        for (int i = 0; i < list.Count; i++)
            sum += list[i];
        return sum;
    }

    [Benchmark]
    public int DelegateCall()
    {
        Func<int, int> transform = _double;
        int sum = 0;
        foreach (int value in _array)
            sum += transform(value);
        return sum;
    }
}
