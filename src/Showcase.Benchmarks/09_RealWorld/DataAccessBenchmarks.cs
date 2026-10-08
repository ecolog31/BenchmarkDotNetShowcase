using BenchmarkDotNet.Attributes;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Showcase.Benchmarks.RealWorld;

/// <summary>
/// Доступ до даних: «сирий» ADO.NET vs Dapper vs EF Core (з трекінгом і AsNoTracking) на SQLite in-memory.
/// Таблиця з 1 000 продуктів створюється в GlobalSetup; спільна in-memory база живе, доки відкрите «якірне» з'єднання.
/// Дві операції: вибірка 100 рядків за умовою і пошук одного рядка за ключем.
/// Чого очікувати: Dapper на ~30% повільніший за ручний ADO.NET (тонка обгортка з маппінгом); EF Core повільніший і алокує більше,
/// AsNoTracking помітно зменшує різницю (немає change tracking).
/// </summary>
[MemoryDiagnoser]
public class DataAccessBenchmarks
{
    private const string ConnectionString = "Data Source=bdn-showcase;Mode=Memory;Cache=Shared";
    private SqliteConnection _anchor = null!;
    private SqliteConnection _connection = null!;
    private ShopContext _db = null!;

    public sealed class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public double Price { get; set; }
        public int CategoryId { get; set; }
    }

    public sealed class ShopContext(DbContextOptions<ShopContext> options) : DbContext(options)
    {
        public DbSet<Product> Products => Set<Product>();
    }

    [GlobalSetup]
    public void Setup()
    {
        _anchor = new SqliteConnection(ConnectionString);
        _anchor.Open();
        _anchor.Execute("CREATE TABLE Products (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL, Price REAL NOT NULL, CategoryId INTEGER NOT NULL)");
        using (var tx = _anchor.BeginTransaction())
        {
            for (int i = 1; i <= 1_000; i++)
                _anchor.Execute("INSERT INTO Products VALUES (@Id, @Name, @Price, @CategoryId)",
                    new { Id = i, Name = $"Product {i}", Price = 1.5 * i, CategoryId = i % 10 }, tx);
            tx.Commit();
        }

        _connection = new SqliteConnection(ConnectionString);
        _connection.Open();
        _db = new ShopContext(new DbContextOptionsBuilder<ShopContext>().UseSqlite(_connection).Options);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _db.Dispose();
        _connection.Dispose();
        _anchor.Dispose();
    }

    [Benchmark(Baseline = true)]
    public int AdoNetQuery()
    {
        using var command = _connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Price, CategoryId FROM Products WHERE CategoryId = 3";
        using var reader = command.ExecuteReader();
        var products = new List<Product>();
        while (reader.Read())
            products.Add(new Product { Id = reader.GetInt32(0), Name = reader.GetString(1), Price = reader.GetDouble(2), CategoryId = reader.GetInt32(3) });
        return products.Count;
    }

    [Benchmark]
    public int DapperQuery() =>
        _connection.Query<Product>("SELECT Id, Name, Price, CategoryId FROM Products WHERE CategoryId = @c", new { c = 3 }).Count();

    [Benchmark]
    public int EfCoreTracking()
    {
        int count = _db.Products.Where(p => p.CategoryId == 3).ToList().Count;
        _db.ChangeTracker.Clear(); // інакше трекер накопичуватиме об'єкти між викликами
        return count;
    }

    [Benchmark]
    public int EfCoreNoTracking() => _db.Products.AsNoTracking().Where(p => p.CategoryId == 3).ToList().Count;

    [Benchmark]
    public Product? DapperById() =>
        _connection.QuerySingleOrDefault<Product>("SELECT Id, Name, Price, CategoryId FROM Products WHERE Id = @id", new { id = 500 });

    [Benchmark]
    public Product? EfCoreFindNoTracking() => _db.Products.AsNoTracking().FirstOrDefault(p => p.Id == 500);
}
