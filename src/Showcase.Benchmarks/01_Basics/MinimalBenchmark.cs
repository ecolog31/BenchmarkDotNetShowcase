using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.Basics;

/// <summary>
/// Мінімальний бенчмарк: один метод з [Benchmark], без додаткових атрибутів.
/// Окремо його можна запустити через <c>BenchmarkRunner.Run&lt;MinimalBenchmark&gt;()</c>;
/// у цьому проєкті все йде через BenchmarkSwitcher (--filter "*MinimalBenchmark*").
/// Чого очікувати: таблицю з Mean / Error / StdDev для одного методу.
/// </summary>
public class MinimalBenchmark
{
    private int[] _numbers = [];

    [GlobalSetup]
    public void Setup() => _numbers = Enumerable.Range(0, 1_000).ToArray();

    [Benchmark]
    public int SumWithForLoop()
    {
        int sum = 0;
        for (int i = 0; i < _numbers.Length; i++)
            sum += _numbers[i];
        return sum;
    }
}
