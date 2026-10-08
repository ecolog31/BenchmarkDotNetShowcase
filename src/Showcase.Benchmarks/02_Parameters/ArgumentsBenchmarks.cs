using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.Parameters;

/// <summary>
/// [Arguments]: значення передаються прямо в параметри бенчмарк-методу, а не через властивості класу.
/// Зручно, коли різні методи потребують різних вхідних даних (у [Params] вони спільні для всього класу).
/// Кожен [Arguments] — окремий рядок таблиці з колонкою number.
/// Чого очікувати: перевірка простоти пробним діленням — час росте приблизно як √n:
/// 1 000 003 (простий) перевіряється найдовше. Парне 1 000 000 відсіюється однією перевіркою —
/// це не відрізнити від порожнього методу, тому буде ≈ 0 ns і попередження ZeroMeasurement.
/// </summary>
public class ArgumentsBenchmarks
{
    [Benchmark]
    [Arguments(1_000_000)]
    [Arguments(7_919)]
    [Arguments(104_729)]
    [Arguments(1_000_003)]
    public bool IsPrime(int number)
    {
        if (number < 2) return false;
        if (number % 2 == 0) return number == 2;
        for (int divisor = 3; (long)divisor * divisor <= number; divisor += 2)
            if (number % divisor == 0)
                return false;
        return true;
    }
}
