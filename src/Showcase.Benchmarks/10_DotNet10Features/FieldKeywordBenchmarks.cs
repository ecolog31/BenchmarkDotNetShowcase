using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.DotNet10;

/// <summary>
/// Ключове слово field (C# 14): доступ до автоматично згенерованого поля властивості з її аксесорів —
/// можна додати логіку (тут — валідацію) без ручного оголошення backing field.
/// Це чистий синтаксис: компілятор генерує той самий IL, що й для ручного поля.
/// Чого очікувати: ExplicitBackingField і FieldKeyword — однаковий час на всіх рантаймах (Ratio ≈ 1.00), 0 B.
/// </summary>
[Config(typeof(DotNetVersionsConfig))]
public class FieldKeywordBenchmarks
{
    private sealed class WithExplicitField
    {
        private int _value;

        public int Value
        {
            get => _value;
            set => _value = value >= 0 ? value : throw new ArgumentOutOfRangeException(nameof(value));
        }
    }

    private sealed class WithFieldKeyword
    {
        public int Value
        {
            get => field;
            set => field = value >= 0 ? value : throw new ArgumentOutOfRangeException(nameof(value));
        }
    }

    private readonly WithExplicitField _explicit = new();
    private readonly WithFieldKeyword _keyword = new();

    [Benchmark(Baseline = true)]
    public int ExplicitBackingField()
    {
        for (int i = 0; i < 1_000; i++)
            _explicit.Value = i;
        return _explicit.Value;
    }

    [Benchmark]
    public int FieldKeyword()
    {
        for (int i = 0; i < 1_000; i++)
            _keyword.Value = i;
        return _keyword.Value;
    }
}
