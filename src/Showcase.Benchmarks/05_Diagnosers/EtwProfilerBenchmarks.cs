using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;

namespace Showcase.Benchmarks.Diagnosers;

/// <summary>
/// [EtwProfiler] (пакет BenchmarkDotNet.Diagnostics.Windows): під час бенчмарка записує ETW-трасу
/// (CPU-семпли зі стеками) у файл *.etl у BenchmarkDotNet.Artifacts. Таблиця результатів звичайна,
/// цінність — у трасі: її відкривають у PerfView або Windows Performance Analyzer і дивляться,
/// які функції забирають час.
/// Чого очікувати: у трасі для ParseAndSerialize — гарячі методи System.Text.Json (Utf8JsonReader/Writer).
///
/// ⚠ Потребує запуску від АДМІНІСТРАТОРА (ETW-сесія ядра):
///   dotnet run -c Release -f net10.0 -- --filter "*EtwProfiler*" --job short
/// </summary>
[EtwProfiler]
public class EtwProfilerBenchmarks
{
    private string _json = "";

    [GlobalSetup]
    public void Setup()
    {
        var items = Enumerable.Range(0, 100).Select(i => new { Id = i, Name = $"item-{i}", Price = i * 1.5m });
        _json = JsonSerializer.Serialize(items);
    }

    [Benchmark]
    public string ParseAndSerialize()
    {
        using JsonDocument document = JsonDocument.Parse(_json);
        return JsonSerializer.Serialize(document.RootElement);
    }
}
