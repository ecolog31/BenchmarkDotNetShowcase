using System.Text.Json;
using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.Setup;

/// <summary>
/// [GlobalSetup] / [GlobalCleanup]: виконуються один раз на процес бенчмарка — до всіх ітерацій і після них.
/// Тут GlobalSetup створює тимчасовий JSON-файл з 1 000 записів, а GlobalCleanup його видаляє,
/// тож у вимір потрапляє лише читання й десеріалізація, а не підготовка даних.
/// Чого очікувати: десеріалізація зі Stream помітно повільніша (~1.7×) за десеріалізацію вже прочитаних байтів
/// (додається файловий I/O, хоча файл після першого читання в кеші ОС); алокації — сам список об'єктів.
/// </summary>
[MemoryDiagnoser]
public class FileSerializationSetupBenchmarks
{
    public sealed record Product(int Id, string Name, decimal Price, string[] Tags);

    private string _filePath = "";
    private byte[] _bytes = [];

    [GlobalSetup]
    public void Setup()
    {
        var products = Enumerable.Range(0, 1_000)
            .Select(i => new Product(i, $"Product {i}", i * 1.25m, [$"tag{i % 10}", "showcase"]))
            .ToList();
        _filePath = Path.Combine(Path.GetTempPath(), $"bdn-showcase-{Guid.NewGuid():N}.json");
        File.WriteAllText(_filePath, JsonSerializer.Serialize(products));
        _bytes = File.ReadAllBytes(_filePath);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (File.Exists(_filePath))
            File.Delete(_filePath);
    }

    [Benchmark(Baseline = true)]
    public List<Product>? DeserializeFromBytes() => JsonSerializer.Deserialize<List<Product>>(_bytes);

    [Benchmark]
    public List<Product>? DeserializeFromFile()
    {
        using FileStream stream = File.OpenRead(_filePath);
        return JsonSerializer.Deserialize<List<Product>>(stream);
    }
}
