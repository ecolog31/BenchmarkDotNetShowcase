using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;

namespace Showcase.Benchmarks.Memory;

/// <summary>
/// [NativeMemoryProfiler] (пакет BenchmarkDotNet.Diagnostics.Windows): колонки Allocated native memory
/// і Native memory leak — пам'ять поза керованою купою, яку [MemoryDiagnoser] не бачить.
/// Чого очікувати: усі три методи мають Allocated (managed) = 0 B або майже 0;
/// NativeAllocAndFree — 1 КБ native allocated, leak 0; NativeAllocLeak — 1 КБ native allocated і ~1 КБ leak на операцію.
///
/// ⚠ Потребує запуску терміналу від АДМІНІСТРАТОРА (використовує ETW-сесію ядра):
///   dotnet run -c Release -f net10.0 -- --filter "*NativeMemoryBenchmarks*" --job short
/// [InvocationCount] обмежує кількість викликів за ітерацію, щоб метод з витоком не з'їв гігабайти пам'яті.
/// </summary>
[NativeMemoryProfiler]
[MemoryDiagnoser]
[InvocationCount(1_000)]
public class NativeMemoryBenchmarks
{
    private const int Size = 1_024;

    [Benchmark(Baseline = true)]
    public unsafe byte NativeAllocAndFree()
    {
        byte* buffer = (byte*)NativeMemory.Alloc(Size);
        buffer[0] = 42;
        byte result = buffer[0];
        NativeMemory.Free(buffer);
        return result;
    }

    [Benchmark]
    public unsafe byte NativeAllocLeak()
    {
        byte* buffer = (byte*)NativeMemory.Alloc(Size);
        buffer[0] = 42;
        return buffer[0]; // Free навмисно пропущено — це витік, який має показати профайлер
    }

    [Benchmark]
    public byte AllocHGlobalAndFree()
    {
        IntPtr buffer = Marshal.AllocHGlobal(Size);
        Marshal.WriteByte(buffer, 42);
        byte result = Marshal.ReadByte(buffer);
        Marshal.FreeHGlobal(buffer);
        return result;
    }
}
