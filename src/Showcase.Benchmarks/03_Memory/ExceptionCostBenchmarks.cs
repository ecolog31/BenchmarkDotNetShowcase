using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.Memory;

/// <summary>
/// [ExceptionDiagnoser]: колонка Exceptions — скільки винятків кидається за одну операцію.
/// Разом з [MemoryDiagnoser] видно і час, і алокації об'єкта винятку та stack trace.
/// Задача — розібрати рядок з числом: коректний і некоректний вхід, Parse+catch vs TryParse.
/// Чого очікувати: на коректному вході обидва однакові й без винятків;
/// на некоректному Parse+catch у сотні разів повільніший (Exceptions = 1, сотні байт Allocated),
/// TryParse лишається таким же швидким.
/// </summary>
[MemoryDiagnoser]
[ExceptionDiagnoser]
public class ExceptionCostBenchmarks
{
    [Params("12345", "12x45")]
    public string Input { get; set; } = "";

    [Benchmark(Baseline = true)]
    public int TryParse() => int.TryParse(Input, out int value) ? value : -1;

    [Benchmark]
    public int ParseWithCatch()
    {
        try
        {
            return int.Parse(Input);
        }
        catch (FormatException)
        {
            return -1;
        }
    }
}
