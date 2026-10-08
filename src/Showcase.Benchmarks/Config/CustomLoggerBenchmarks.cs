using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Loggers;

namespace Showcase.Benchmarks.Configuration;

/// <summary>
/// Власний ILogger: BenchmarkDotNet пише весь вивід (прогрес, ітерації, таблицю) у ВСІ зареєстровані логери.
/// Логер з [Config] додається до стандартних (консоль, файл *.log), а не замінює їх, тому
/// ElapsedTimeFileLogger пише в окремий файл BenchmarkDotNet.Artifacts/elapsed-time.log,
/// додаючи до кожного рядка час від старту — видно, скільки триває кожна фаза (збірка, прогрів, вимір).
/// Логери об'єднуються на рівні всього запуску: якщо разом з цим класом запускаються інші, у файл потрапить і їхній вивід.
/// Чого очікувати: у файлі рядки на кшталт «[+00:19.820] WorkloadActual   1: ...».
/// </summary>
[Config(typeof(CustomLoggerConfig))]
public class CustomLoggerBenchmarks
{
    private readonly byte[] _data = Enumerable.Range(0, 4_096).Select(i => (byte)i).ToArray();

    [Benchmark]
    public int Checksum()
    {
        int sum = 0;
        foreach (byte b in _data)
            sum = unchecked(sum * 31 + b);
        return sum;
    }

    private sealed class CustomLoggerConfig : ManualConfig
    {
        public CustomLoggerConfig() =>
            AddLogger(new ElapsedTimeFileLogger(Path.Combine("BenchmarkDotNet.Artifacts", "elapsed-time.log")));
    }
}

/// <summary>
/// Логер, що пише у файл із префіксом часу від першого запису.
/// Файл відкривається ліниво: BenchmarkSwitcher створює конфігурації ВСІХ класів під час пошуку бенчмарків
/// (навіть відфільтрованих), тож відкриття файлу в конструкторі ламало б інші запуски.
/// </summary>
public sealed class ElapsedTimeFileLogger(string path) : ILogger
{
    private readonly Stopwatch _stopwatch = new();
    private StreamWriter? _file;
    private bool _atLineStart = true;

    private StreamWriter Writer
    {
        get
        {
            if (_file is null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
                var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                _file = new StreamWriter(stream) { AutoFlush = true };
                _stopwatch.Start();
            }
            return _file;
        }
    }

    public string Id => nameof(ElapsedTimeFileLogger);
    public int Priority => 0;

    public void Write(LogKind logKind, string text)
    {
        if (_atLineStart)
            Writer.Write($"[+{_stopwatch.Elapsed:mm\\:ss\\.fff}] ");
        Writer.Write(text);
        _atLineStart = false;
    }

    public void WriteLine()
    {
        Writer.WriteLine();
        _atLineStart = true;
    }

    public void WriteLine(LogKind logKind, string text)
    {
        Write(logKind, text);
        WriteLine();
    }

    public void Flush() => _file?.Flush();
}
