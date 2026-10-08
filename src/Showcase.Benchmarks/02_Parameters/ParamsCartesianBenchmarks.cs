using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.Parameters;

/// <summary>
/// [Params] з різними типами: enum (StringComparison), bool і рядки.
/// Кілька [Params] утворюють декартів добуток: 3 × 2 × 2 = 12 запусків одного методу,
/// кожна комбінація — окремий рядок таблиці з колонками Comparison / SameCase / Length.
/// Чого очікувати: Ordinal найшвидший; OrdinalIgnoreCase дорожчий лише трохи;
/// InvariantCulture значно повільніший (культурне порівняння через ICU/NLS);
/// для довших рядків різниця зростає.
/// </summary>
public class ParamsCartesianBenchmarks
{
    private string _left = "";
    private string _right = "";

    [Params(StringComparison.Ordinal, StringComparison.OrdinalIgnoreCase, StringComparison.InvariantCulture)]
    public StringComparison Comparison { get; set; }

    /// <summary>true — рядки ідентичні; false — відрізняються лише регістром.</summary>
    [Params(true, false)]
    public bool SameCase { get; set; }

    // Рядки в [Params] показуються в таблиці як є, тому тут короткі мітки, а не самі дані.
    [Params("short", "long")]
    public string Length { get; set; } = "short";

    [GlobalSetup]
    public void Setup()
    {
        string text = Length == "short"
            ? "Hello, BenchmarkDotNet"
            : string.Concat(Enumerable.Repeat("Hello, BenchmarkDotNet! ", 40));
        _left = text;
        _right = SameCase ? new string(text.AsSpan()) : text.ToUpperInvariant();
    }

    [Benchmark]
    public bool StringEquals() => string.Equals(_left, _right, Comparison);
}
