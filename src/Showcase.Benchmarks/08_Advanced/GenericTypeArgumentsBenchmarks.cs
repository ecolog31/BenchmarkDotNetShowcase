using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.Advanced;

/// <summary>
/// [GenericTypeArguments]: generic-клас бенчмарка запускається окремо для кожного вказаного типу.
/// Назва типу з'являється в назві бенчмарка (GenericTypeArgumentsBenchmarks&lt;Int32&gt;).
/// Приклад: List&lt;T&gt;.Contains для value type (int), reference type (string) і великої структури (Guid).
/// Чого очікувати: int найшвидший (векторизований пошук), Guid повільніший (16 байт на порівняння),
/// string — найповільніший (посилання + порівняння вмісту рядків).
/// </summary>
[GenericTypeArguments(typeof(int))]
[GenericTypeArguments(typeof(Guid))]
[GenericTypeArguments(typeof(string))]
public class GenericTypeArgumentsBenchmarks<T> where T : notnull
{
    private List<T> _items = [];
    private T _missing = default!;

    [GlobalSetup]
    public void Setup()
    {
        _items = Enumerable.Range(0, 1_000).Select(Create).ToList();
        _missing = Create(-1);
    }

    [Benchmark]
    public bool ContainsMissing() => _items.Contains(_missing);

    private static T Create(int i) => typeof(T) switch
    {
        var t when t == typeof(int) => (T)(object)i,
        var t when t == typeof(string) => (T)(object)$"item-{i}",
        var t when t == typeof(Guid) => (T)(object)new Guid(i, 0, 0, new byte[8]),
        _ => throw new NotSupportedException(typeof(T).Name),
    };
}
