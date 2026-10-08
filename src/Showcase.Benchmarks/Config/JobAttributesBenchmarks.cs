using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.Configuration;

/// <summary>
/// Готові атрибути job-ів: [DryJob] — один прогін без прогріву (перевірити, що все компілюється й не падає);
/// [ShortRunJob] — 3 прогріви + 3 ітерації (швидка оцінка). Є ще [MediumRunJob], [LongRunJob], [VeryLongRunJob].
/// Кожен атрибут додає окремий job, тож тут кожен метод запускається двічі.
/// Чого очікувати: Dry показує час одного холодного виклику (з JIT) — значно більший і неточний;
/// ShortRun — реалістичний час, але з широким Error.
/// Запускати без --job short (інакше додасться третій job):
///   dotnet run -c Release -f net10.0 -- --filter "*JobAttributes*"
/// </summary>
[DryJob]
[ShortRunJob]
public class JobAttributesBenchmarks
{
    private readonly double[] _values = Enumerable.Range(1, 1_000).Select(i => (double)i).ToArray();

    [Benchmark]
    public double SumOfSquareRoots()
    {
        double sum = 0;
        foreach (double value in _values)
            sum += Math.Sqrt(value);
        return sum;
    }
}
