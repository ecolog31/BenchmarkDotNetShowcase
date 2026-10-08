using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

namespace Showcase.Benchmarks.Advanced;

/// <summary>
/// Вимірювання зовнішнього процесу: повний час запуску програми (cold start) — від Process.Start до виходу.
/// GlobalSetup публікує src/Showcase.ColdStartApp у трьох варіантах (це довго — поза виміром):
/// JIT (звичайний framework-dependent), ReadyToRun (код попередньо скомпільовано, JIT лише доробляє),
/// NativeAOT (нативний exe без JIT; потрібен C++ workload у Visual Studio).
/// RunStrategy.Monitoring — без підбору кількості викликів і без віднімання overhead: один запуск процесу = один вимір.
/// Чого очікувати: NativeAOT стартує в кілька разів швидше за JIT; ReadyToRun — між ними.
/// Запускати без --job short (job заданий у класі).
/// </summary>
[SimpleJob(RunStrategy.Monitoring, warmupCount: 2, iterationCount: 15)]
public class ColdStartProcessBenchmarks
{
    private string _jitExe = "";
    private string _readyToRunExe = "";
    private string _nativeAotExe = "";

    [GlobalSetup]
    public void Setup()
    {
        string repoRoot = FindRepoRoot();
        string project = Path.Combine(repoRoot, "src", "Showcase.ColdStartApp", "Showcase.ColdStartApp.csproj");
        string outRoot = Path.Combine(Path.GetTempPath(), "bdn-showcase-coldstart");

        _jitExe = Publish(project, Path.Combine(outRoot, "jit"), "-p:UseAppHost=true");
        _readyToRunExe = Publish(project, Path.Combine(outRoot, "r2r"), "-r win-x64 --self-contained false -p:PublishReadyToRun=true");
        _nativeAotExe = Publish(project, Path.Combine(outRoot, "aot"), "-r win-x64 -p:PublishAot=true");
    }

    [Benchmark(Baseline = true)]
    public int Jit() => RunToExit(_jitExe);

    [Benchmark]
    public int ReadyToRun() => RunToExit(_readyToRunExe);

    [Benchmark]
    public int NativeAot() => RunToExit(_nativeAotExe);

    private static int RunToExit(string exe)
    {
        var startInfo = new ProcessStartInfo(exe) { RedirectStandardOutput = true, UseShellExecute = false };
        using Process process = Process.Start(startInfo)!;
        process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return process.ExitCode;
    }

    private static string Publish(string project, string output, string extraArgs)
    {
        Console.WriteLine($"// Publishing {Path.GetFileName(output)}: {extraArgs}");
        var startInfo = new ProcessStartInfo("dotnet", $"publish \"{project}\" -c Release -f net10.0 -o \"{output}\" {extraArgs}")
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };
        using Process process = Process.Start(startInfo)!;
        string log = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"dotnet publish failed ({extraArgs}):{Environment.NewLine}{log}");
        return Path.Combine(output, "Showcase.ColdStartApp.exe");
    }

    // Процес бенчмарка працює з bin/.../Showcase.Benchmarks-*/..., тому шукаємо корінь репозиторію вгору по дереву.
    private static string FindRepoRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "BenchmarkDotNetShowcase.sln")))
                return dir.FullName;
        throw new DirectoryNotFoundException("Не знайдено BenchmarkDotNetShowcase.sln вище за " + AppContext.BaseDirectory);
    }
}
