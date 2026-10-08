using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.Async;

/// <summary>
/// Task&lt;T&gt; vs ValueTask&lt;T&gt; vs Task (без результату) у типовому «гарячому» сценарії: async-метод, який
/// зазвичай завершується синхронно (значення вже в кеші). BenchmarkDotNet сам чекає (await) на
/// Task/ValueTask, що повертаються з бенчмарка.
/// Чого очікувати: Task&lt;int&gt; з довільним результатом алокує об'єкт Task на кожен виклик (~72 B);
/// ValueTask&lt;int&gt; при синхронному завершенні — 0 B; Task без результату — 0 B (кешований Task.CompletedTask).
/// </summary>
[MemoryDiagnoser]
public class TaskVsValueTaskBenchmarks
{
    private readonly Dictionary<int, int> _cache = Enumerable.Range(0, 100).ToDictionary(i => i, i => i * i + 1_000);
    private int _key;

    [Benchmark(Baseline = true)]
    public Task<int> TaskOfT() => GetWithTaskAsync(NextKey());

    [Benchmark]
    public ValueTask<int> ValueTaskOfT() => GetWithValueTaskAsync(NextKey());

    [Benchmark]
    public Task TaskWithoutResult() => TouchAsync(NextKey());

    private int NextKey() => _key = (_key + 1) % 100;

    private async Task<int> GetWithTaskAsync(int key)
    {
        if (_cache.TryGetValue(key, out int value))
            return value;
        await Task.Delay(1);
        return -1;
    }

    private async ValueTask<int> GetWithValueTaskAsync(int key)
    {
        if (_cache.TryGetValue(key, out int value))
            return value;
        await Task.Delay(1);
        return -1;
    }

    private async Task TouchAsync(int key)
    {
        if (_cache.ContainsKey(key))
            return;
        await Task.Delay(1);
    }
}
