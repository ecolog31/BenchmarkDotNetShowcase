using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;

namespace Showcase.Benchmarks.Basics;

/// <summary>
/// Керування порядком і рангами в підсумковій таблиці:
/// [Orderer(SummaryOrderPolicy.FastestToSlowest)] сортує рядки від найшвидшого,
/// [RankColumn] додає колонку Rank (однаковий ранг, якщо різниця статистично незначуща),
/// [GroupBenchmarksBy(ByParams)] розбиває таблицю на логічні групи за значенням Count
/// (без нього ранги й сортування наскрізні для всіх параметрів),
/// [LogicalGroupColumn] показує, до якої логічної групи належить рядок.
/// Приклад: три способи порахувати кількість одиничних бітів у випадкових uint.
/// Чого очікувати: BitOperations.PopCount (апаратна інструкція POPCNT) — Rank 1 в обох групах.
/// Кернігана vs наївний цикл залежить від розміру: цикл Кернігана має змінну кількість ітерацій,
/// і на 1 000 елементів (ті самі дані щоразу) передбачувач переходів їх «запам'ятовує»,
/// а на 100 000 — ні, тож Кернігана може стати повільнішим за наївний цикл (див. 05_Diagnosers).
/// </summary>
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByParams)]
[RankColumn]
[LogicalGroupColumn]
public class OrderingAndRankBenchmarks
{
    private uint[] _values = [];

    // [Params] детально розбирається в розділі 02_Parameters; тут він потрібен,
    // щоб утворилися дві логічні групи для LogicalGroupColumn.
    [Params(1_000, 100_000)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        _values = Enumerable.Range(0, Count).Select(_ => (uint)random.Next()).ToArray();
    }

    [Benchmark]
    public int NaiveLoop()
    {
        int total = 0;
        foreach (uint value in _values)
            for (int bit = 0; bit < 32; bit++)
                total += (int)((value >> bit) & 1);
        return total;
    }

    [Benchmark]
    public int Kernighan()
    {
        int total = 0;
        foreach (uint value in _values)
        {
            uint v = value;
            while (v != 0)
            {
                v &= v - 1;
                total++;
            }
        }
        return total;
    }

    [Benchmark]
    public int HardwarePopCount()
    {
        int total = 0;
        foreach (uint value in _values)
            total += BitOperations.PopCount(value);
        return total;
    }
}
