using System.Collections.Frozen;
using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.Setup;

/// <summary>
/// Targeted setup: [GlobalSetup(Target = nameof(X))] / [GlobalSetup(Targets = [..])] — окремий setup
/// для конкретних бенчмарків. Кожному методу потрібна своя структура даних, і немає сенсу будувати всі
/// (наприклад, FrozenDictionary будується відчутно довго).
/// Setup без Target виконується для методів, які не мають власного targeted setup.
/// Чого очікувати: у лозі видно «// GlobalSetup» лише відповідний для кожного методу (див. Console.WriteLine);
/// SortedDictionary (дерево, порівняння рядків) — на порядок-два повільніший; FrozenDictionary тут ≈ Dictionary
/// (ключі «key-N» короткі й схожі — Frozen не має переваги; див. також 09_RealWorld/DictionaryKindsBenchmarks).
/// </summary>
public class TargetedSetupBenchmarks
{
    private const int Count = 1_000;
    private string[] _keys = [];
    private Dictionary<string, int> _dictionary = [];
    private FrozenDictionary<string, int> _frozen = FrozenDictionary<string, int>.Empty;
    private SortedDictionary<string, int> _sorted = [];

    private void CreateKeys() => _keys = Enumerable.Range(0, Count).Select(i => $"key-{i}").ToArray();

    [GlobalSetup(Targets = [nameof(DictionaryLookup), nameof(FrozenDictionaryLookup)])]
    public void SetupHashBased()
    {
        Console.WriteLine("// GlobalSetup: SetupHashBased (Dictionary + FrozenDictionary)");
        CreateKeys();
        _dictionary = _keys.ToDictionary(k => k, k => k.Length);
        _frozen = _dictionary.ToFrozenDictionary();
    }

    [GlobalSetup(Target = nameof(SortedDictionaryLookup))]
    public void SetupSorted()
    {
        Console.WriteLine("// GlobalSetup: SetupSorted (SortedDictionary)");
        CreateKeys();
        _sorted = new SortedDictionary<string, int>(_keys.ToDictionary(k => k, k => k.Length));
    }

    [Benchmark(Baseline = true)]
    public int DictionaryLookup()
    {
        int sum = 0;
        foreach (string key in _keys)
            sum += _dictionary[key];
        return sum;
    }

    [Benchmark]
    public int FrozenDictionaryLookup()
    {
        int sum = 0;
        foreach (string key in _keys)
            sum += _frozen[key];
        return sum;
    }

    [Benchmark]
    public int SortedDictionaryLookup()
    {
        int sum = 0;
        foreach (string key in _keys)
            sum += _sorted[key];
        return sum;
    }
}
