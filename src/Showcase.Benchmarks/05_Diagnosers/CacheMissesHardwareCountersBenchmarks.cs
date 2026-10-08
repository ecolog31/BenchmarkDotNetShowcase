using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

namespace Showcase.Benchmarks.Diagnosers;

/// <summary>
/// [HardwareCounters] з CacheMisses: обхід 2D-масиву 2048×2048 int (16 МБ — більше за L3) по рядках vs по стовпцях.
/// У .NET прямокутний масив зберігається по рядках, тож обхід по рядках читає пам'ять послідовно,
/// а по стовпцях — стрибає на 8 КБ при кожному кроці.
/// Чого очікувати: по стовпцях — на порядки більше CacheMisses/op і в рази повільніше.
///
/// ⚠ Потребує запуску від АДМІНІСТРАТОРА і фізичної машини:
///   dotnet run -c Release -f net10.0 -- --filter "*HardwareCounters*" --job short
/// </summary>
[HardwareCounters(HardwareCounter.CacheMisses)]
public class CacheMissesHardwareCountersBenchmarks
{
    private const int N = 2_048;
    private readonly int[,] _matrix = new int[N, N];

    [GlobalSetup]
    public void Setup()
    {
        for (int row = 0; row < N; row++)
            for (int column = 0; column < N; column++)
                _matrix[row, column] = row ^ column;
    }

    [Benchmark(Baseline = true)]
    public long RowMajor()
    {
        long sum = 0;
        for (int row = 0; row < N; row++)
            for (int column = 0; column < N; column++)
                sum += _matrix[row, column];
        return sum;
    }

    [Benchmark]
    public long ColumnMajor()
    {
        long sum = 0;
        for (int column = 0; column < N; column++)
            for (int row = 0; row < N; row++)
                sum += _matrix[row, column];
        return sum;
    }
}
