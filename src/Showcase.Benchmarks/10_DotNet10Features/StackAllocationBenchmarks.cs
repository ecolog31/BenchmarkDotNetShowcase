using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.DotNet10;

/// <summary>
/// Escape analysis і розміщення об'єктів у стеку (.NET 10): якщо JIT доводить, що об'єкт не «втікає» з методу
/// (не зберігається в поле, не повертається, не передається туди, де може бути збережений), він розміщує його
/// в стеку — без алокації в купі і без роботи для GC.
/// Три випадки: малий масив, малий об'єкт-клас і делегат-замикання, що використовуються лише локально.
/// Чого очікувати (виміряно): малий масив — 56 B на .NET 8/9, 0 B на .NET 10; об'єкт класу — 24 B лише на .NET 8
/// (escape analysis для об'єктів з'явився вже в .NET 9); делегат-замикання — 88 B на .NET 8/9 і 24 B на .NET 10
/// (делегат розміщено в стеку, а об'єкт замикання з полем captured — ще в купі).
/// Те саме ми вже бачили «випадково» в 04_Runtimes/GcModeBenchmarks: new byte[64] у циклі не алокувався.
/// </summary>
[Config(typeof(DotNetVersionsConfig))]
public class StackAllocationBenchmarks
{
    private int _seed = 7;

    private sealed class Point(int x, int y)
    {
        public int X { get; } = x;
        public int Y { get; } = y;
    }

    [Benchmark]
    public int SmallArrayNotEscaping()
    {
        int[] values = new int[8];
        for (int i = 0; i < values.Length; i++)
            values[i] = _seed + i;
        int sum = 0;
        foreach (int v in values)
            sum += v;
        return sum;
    }

    [Benchmark]
    public int SmallObjectNotEscaping()
    {
        var point = new Point(_seed, _seed * 2);
        return point.X + point.Y;
    }

    [Benchmark]
    public int LocalDelegateNotEscaping()
    {
        int captured = _seed;
        Func<int, int> addCaptured = x => x + captured;
        return addCaptured(10);
    }
}
