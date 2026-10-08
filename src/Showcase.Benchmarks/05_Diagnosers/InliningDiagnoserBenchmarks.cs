using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;

namespace Showcase.Benchmarks.Diagnosers;

/// <summary>
/// [InliningDiagnoser] (пакет BenchmarkDotNet.Diagnostics.Windows): виводить рішення JIT про інлайнінг —
/// які виклики вбудовано, а які ні і чому (з ETW-подій рантайму). logFailuresOnly: false — показати й успіхи;
/// allowedNamespaces — лише наш код, без шуму з BCL.
/// Результат (виміряно): AddSmall вбудовано; AddNoInlining — ні, з причиною «noinline per IL/cached result»
/// (виклик коштує ~0.9 ns: 1 000 викликів — 1.13 µs проти 0.23 µs, ×4.8). AddLarge, попри шість if, теж ВБУДОВАНО —
/// ліміт розміру IL не перевищено; CallsLargeMethod повільніший лише через більший обсяг роботи (ділення).
///
/// ⚠ Потребує запуску від АДМІНІСТРАТОРА (ETW-сесія):
///   dotnet run -c Release -f net10.0 -- --filter "*InliningDiagnoser*" --job short
/// </summary>
[InliningDiagnoser(logFailuresOnly: false, allowedNamespaces: ["Showcase.Benchmarks.Diagnosers"])]
public class InliningDiagnoserBenchmarks
{
    private const int Iterations = 1_000;

    [Benchmark(Baseline = true)]
    public int CallsInlinedMethod()
    {
        int sum = 0;
        for (int i = 0; i < Iterations; i++)
            sum = AddSmall(sum, i);
        return sum;
    }

    [Benchmark]
    public int CallsNoInliningMethod()
    {
        int sum = 0;
        for (int i = 0; i < Iterations; i++)
            sum = AddNoInlining(sum, i);
        return sum;
    }

    [Benchmark]
    public int CallsLargeMethod()
    {
        int sum = 0;
        for (int i = 0; i < Iterations; i++)
            sum = AddLarge(sum, i);
        return sum;
    }

    private static int AddSmall(int a, int b) => a + b;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int AddNoInlining(int a, int b) => a + b;

    // «Великий» метод: очікувалося, що JIT відмовиться його інлайнити, але InliningDiagnoser показав, що він вбудований.
    private static int AddLarge(int a, int b)
    {
        int result = a + b;
        if (b % 2 == 0) result ^= b << 1; else result ^= b >> 1;
        if (b % 3 == 0) result += b * 3; else result -= b / 3;
        if (b % 5 == 0) result ^= b << 5; else result ^= b >> 5;
        if (b % 7 == 0) result += b * 7; else result -= b / 7;
        if (b % 11 == 0) result ^= b << 11; else result ^= b >> 11;
        if (b % 13 == 0) result += b * 13; else result -= b / 13;
        return result - (result ^ (a + b)) + (a + b);
    }
}
