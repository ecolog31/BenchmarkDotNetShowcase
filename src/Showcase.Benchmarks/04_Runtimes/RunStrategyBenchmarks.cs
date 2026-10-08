using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

namespace Showcase.Benchmarks.Runtimes;

/// <summary>
/// Стратегії запуску (RunStrategy):
/// Throughput (за замовчуванням) — прогрів, підбір кількості викликів, мільйони повторень: «стабільна» швидкість;
/// ColdStart — без прогріву, один виклик на ітерацію: видно ціну першого виклику (JIT, ініціалізація);
/// Monitoring — без прогріву й без віднімання overhead, для довгих макро-сценаріїв.
/// launchCount = кількість окремих процесів; для ColdStart кожен процес дає новий «холодний» перший виклик.
/// Приклад: System.Text.Json серіалізація — перший виклик будує метадані типу, далі дешево.
/// Чого очікувати: Throughput — мікросекунди або менше; ColdStart — значно більше середнє через першу
/// ітерацію кожного процесу (дивіться Max/Min у логах); Monitoring — між ними.
/// </summary>
[SimpleJob(RunStrategy.Throughput, id: "Throughput")]
[SimpleJob(RunStrategy.ColdStart, launchCount: 5, iterationCount: 1, id: "ColdStart")]
[SimpleJob(RunStrategy.Monitoring, iterationCount: 20, id: "Monitoring")]
[MinColumn, MaxColumn]
public class RunStrategyBenchmarks
{
    public sealed record Order(int Id, string Customer, decimal Total, DateTime CreatedAt, string[] Tags);

    private readonly Order _order = new(42, "Contoso", 199.99m, new DateTime(2026, 1, 1), ["priority", "gift"]);

    [Benchmark]
    public string SerializeToJson() => JsonSerializer.Serialize(_order);
}
