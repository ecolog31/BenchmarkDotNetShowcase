using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

namespace Showcase.Benchmarks.Diagnosers;

/// <summary>
/// SIMD + [DisassemblyDiagnoser]: сума float-масиву скалярно, через Vector&lt;T&gt; і через Vector256.
/// Vector&lt;T&gt; має ширину, яку обирає рантайм (на AVX2 — 256 біт = 8 float);
/// Vector256 — явна ширина. Vector256.IsHardwareAccelerated / Vector512.IsHardwareAccelerated
/// виводяться в GlobalSetup (див. лог), бо від них залежить, чи взагалі буде прискорення.
/// Чого очікувати: у asm скалярної версії — vaddss (одне число), у векторних — vaddps з ymm-регістрами
/// (8 чисел за інструкцію); векторні версії в кілька разів швидші.
/// </summary>
[DisassemblyDiagnoser(maxDepth: 2, syntax: DisassemblySyntax.Intel, exportGithubMarkdown: true)]
public class SimdDisassemblyBenchmarks
{
    private float[] _values = [];

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        _values = Enumerable.Range(0, 4_096).Select(_ => (float)random.NextDouble()).ToArray();
        Console.WriteLine($"// Vector<float>.Count = {Vector<float>.Count}, " +
                          $"Vector256.IsHardwareAccelerated = {Vector256.IsHardwareAccelerated}, " +
                          $"Vector512.IsHardwareAccelerated = {Vector512.IsHardwareAccelerated}");
    }

    [Benchmark(Baseline = true)]
    public float Scalar()
    {
        float sum = 0;
        foreach (float value in _values)
            sum += value;
        return sum;
    }

    [Benchmark]
    public float VectorT()
    {
        ReadOnlySpan<float> values = _values;
        var accumulator = Vector<float>.Zero;
        int i = 0;
        for (; i <= values.Length - Vector<float>.Count; i += Vector<float>.Count)
            accumulator += new Vector<float>(values[i..]);

        float sum = Vector.Sum(accumulator);
        for (; i < values.Length; i++)
            sum += values[i];
        return sum;
    }

    [Benchmark]
    public float Vector256Explicit()
    {
        ReadOnlySpan<float> values = _values;
        ref float start = ref MemoryMarshal.GetReference(values);
        var accumulator = Vector256<float>.Zero;
        int i = 0;
        for (; i <= values.Length - Vector256<float>.Count; i += Vector256<float>.Count)
            accumulator += Vector256.LoadUnsafe(ref start, (nuint)i);

        float sum = Vector256.Sum(accumulator);
        for (; i < values.Length; i++)
            sum += values[i];
        return sum;
    }
}
