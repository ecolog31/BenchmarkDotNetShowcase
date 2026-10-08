using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.Memory;

/// <summary>
/// [MemoryDiagnoser] на прикладі class vs struct vs record struct: створюємо масив з 1 000 точок і сумуємо координати.
/// Чого очікувати: class — 1 000 окремих об'єктів у купі + масив посилань (Allocated ≈ 32 КБ, більше Gen0-збірок);
/// struct і record struct — один суцільний масив значень (≈ 8 КБ), швидше завдяки локальності даних.
/// record struct за продуктивністю не відрізняється від звичайного struct.
/// </summary>
[MemoryDiagnoser]
public class ClassVsStructBenchmarks
{
    private const int Count = 1_000;

    public sealed class PointClass(int x, int y)
    {
        public int X { get; } = x;
        public int Y { get; } = y;
    }

    public readonly struct PointStruct(int x, int y)
    {
        public int X { get; } = x;
        public int Y { get; } = y;
    }

    public readonly record struct PointRecordStruct(int X, int Y);

    [Benchmark(Baseline = true)]
    public long Class()
    {
        var points = new PointClass[Count];
        for (int i = 0; i < points.Length; i++)
            points[i] = new PointClass(i, -i);

        long sum = 0;
        foreach (var p in points)
            sum += p.X + p.Y * 2;
        return sum;
    }

    [Benchmark]
    public long Struct()
    {
        var points = new PointStruct[Count];
        for (int i = 0; i < points.Length; i++)
            points[i] = new PointStruct(i, -i);

        long sum = 0;
        foreach (var p in points)
            sum += p.X + p.Y * 2;
        return sum;
    }

    [Benchmark]
    public long RecordStruct()
    {
        var points = new PointRecordStruct[Count];
        for (int i = 0; i < points.Length; i++)
            points[i] = new PointRecordStruct(i, -i);

        long sum = 0;
        foreach (var p in points)
            sum += p.X + p.Y * 2;
        return sum;
    }
}
