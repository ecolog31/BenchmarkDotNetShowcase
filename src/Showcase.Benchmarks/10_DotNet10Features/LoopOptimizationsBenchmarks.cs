using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

namespace Showcase.Benchmarks.DotNet10;

/// <summary>
/// Оптимізації циклів у JIT між версіями + [DisassemblyDiagnoser], щоб порівняти машинний код .NET 8/9/10.
/// ReverseLoop: цикл від кінця масиву до початку — історично JIT не прибирав для нього bounds check.
/// SpanSliceLoop: цикл по зрізу Span — перевірка меж і похідні змінні індукції (strength reduction у .NET 9+).
/// StridedLoop: крок 2 — ще один випадок, де старіший JIT залишав перевірки меж.
/// Результат вимірів: різниці між .NET 8/9/10 тут НЕМАЄ (Code Size 52–88 B у всіх версіях) — ці прості цикли JIT
/// оптимізував уже в .NET 8. Це теж корисний висновок: «новіший рантайм» не означає прискорення кожного шматка коду.
/// asm для порівняння — у BenchmarkDotNet.Artifacts/results/*-asm.md.
/// </summary>
[Config(typeof(DotNetVersionsConfig))]
[DisassemblyDiagnoser(maxDepth: 1, exportGithubMarkdown: true, printSource: true)]
public class LoopOptimizationsBenchmarks
{
    private int[] _data = [];

    [GlobalSetup]
    public void Setup() => _data = Enumerable.Range(0, 10_000).ToArray();

    [Benchmark]
    public long ReverseLoop()
    {
        int[] data = _data;
        long sum = 0;
        for (int i = data.Length - 1; i >= 0; i--)
            sum += data[i];
        return sum;
    }

    [Benchmark]
    public long SpanSliceLoop()
    {
        ReadOnlySpan<int> span = _data.AsSpan(100, 9_800);
        long sum = 0;
        for (int i = 0; i < span.Length; i++)
            sum += span[i];
        return sum;
    }

    [Benchmark]
    public long StridedLoop()
    {
        int[] data = _data;
        long sum = 0;
        for (int i = 0; i < data.Length; i += 2)
            sum += data[i];
        return sum;
    }
}
