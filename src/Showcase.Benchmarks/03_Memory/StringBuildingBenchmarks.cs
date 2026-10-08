using System.Text;
using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.Memory;

/// <summary>
/// [MemoryDiagnoser]: колонки Allocated (байт на операцію) і Gen0/Gen1/Gen2 (кількість збірок GC на 1000 операцій).
/// Чотири способи зібрати рядок "0,1,2,...,N-1".
/// Чого очікувати: конкатенація через += алокує новий рядок на кожному кроці — Allocated росте квадратично;
/// StringBuilder — кілька буферів + фінальний рядок; string.Create і stackalloc Span&lt;char&gt; —
/// лише одна алокація (сам результат), Allocated мінімальний.
/// </summary>
[MemoryDiagnoser]
public class StringBuildingBenchmarks
{
    [Params(10, 100)]
    public int Count { get; set; }

    [Benchmark(Baseline = true)]
    public string Concatenation()
    {
        string result = "";
        for (int i = 0; i < Count; i++)
            result += i == 0 ? i.ToString() : "," + i;
        return result;
    }

    [Benchmark]
    public string StringBuilder()
    {
        var builder = new StringBuilder();
        for (int i = 0; i < Count; i++)
        {
            if (i > 0) builder.Append(',');
            builder.Append(i);
        }
        return builder.ToString();
    }

    [Benchmark]
    public string StringCreate()
    {
        int length = Count - 1; // коми
        for (int i = 0; i < Count; i++)
            length += DigitCount(i);

        return string.Create(length, Count, static (span, count) =>
        {
            int position = 0;
            for (int i = 0; i < count; i++)
            {
                if (i > 0) span[position++] = ',';
                i.TryFormat(span[position..], out int written);
                position += written;
            }
        });
    }

    [Benchmark]
    public string StackallocSpan()
    {
        Span<char> buffer = stackalloc char[1024];
        int position = 0;
        for (int i = 0; i < Count; i++)
        {
            if (i > 0) buffer[position++] = ',';
            i.TryFormat(buffer[position..], out int written);
            position += written;
        }
        return new string(buffer[..position]);
    }

    // Цілочисельний підрахунок: Math.Log10 тут був би дорожчим за саме форматування.
    private static int DigitCount(int value)
    {
        int digits = 1;
        while (value >= 10)
        {
            value /= 10;
            digits++;
        }
        return digits;
    }
}
