using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.Configuration;

/// <summary>
/// Експортери результатів: кожен атрибут додає файл у BenchmarkDotNet.Artifacts/results:
/// *-report-github.md (Markdown для GitHub), *-report.html, *-report.csv,
/// *-report-full.json, *-report-brief.xml, *-report.asciidoc, а [RPlotExporter] — *-measurements.csv і PNG-графіки через R
/// (потребує встановленого R і Rscript у PATH; без нього BenchmarkDotNet лише попередить).
/// Чого очікувати: у кінці виводу — список «Export: ...» з шляхами до всіх файлів.
/// </summary>
[MarkdownExporterAttribute.GitHub]
[HtmlExporter]
[CsvExporter]
[JsonExporterAttribute.Full]
[XmlExporterAttribute.Brief]
[AsciiDocExporter]
[RPlotExporter]
public class ExportersBenchmarks
{
    private readonly string[] _words = ["delta", "alpha", "charlie", "bravo", "echo", "foxtrot"];

    [Benchmark(Baseline = true)]
    public string[] SortOrdinal()
    {
        var copy = (string[])_words.Clone();
        Array.Sort(copy, StringComparer.Ordinal);
        return copy;
    }

    [Benchmark]
    public string[] SortInvariantCulture()
    {
        var copy = (string[])_words.Clone();
        Array.Sort(copy, StringComparer.InvariantCulture);
        return copy;
    }
}
