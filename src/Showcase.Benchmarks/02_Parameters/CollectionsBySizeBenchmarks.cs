using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.Parameters;

/// <summary>
/// Типовий сценарій для [Params]: та сама операція на різних розмірах даних.
/// Сума елементів List&lt;int&gt; vs int[] vs Span&lt;int&gt; на 10 / 1 000 / 100 000 елементах.
/// Ratio рахується окремо для кожного значення Size (baseline — масив).
/// Чого очікувати: на 10 елементах помітні фіксовані накладні витрати; List&lt;T&gt; повільніший
/// за масив (індексатор з перевіркою версії/меж); Span&lt;T&gt; ≈ масив або трохи швидший.
/// </summary>
public class CollectionsBySizeBenchmarks
{
    private int[] _array = [];
    private List<int> _list = [];

    [Params(10, 1_000, 100_000)]
    public int Size { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _array = Enumerable.Range(0, Size).ToArray();
        _list = [.. _array];
    }

    [Benchmark(Baseline = true)]
    public long SumArray()
    {
        int[] array = _array;
        long sum = 0;
        for (int i = 0; i < array.Length; i++)
            sum += array[i];
        return sum;
    }

    [Benchmark]
    public long SumList()
    {
        List<int> list = _list;
        long sum = 0;
        for (int i = 0; i < list.Count; i++)
            sum += list[i];
        return sum;
    }

    [Benchmark]
    public long SumSpan()
    {
        Span<int> span = _array;
        long sum = 0;
        for (int i = 0; i < span.Length; i++)
            sum += span[i];
        return sum;
    }
}
