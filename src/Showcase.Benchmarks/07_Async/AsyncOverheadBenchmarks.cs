using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.Async;

/// <summary>
/// Накладні витрати async/await: обгортка, що робить «await Inner()», vs пряме повернення Task (без state machine),
/// і ConfigureAwait(false). Inner справді асинхронний (Task.Yield), тож продовження йде через ThreadPool.
/// Чого очікувати: пряме повернення Task трохи швидше й алокує менше (немає власної state machine обгортки);
/// ConfigureAwait(false) у процесі бенчмарка майже не впливає — тут немає SynchronizationContext
/// (на відміну від UI-застосунків, де різниця суттєва).
/// Пам'ятайте: пряме повернення Task змінює поведінку винятків і using-блоків — це не безкоштовна оптимізація.
/// </summary>
[MemoryDiagnoser]
public class AsyncOverheadBenchmarks
{
    [Benchmark(Baseline = true)]
    public async Task<int> AsyncAwaitWrapper() => await InnerAsync();

    [Benchmark]
    public Task<int> DirectTaskReturn() => InnerAsync();

    [Benchmark]
    public async Task<int> AsyncAwaitConfigureAwaitFalse() => await InnerAsync().ConfigureAwait(false);

    private static async Task<int> InnerAsync()
    {
        await Task.Yield();
        return 42;
    }
}
