using System.Globalization;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Showcase.Benchmarks.Configuration;

/// <summary>
/// Власний ManualConfig, що додає статистичні колонки (Median, P95, Min, Max)
/// і власну колонку BudgetColumn. Підключається до класу через [Config(typeof(CustomColumnsConfig))].
/// </summary>
public sealed class CustomColumnsConfig : ManualConfig
{
    public CustomColumnsConfig()
    {
        AddColumn(StatisticColumn.Median, StatisticColumn.P95, StatisticColumn.Min, StatisticColumn.Max);
        AddColumn(new BudgetColumn(budgetNanoseconds: 1_000));
    }
}

/// <summary>
/// Власна колонка (IColumn): чи вкладається середній час у заданий бюджет.
/// GetValue отримує весь Summary і конкретний BenchmarkCase, тож колонка може рахувати що завгодно зі статистики.
/// </summary>
public sealed class BudgetColumn(double budgetNanoseconds) : IColumn
{
    public string Id => $"{nameof(BudgetColumn)}.{budgetNanoseconds}";
    public string ColumnName => string.Create(CultureInfo.InvariantCulture, $"Budget {budgetNanoseconds:N0} ns");
    public bool AlwaysShow => true;
    public ColumnCategory Category => ColumnCategory.Custom;
    public int PriorityInCategory => 0;
    public bool IsNumeric => false;
    public UnitType UnitType => UnitType.Dimensionless;
    public string Legend => $"OK, якщо Mean не перевищує {budgetNanoseconds:N0} ns, інакше — у скільки разів перевищено";

    public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;
    public bool IsAvailable(Summary summary) => true;

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase) =>
        GetValue(summary, benchmarkCase, SummaryStyle.Default);

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style)
    {
        double? mean = summary[benchmarkCase]?.ResultStatistics?.Mean;
        if (mean is null)
            return "?";
        // InvariantCulture: інакше в українській культурі вийде «x46,1» і нерозривний пробіл у «1 000».
        return mean <= budgetNanoseconds ? "OK" : string.Create(CultureInfo.InvariantCulture, $"x{mean.Value / budgetNanoseconds:F1}");
    }

    public override string ToString() => ColumnName;
}
