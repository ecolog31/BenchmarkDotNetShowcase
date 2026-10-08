using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.Advanced;

/// <summary>
/// Static-методи і BenchmarkDotNet 0.15.8: позначити [Benchmark] сам static-метод НЕ можна — валідатор зупиняє запуск:
/// «Benchmarked method `Recursive` is static. Benchmarks MUST be instance methods, static methods are not supported.»
/// Робочий підхід — тонкі instance-обгортки над static-функціями (чисті функції без стану лишаються static).
/// Приклад: обчислення Фібоначчі — рекурсивно vs ітеративно vs формулою Біне.
/// Чого очікувати: рекурсія експоненційно повільна вже на n = 25; цикл — наносекунди; формула Біне, попри O(1),
/// повільніша за цикл (Math.Pow і Math.Sqrt дорожчі за 25 додавань) і має double-похибку на великих n.
/// </summary>
public class StaticMethodsBenchmarks
{
    private int _n;

    [GlobalSetup]
    public void Setup() => _n = 25;

    [Benchmark(Baseline = true)]
    public long Recursive() => FibRecursive(_n);

    [Benchmark]
    public long Iterative() => FibIterative(_n);

    [Benchmark]
    public long BinetFormula() => FibBinet(_n);

    private static long FibRecursive(int n) => n < 2 ? n : FibRecursive(n - 1) + FibRecursive(n - 2);

    private static long FibIterative(int n)
    {
        long a = 0, b = 1;
        for (int i = 0; i < n; i++)
            (a, b) = (b, a + b);
        return a;
    }

    private static long FibBinet(int n)
    {
        double phi = (1 + Math.Sqrt(5)) / 2;
        return (long)Math.Round(Math.Pow(phi, n) / Math.Sqrt(5));
    }
}
