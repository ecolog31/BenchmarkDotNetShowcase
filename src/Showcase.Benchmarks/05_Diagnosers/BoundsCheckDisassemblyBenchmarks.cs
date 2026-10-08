using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

namespace Showcase.Benchmarks.Diagnosers;

/// <summary>
/// [DisassemblyDiagnoser]: показує машинний код, який JIT згенерував для кожного бенчмарка.
/// printSource — рядки C# поруч з asm; syntax — Intel/Masm/Att; exportGithubMarkdown / exportHtml —
/// окремі файли *-asm.md / *-asm.html у BenchmarkDotNet.Artifacts/results; exportDiff — diff між методами.
/// Приклад: bounds-check elimination (BCE). JIT прибирає перевірку меж масиву, коли може довести,
/// що індекс у межах (умова циклу i &lt; array.Length по локальній змінній).
/// Чого очікувати в asm: у LocalArrayToLength немає перевірки меж у циклі. У LocalArrayToSeparateCount JIT
/// робить loop cloning: один раз перед циклом перевіряє count ≤ Length і йде «швидкою» копією циклу без перевірок
/// (повільна копія з CORINFO_HELP_RNGCHKFAIL лишається про запас) — швидкість та сама, код більший.
/// У FieldArray перевірка меж і перечитування довжини з поля — в кожній ітерації; це пояснює for vs foreach з 01_Basics.
/// </summary>
[DisassemblyDiagnoser(maxDepth: 1, syntax: DisassemblySyntax.Intel, printSource: true,
    exportGithubMarkdown: true, exportHtml: true, exportDiff: true)]
public class BoundsCheckDisassemblyBenchmarks
{
    private int[] _data = [];
    private int _count;

    [GlobalSetup]
    public void Setup()
    {
        _data = Enumerable.Range(0, 10_000).ToArray();
        _count = _data.Length;
    }

    [Benchmark(Baseline = true)]
    public int LocalArrayToLength()
    {
        int[] data = _data;
        int sum = 0;
        for (int i = 0; i < data.Length; i++)
            sum += data[i];
        return sum;
    }

    [Benchmark]
    public int LocalArrayToSeparateCount()
    {
        int[] data = _data;
        int count = _count;
        int sum = 0;
        for (int i = 0; i < count; i++)
            sum += data[i];
        return sum;
    }

    [Benchmark]
    public int FieldArray()
    {
        int sum = 0;
        for (int i = 0; i < _data.Length; i++)
            sum += _data[i];
        return sum;
    }
}
