using System.Buffers;
using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.Memory;

/// <summary>
/// [MemoryDiagnoser] + ArrayPool&lt;T&gt;: тимчасовий буфер через new vs оренда з пулу.
/// Розміри обрано так, щоб показати різні покоління GC: 1 КБ — звичайна купа (Gen0),
/// 128 КБ — більше порогу 85 000 байт, тобто Large Object Heap, який збирається разом з Gen2.
/// Чого очікувати: ArrayPool — 0 B Allocated на обох розмірах; new byte[1 КБ] — дешево, але з Gen0;
/// new byte[128 КБ] — помітні Gen2-збірки (колонка Gen2) і значно повільніше.
/// </summary>
[MemoryDiagnoser]
public class ArrayPoolBenchmarks
{
    [Params(1_024, 128 * 1_024)]
    public int Size { get; set; }

    [Benchmark(Baseline = true)]
    public byte NewArray()
    {
        byte[] buffer = new byte[Size];
        return Process(buffer, Size);
    }

    [Benchmark]
    public byte RentFromPool()
    {
        byte[] buffer = ArrayPool<byte>.Shared.Rent(Size);
        try
        {
            return Process(buffer, Size);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    // Орендований масив може бути більшим за запитаний розмір, тому працюємо лише з першими size байтами.
    private static byte Process(byte[] buffer, int size)
    {
        Span<byte> span = buffer.AsSpan(0, size);
        span.Fill(1);
        return span[^1];
    }
}
