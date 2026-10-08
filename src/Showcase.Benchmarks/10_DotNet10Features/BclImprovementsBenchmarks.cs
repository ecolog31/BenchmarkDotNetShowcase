using System.Buffers;
using System.Collections.Frozen;
using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.DotNet10;

/// <summary>
/// Типові операції BCL, які прискорювалися від версії до версії: string.Join, string.Contains,
/// SearchValues&lt;char&gt; (пошук будь-якого з набору символів), FrozenDictionary і GeneratedRegex.
/// Код однаковий для всіх рантаймів — різниця лише в реалізації BCL і JIT.
/// Чого очікувати (виміряно): string.Join на .NET 10 −39%, Contains(OrdinalIgnoreCase) −27%; SearchValues і GeneratedRegex −8%;
/// FrozenDictionary без змін. .NET 9 тут майже не відрізняється від .NET 8.
/// </summary>
[Config(typeof(DotNetVersionsConfig))]
public partial class BclImprovementsBenchmarks
{
    private string[] _words = [];
    private string _text = "";
    private FrozenDictionary<string, int> _frozen = FrozenDictionary<string, int>.Empty;
    // SearchValues<char> є з .NET 8; SearchValues<string> (кілька підрядків) — лише з .NET 9, тому тут не підходить.
    private static readonly SearchValues<char> s_specialChars = SearchValues.Create("<>&\"'{}[]");

    [GeneratedRegex(@"\b\d{4}-\d{2}-\d{2}\b")]
    private static partial Regex DateRegex();

    [GlobalSetup]
    public void Setup()
    {
        _words = Enumerable.Range(0, 100).Select(i => $"word{i}").ToArray();
        _text = string.Concat(Enumerable.Repeat("INFO request handled in 12 ms; ", 300)) + "ERROR Deadlock detected at 2026-10-08 {retry}";
        _frozen = _words.Select((w, i) => (w, i)).ToFrozenDictionary(p => p.w, p => p.i);
    }

    [Benchmark]
    public string StringJoin() => string.Join(", ", _words);

    [Benchmark]
    public bool StringContainsIgnoreCase() => _text.Contains("deadlock", StringComparison.OrdinalIgnoreCase);

    [Benchmark]
    public int SearchValuesIndexOfAny() => _text.AsSpan().IndexOfAny(s_specialChars);

    [Benchmark]
    public int FrozenDictionaryLookups()
    {
        int sum = 0;
        foreach (string word in _words)
            sum += _frozen[word];
        return sum;
    }

    [Benchmark]
    public bool GeneratedRegexIsMatch() => DateRegex().IsMatch(_text);
}
