using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using Newtonsoft.Json;

namespace Showcase.Benchmarks.Advanced;

/// <summary>
/// Порівняння двох версій одного NuGet-пакета (Newtonsoft.Json 12.0.3 vs 13.0.3) в одному запуску.
/// Старий API WithNuGet / [NuGetReference] у BenchmarkDotNet 0.14+ видалено; рекомендований спосіб —
/// версія пакета як MSBuild-властивість у csproj (Version="$(NewtonsoftJsonVersion)") і окремий job
/// з WithMsBuildArguments("/p:NewtonsoftJsonVersion=..."). BenchmarkDotNet збирає проєкт двічі з різними версіями.
/// Чого очікувати: колонка Job з назвами версій; у цьому сценарії різниця між 12.0.3 і 13.0.3 невелика
/// (13.x переважно змінював безпекові налаштування, а не швидкість) — і це теж корисний результат.
/// Запускати без --job short (job-и задані в класі).
/// </summary>
[Config(typeof(NuGetVersionsConfig))]
[MemoryDiagnoser]
public class NuGetVersionsBenchmarks
{
    public sealed class Order
    {
        public int Id { get; set; }
        public string Customer { get; set; } = "";
        public decimal Total { get; set; }
        public List<string> Items { get; set; } = [];
    }

    private Order _order = new();
    private string _json = "";

    [GlobalSetup]
    public void Setup()
    {
        _order = new Order { Id = 42, Customer = "Contoso", Total = 199.99m, Items = ["book", "pen", "lamp"] };
        _json = JsonConvert.SerializeObject(_order);
        Console.WriteLine($"// Newtonsoft.Json loaded: {typeof(JsonConvert).Assembly.GetName().Version}");
    }

    [Benchmark]
    public string Serialize() => JsonConvert.SerializeObject(_order);

    [Benchmark]
    public Order? Deserialize() => JsonConvert.DeserializeObject<Order>(_json);

    private sealed class NuGetVersionsConfig : ManualConfig
    {
        public NuGetVersionsConfig()
        {
            Job baseJob = Job.ShortRun;
            AddJob(baseJob.WithMsBuildArguments("/p:NewtonsoftJsonVersion=12.0.3").WithId("Newtonsoft 12.0.3").AsBaseline());
            AddJob(baseJob.WithMsBuildArguments("/p:NewtonsoftJsonVersion=13.0.3").WithId("Newtonsoft 13.0.3"));
        }
    }
}
