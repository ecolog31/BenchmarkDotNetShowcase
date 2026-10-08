using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace Showcase.Benchmarks.Runtimes;

/// <summary>
/// Кілька Job у власному ManualConfig + змінні середовища рантайму через WithEnvironmentVariable.
/// Job.Default.AsBaseline() — точка відліку; інші job-и вимикають по одній можливості JIT:
/// DOTNET_TieredPGO=0 (динамічна оптимізація за профілем), DOTNET_TieredCompilation=0 (одразу повна
/// оптимізація, без tier-0 і без PGO), DOTNET_ReadyToRun=0 (ігнорувати попередньо скомпільований код BCL).
/// Приклад: виклик через інтерфейс, коли всі об'єкти насправді одного типу — ідеальний випадок для
/// guarded devirtualization, яку дає PGO.
/// Чого очікувати: без PGO (NoPGO, NoTiering) віртуальні виклики не девіртуалізуються — помітно повільніше;
/// NoReadyToRun на стабільну швидкість майже не впливає (він впливає на старт).
/// </summary>
[Config(typeof(JitSettingsConfig))]
[HideColumns("Error", "StdDev", "RatioSD")]
public class JitSettingsBenchmarks
{
    public interface IShape
    {
        double Area();
    }

    public sealed class Square(double side) : IShape
    {
        public double Area() => side * side;
    }

    public sealed class Circle(double radius) : IShape
    {
        public double Area() => Math.PI * radius * radius;
    }

    private IShape[] _shapes = [];

    [GlobalSetup]
    public void Setup()
    {
        // Circle існує лише щоб JIT не міг статично довести, що IShape завжди Square.
        _shapes = Enumerable.Range(0, 1_000).Select(i => (IShape)new Square(i)).ToArray();
        GC.KeepAlive(new Circle(1));
    }

    [Benchmark]
    public double SumAreasViaInterface()
    {
        double total = 0;
        foreach (IShape shape in _shapes)
            total += shape.Area();
        return total;
    }

    private sealed class JitSettingsConfig : ManualConfig
    {
        public JitSettingsConfig()
        {
            AddJob(Job.Default.WithId("Default").AsBaseline());
            AddJob(Job.Default.WithId("NoPGO").WithEnvironmentVariable("DOTNET_TieredPGO", "0"));
            AddJob(Job.Default.WithId("NoTiering").WithEnvironmentVariable("DOTNET_TieredCompilation", "0"));
            AddJob(Job.Default.WithId("NoReadyToRun").WithEnvironmentVariable("DOTNET_ReadyToRun", "0"));
        }
    }
}
