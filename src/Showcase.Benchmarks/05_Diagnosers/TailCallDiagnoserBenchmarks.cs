using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;

namespace Showcase.Benchmarks.Diagnosers;

/// <summary>
/// [TailCallDiagnoser] (пакет BenchmarkDotNet.Diagnostics.Windows, лише x64): виводить, які виклики
/// JIT перетворив на хвостові (jmp замість call — без нового кадру стеку), а які ні і чому.
/// C# не позначає хвостові виклики явно (на відміну від F# з префіксом tail.), але x64 JIT
/// може зробити «fast tail call» сам, якщо сигнатури сумісні.
/// Результат (виміряно): для SumTailRecursive JIT повідомив «Tail call type: RecursiveLoop» — рекурсію перетворено
/// на звичайний цикл (і OptimizedTailCall для першого виклику); для SumNotTail подій немає. TailRecursive у 3.4 раза
/// швидший (233 ns проти 799 ns на 1 000 рівнів) — немає 1 000 кадрів стеку.
///
/// ⚠ Потребує запуску від АДМІНІСТРАТОРА (ETW-сесія):
///   dotnet run -c Release -f net10.0 -- --filter "*TailCallDiagnoser*" --job short
/// </summary>
[TailCallDiagnoser(logFailuresOnly: false, filterByNamespace: true)]
public class TailCallDiagnoserBenchmarks
{
    private const int Depth = 1_000;

    [Benchmark(Baseline = true)]
    public long TailRecursive() => SumTailRecursive(Depth, 0);

    [Benchmark]
    public long NotTailRecursive() => SumNotTail(Depth);

    private static long SumTailRecursive(int n, long accumulator) =>
        n == 0 ? accumulator : SumTailRecursive(n - 1, accumulator + n);

    private static long SumNotTail(int n) => n == 0 ? 0 : n + SumNotTail(n - 1);
}
