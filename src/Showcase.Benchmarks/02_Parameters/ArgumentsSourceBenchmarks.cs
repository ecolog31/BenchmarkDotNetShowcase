using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace Showcase.Benchmarks.Parameters;

/// <summary>
/// [ArgumentsSource]: аргументи методу беруться з методу/властивості, що повертає IEnumerable.
/// Для одного параметра достатньо IEnumerable&lt;T&gt;; для кількох — IEnumerable&lt;object[]&gt;.
/// Як і в ParamsSource, у таблиці показується ToString() аргументу.
/// Приклад: пошук підрядка в 10 000-символьному тексті — ordinal (векторизований) vs культурний.
/// Чого очікувати: час росте з позицією збігу; Ordinal на порядок-два швидший за InvariantCulture.
/// </summary>
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByParams)]
public class ArgumentsSourceBenchmarks
{
    public sealed record SearchCase(string Name, string Text, string Needle)
    {
        public override string ToString() => Name;
    }

    public static IEnumerable<SearchCase> Cases()
    {
        string filler = new('a', 10_000);
        yield return new("AtStart", "needle" + filler, "needle");
        yield return new("InMiddle", filler[..5_000] + "needle" + filler[5_000..], "needle");
        yield return new("AtEnd", filler + "needle", "needle");
        yield return new("Missing", filler, "needle");
    }

    [Benchmark(Baseline = true)]
    [ArgumentsSource(nameof(Cases))]
    public int IndexOfOrdinal(SearchCase searchCase) =>
        searchCase.Text.IndexOf(searchCase.Needle, StringComparison.Ordinal);

    [Benchmark]
    [ArgumentsSource(nameof(Cases))]
    public int IndexOfInvariantCulture(SearchCase searchCase) =>
        searchCase.Text.IndexOf(searchCase.Needle, StringComparison.InvariantCulture);
}
