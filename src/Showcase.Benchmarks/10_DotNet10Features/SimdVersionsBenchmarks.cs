using System.Numerics;
using System.Runtime.Intrinsics;
using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.DotNet10;

/// <summary>
/// SIMD між версіями: ті самі векторизовані API BCL (Span.IndexOfAny, Count, SequenceEqual) і
/// ручний Vector&lt;T&gt; на .NET 8/9/10. Апаратна підтримка друкується в GlobalSetup:
/// Vector128/256/512.IsHardwareAccelerated — на процесорах без AVX-512 (наприклад, AMD Zen 3) Vector512 = False,
/// і .NET 10 використовує 256-бітні шляхи.
/// Чого очікувати: усе в межах наносекунд-мікросекунд; приріст між версіями залежить від методу —
/// BCL постійно розширює векторизацію (особливо Count і IndexOfAny з кількома значеннями).
/// </summary>
[Config(typeof(DotNetVersionsConfig))]
public class SimdVersionsBenchmarks
{
    private byte[] _bytes = [];
    private byte[] _bytesCopy = [];
    private int[] _ints = [];

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        _bytes = new byte[64 * 1_024];
        random.NextBytes(_bytes);
        for (int i = 0; i < _bytes.Length; i++)
            if (_bytes[i] is (byte)'\n' or (byte)'\r' or 0) _bytes[i] = 1;
        _bytes[^1] = (byte)'\n';
        _bytesCopy = (byte[])_bytes.Clone();
        _ints = Enumerable.Range(0, 16_384).ToArray();

        Console.WriteLine($"// Vector128={Vector128.IsHardwareAccelerated}, Vector256={Vector256.IsHardwareAccelerated}, " +
                          $"Vector512={Vector512.IsHardwareAccelerated}, Vector<byte>.Count={Vector<byte>.Count}");
    }

    [Benchmark]
    public int IndexOfAnyNewLine() => _bytes.AsSpan().IndexOfAny((byte)'\n', (byte)'\r', (byte)0);

    [Benchmark]
    public int CountByte() => _bytes.AsSpan().Count((byte)1);

    [Benchmark]
    public bool SequenceEqual() => _bytes.AsSpan().SequenceEqual(_bytesCopy);

    [Benchmark]
    public int VectorTSum()
    {
        ReadOnlySpan<int> values = _ints;
        var accumulator = Vector<int>.Zero;
        int i = 0;
        for (; i <= values.Length - Vector<int>.Count; i += Vector<int>.Count)
            accumulator += new Vector<int>(values[i..]);
        int sum = Vector.Sum(accumulator);
        for (; i < values.Length; i++)
            sum += values[i];
        return sum;
    }
}
