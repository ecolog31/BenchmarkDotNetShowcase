using System.Collections.Frozen;
using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.RealWorld;

/// <summary>
/// Пошук у словниках з рядковими ключами: Dictionary vs FrozenDictionary vs ImmutableDictionary.
/// FrozenDictionary один раз (дорого) аналізує ключі при створенні і підбирає оптимальну хеш-стратегію —
/// ідеальний для конфігурацій і довідників, які не змінюються. ImmutableDictionary — дерево (AVL),
/// оптимізоване для «змін без мутації», а не для швидкого читання.
/// Кожен метод працює з конкретним типом (без IReadOnlyDictionary), щоб не додавати інтерфейсних викликів.
/// Чого очікувати: Frozen швидший за Dictionary (тут у 1.4–2 рази); Immutable — у 1.5–2 рази повільніший.
/// </summary>
public class DictionaryKindsBenchmarks
{
    private string[] _lookupKeys = [];
    private Dictionary<string, int> _dictionary = [];
    private FrozenDictionary<string, int> _frozen = FrozenDictionary<string, int>.Empty;
    private ImmutableDictionary<string, int> _immutable = ImmutableDictionary<string, int>.Empty;

    [Params(10, 1_000)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        string[] keys = Enumerable.Range(0, Count).Select(i => $"setting.section{i % 7}.key{i}").ToArray();
        _dictionary = keys.ToDictionary(k => k, k => k.Length);
        _frozen = _dictionary.ToFrozenDictionary();
        _immutable = _dictionary.ToImmutableDictionary();
        // Інші екземпляри тих самих рядків: порівняння за вмістом, а не за посиланням.
        _lookupKeys = keys.Select(k => new string(k.AsSpan())).ToArray();
    }

    [Benchmark(Baseline = true)]
    public int DictionaryLookup()
    {
        int sum = 0;
        foreach (string key in _lookupKeys)
            sum += _dictionary[key];
        return sum;
    }

    [Benchmark]
    public int FrozenDictionaryLookup()
    {
        int sum = 0;
        foreach (string key in _lookupKeys)
            sum += _frozen[key];
        return sum;
    }

    [Benchmark]
    public int ImmutableDictionaryLookup()
    {
        int sum = 0;
        foreach (string key in _lookupKeys)
            sum += _immutable[key];
        return sum;
    }
}
