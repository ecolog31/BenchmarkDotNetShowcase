using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Microsoft.Extensions.Logging;

namespace Showcase.Benchmarks.RealWorld;

/// <summary>
/// Ціна логування з Microsoft.Extensions.Logging: інтерполяція рядка vs шаблон з параметрами
/// vs [LoggerMessage] source generator. Дві категорії: рівень вимкнений (Debug при мінімумі Information —
/// найчастіший випадок у продакшні) і рівень увімкнений (логер-«поглинач», що нічого не пише).
/// Чого очікувати: при вимкненому рівні інтерполяція все одно форматує рядок і алокує (88 B),
/// шаблон — боксинг параметрів і масив params, а LoggerMessage — ~0 ns і 0 B (спочатку перевіряє IsEnabled).
/// При увімкненому рівні рядок однаково форматується, тож переваги в швидкості немає: інтерполяція тут навіть швидша,
/// шаблон — найдорожчий (боксинг + розбір шаблону). Сила LoggerMessage — вимкнені рівні і структуроване логування без боксингу.
/// </summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public partial class LoggingBenchmarks
{
    private readonly ILogger _disabledLogger = new SinkLogger(isEnabled: false);
    private readonly ILogger _enabledLogger = new SinkLogger(isEnabled: true);
    private readonly int _orderId = 1001;
    private readonly double _elapsedMs = 12.5;

    [LoggerMessage(Level = LogLevel.Debug, Message = "Order {OrderId} processed in {ElapsedMs} ms")]
    private static partial void LogOrderProcessed(ILogger logger, int orderId, double elapsedMs);

    [Benchmark(Baseline = true), BenchmarkCategory("Disabled")]
    public void Disabled_Interpolation() => _disabledLogger.LogDebug($"Order {_orderId} processed in {_elapsedMs} ms");

    [Benchmark, BenchmarkCategory("Disabled")]
    public void Disabled_MessageTemplate() => _disabledLogger.LogDebug("Order {OrderId} processed in {ElapsedMs} ms", _orderId, _elapsedMs);

    [Benchmark, BenchmarkCategory("Disabled")]
    public void Disabled_LoggerMessage() => LogOrderProcessed(_disabledLogger, _orderId, _elapsedMs);

    [Benchmark(Baseline = true), BenchmarkCategory("Enabled")]
    public void Enabled_Interpolation() => _enabledLogger.LogDebug($"Order {_orderId} processed in {_elapsedMs} ms");

    [Benchmark, BenchmarkCategory("Enabled")]
    public void Enabled_MessageTemplate() => _enabledLogger.LogDebug("Order {OrderId} processed in {ElapsedMs} ms", _orderId, _elapsedMs);

    [Benchmark, BenchmarkCategory("Enabled")]
    public void Enabled_LoggerMessage() => LogOrderProcessed(_enabledLogger, _orderId, _elapsedMs);

    /// <summary>Логер без виводу: при isEnabled = true форматує повідомлення (як справжній логер) і відкидає його.</summary>
    private sealed class SinkLogger(bool isEnabled) : ILogger
    {
        public int LastLength { get; private set; }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => isEnabled;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;
            LastLength = formatter(state, exception).Length;
        }
    }
}
