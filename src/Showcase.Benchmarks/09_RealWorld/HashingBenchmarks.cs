using System.IO.Hashing;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.RealWorld;

/// <summary>
/// Хешування буфера: криптографічні MD5 і SHA256 (через статичні HashData — без створення об'єкта)
/// vs некриптографічний XxHash3 / XxHash64 (пакет System.IO.Hashing) — для кешів, дедуплікації, контрольних сум.
/// Чого очікувати: XxHash3 — найшвидший (десятки ГБ/с); SHA256 на процесорах з SHA-розширеннями швидший за MD5;
/// для 1 МБ різниця між xxHash і SHA256 — на порядок.
/// </summary>
[MemoryDiagnoser]
public class HashingBenchmarks
{
    private byte[] _data = [];

    [Params(1_024, 1_024 * 1_024)]
    public int Size { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _data = new byte[Size];
        new Random(42).NextBytes(_data);
    }

    [Benchmark(Baseline = true)]
    public byte[] Sha256() => SHA256.HashData(_data);

    [Benchmark]
    public byte[] Md5() => MD5.HashData(_data);

    [Benchmark]
    public ulong XxHash64Hash() => XxHash64.HashToUInt64(_data);

    [Benchmark]
    public ulong XxHash3Hash() => XxHash3.HashToUInt64(_data);
}
