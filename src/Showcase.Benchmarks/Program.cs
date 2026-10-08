using BenchmarkDotNet.Running;

namespace Showcase.Benchmarks;

public static class Program
{
    /// <summary>
    /// Точка входу. Без аргументів BenchmarkSwitcher показує інтерактивне меню
    /// з усіма бенчмарками збірки; з аргументами працює як CLI
    /// (--filter, --list flat, --job short, --runtimes, --memory тощо).
    /// Запускати тільки в Release і без дебагера:
    ///   dotnet run -c Release -f net10.0 -- --filter "*Basics*" --job short
    /// </summary>
    public static void Main(string[] args) =>
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
}
