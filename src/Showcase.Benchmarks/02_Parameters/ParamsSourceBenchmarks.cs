using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.Parameters;

/// <summary>
/// [ParamsSource]: значення параметра беруться з властивості чи методу, що повертає IEnumerable&lt;T&gt;.
/// Так можна передавати складні об'єкти, які не можна записати в атрибут.
/// У таблиці показується ToString() об'єкта — тому DataShape перевизначає його коротким описом.
/// Чого очікувати: Array.Sort (introsort) на вже відсортованих даних і на даних з малою кількістю
/// унікальних значень швидший, ніж на випадкових; на «пилці» — десь посередині.
/// </summary>
public class ParamsSourceBenchmarks
{
    private int[] _source = [];

    public sealed record DataShape(string Name, int Size, Func<int, Random, int> Generator)
    {
        public override string ToString() => $"{Name}-{Size}";
    }

    public static IEnumerable<DataShape> Shapes =>
    [
        new("Random", 10_000, (_, rnd) => rnd.Next()),
        new("Sorted", 10_000, (i, _) => i),
        new("FewUnique", 10_000, (_, rnd) => rnd.Next(4)),
        new("Sawtooth", 10_000, (i, _) => i % 100),
    ];

    [ParamsSource(nameof(Shapes))]
    public DataShape Shape { get; set; } = null!;

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        _source = Enumerable.Range(0, Shape.Size).Select(i => Shape.Generator(i, random)).ToArray();
    }

    [Benchmark]
    public int[] ArraySort()
    {
        var copy = (int[])_source.Clone();
        Array.Sort(copy);
        return copy;
    }
}
