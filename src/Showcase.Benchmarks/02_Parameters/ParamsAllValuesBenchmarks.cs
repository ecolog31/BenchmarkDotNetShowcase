using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace Showcase.Benchmarks.Parameters;

/// <summary>
/// [ParamsAllValues]: BenchmarkDotNet сам перебирає всі значення типу параметра —
/// працює для bool, enum і їхніх nullable-версій (bool? дає true / false / null).
/// Тут — усі значення enum DataPattern без ручного переліку в [Params].
/// Приклад: умовна сума (елементи ≥ 128) з розгалуженням vs без розгалуження.
/// Чого очікувати: Branchy швидкий на Sorted і AllSame (передбачуваний перехід) і повільний на Random;
/// Branchless однаковий на всіх шаблонах. Якщо JIT сам перетворить if на cmov, різниці не буде.
/// </summary>
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByParams)]
public class ParamsAllValuesBenchmarks
{
    public enum DataPattern { Random, Sorted, Alternating, AllSame }

    private const int Size = 32_768;
    private byte[] _data = [];

    [ParamsAllValues]
    public DataPattern Pattern { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        _data = Pattern switch
        {
            DataPattern.Random => Enumerable.Range(0, Size).Select(_ => (byte)random.Next(256)).ToArray(),
            DataPattern.Sorted => Enumerable.Range(0, Size).Select(_ => (byte)random.Next(256)).Order().ToArray(),
            DataPattern.Alternating => Enumerable.Range(0, Size).Select(i => (byte)(i % 2 == 0 ? 0 : 255)).ToArray(),
            DataPattern.AllSame => Enumerable.Repeat((byte)200, Size).ToArray(),
            _ => throw new ArgumentOutOfRangeException(nameof(Pattern)),
        };
    }

    [Benchmark(Baseline = true)]
    public long Branchy()
    {
        long sum = 0;
        foreach (byte b in _data)
            if (b >= 128)
                sum += b;
        return sum;
    }

    [Benchmark]
    public long Branchless()
    {
        long sum = 0;
        foreach (byte b in _data)
        {
            // маска = -1 (усі біти 1), якщо b >= 128, інакше 0
            int mask = (127 - b) >> 31;
            sum += b & mask;
        }
        return sum;
    }
}
