using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.RealWorld;

/// <summary>
/// Три способи виконати той самий Regex (пошук email-адрес у тексті ~10 КБ):
/// Interpreted — інтерпретатор (дешеве створення, повільне виконання);
/// Compiled (RegexOptions.Compiled) — генерація IL під час виконання (дороге створення, швидке виконання);
/// [GeneratedRegex] — source generator: код генерується під час компіляції проєкту (немає ціни створення, сумісний з AOT).
/// Об'єкти Regex створюються в полях/статично — тут вимірюється лише пошук, не створення.
/// Чого очікувати: Compiled і GeneratedRegex близькі між собою й у кілька разів швидші за Interpreted.
/// </summary>
public partial class RegexModesBenchmarks
{
    private const string Pattern = @"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,}";

    private readonly Regex _interpreted = new(Pattern, RegexOptions.IgnoreCase);
    private readonly Regex _compiled = new(Pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private string _text = "";

    [GeneratedRegex(Pattern, RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    [GlobalSetup]
    public void Setup() =>
        _text = string.Concat(Enumerable.Range(0, 200).Select(i =>
            i % 10 == 0 ? $"contact user{i}@example.com for details. " : "lorem ipsum dolor sit amet, "));

    [Benchmark(Baseline = true)]
    public int Interpreted() => _interpreted.Count(_text);

    [Benchmark]
    public int Compiled() => _compiled.Count(_text);

    [Benchmark]
    public int SourceGenerated() => EmailRegex().Count(_text);
}
