#if NET9_0_OR_GREATER
using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.DotNet10;

/// <summary>
/// System.Threading.Lock (.NET 9+) замість lock(object). Компілятор C# 13 для lock(Lock) генерує
/// Lock.EnterScope() замість Monitor.Enter/Exit — легший шлях без об'єктного заголовка й sync block.
/// Класу немає в збірці під net8.0 (#if NET9_0_OR_GREATER), тому порівняння — лише .NET 9 vs .NET 10.
/// Чого очікувати (виміряно): на .NET 9 Lock на 25% швидший за lock(object); у .NET 10 Monitor підтягнувся (−16%),
/// але Lock усе одно трохи швидший.
/// </summary>
[Config(typeof(DotNet9And10Config))]
public class LockTypeBenchmarks
{
    private readonly object _monitorLock = new();
    private readonly Lock _lock = new();
    private int _counter;

    [Benchmark(Baseline = true)]
    public int LockOnObject()
    {
        lock (_monitorLock)
            return ++_counter;
    }

    [Benchmark]
    public int LockOnLockType()
    {
        lock (_lock)
            return ++_counter;
    }
}
#endif
