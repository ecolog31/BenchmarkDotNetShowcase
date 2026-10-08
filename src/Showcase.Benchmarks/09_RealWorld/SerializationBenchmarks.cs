using System.Text.Json;
using System.Text.Json.Serialization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using MessagePack;

namespace Showcase.Benchmarks.RealWorld;

/// <summary>
/// Серіалізація одного й того самого замовлення з 20 позиціями: System.Text.Json (рефлексія і source generator),
/// Newtonsoft.Json і MessagePack (бінарний формат). Дві категорії — Serialize і Deserialize, у кожній свій baseline.
/// Чого очікувати: STJ source-gen найшвидший з JSON і з найменшими алокаціями; Newtonsoft у 1.5–2 рази повільніший
/// і алокує більше; MessagePack — найкомпактніший результат і зазвичай найшвидший загалом.
/// </summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class SerializationBenchmarks
{
    private Order _order = null!;
    private string _json = "";
    private byte[] _messagePack = [];

    [GlobalSetup]
    public void Setup()
    {
        _order = new Order
        {
            Id = 1001,
            Customer = "Contoso Ltd.",
            CreatedAt = new DateTime(2026, 10, 8, 12, 0, 0, DateTimeKind.Utc),
            Lines = Enumerable.Range(1, 20)
                .Select(i => new OrderLine { Sku = $"SKU-{i:D4}", Quantity = i, UnitPrice = 9.99m * i })
                .ToList(),
        };
        _json = JsonSerializer.Serialize(_order, ShowcaseJsonContext.Default.Order);
        _messagePack = MessagePackSerializer.Serialize(_order);
    }

    [Benchmark(Baseline = true), BenchmarkCategory("Serialize")]
    public string StjReflectionSerialize() => JsonSerializer.Serialize(_order);

    [Benchmark, BenchmarkCategory("Serialize")]
    public string StjSourceGenSerialize() => JsonSerializer.Serialize(_order, ShowcaseJsonContext.Default.Order);

    [Benchmark, BenchmarkCategory("Serialize")]
    public string NewtonsoftSerialize() => Newtonsoft.Json.JsonConvert.SerializeObject(_order);

    [Benchmark, BenchmarkCategory("Serialize")]
    public byte[] MessagePackSerialize() => MessagePackSerializer.Serialize(_order);

    [Benchmark(Baseline = true), BenchmarkCategory("Deserialize")]
    public Order? StjReflectionDeserialize() => JsonSerializer.Deserialize<Order>(_json);

    [Benchmark, BenchmarkCategory("Deserialize")]
    public Order? StjSourceGenDeserialize() => JsonSerializer.Deserialize(_json, ShowcaseJsonContext.Default.Order);

    [Benchmark, BenchmarkCategory("Deserialize")]
    public Order? NewtonsoftDeserialize() => Newtonsoft.Json.JsonConvert.DeserializeObject<Order>(_json);

    [Benchmark, BenchmarkCategory("Deserialize")]
    public Order MessagePackDeserialize() => MessagePackSerializer.Deserialize<Order>(_messagePack);
}

[MessagePackObject(keyAsPropertyName: true)]
public class Order
{
    public int Id { get; set; }
    public string Customer { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public List<OrderLine> Lines { get; set; } = [];
}

[MessagePackObject(keyAsPropertyName: true)]
public class OrderLine
{
    public string Sku { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

[JsonSerializable(typeof(Order))]
internal partial class ShowcaseJsonContext : JsonSerializerContext;
