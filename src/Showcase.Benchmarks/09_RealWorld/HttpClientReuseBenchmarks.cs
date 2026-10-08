using System.Net;
using BenchmarkDotNet.Attributes;

namespace Showcase.Benchmarks.RealWorld;

/// <summary>
/// HttpClient: новий екземпляр на кожен запит vs один спільний. Локальний HttpListener на 127.0.0.1
/// (без прав адміністратора для localhost) відповідає коротким текстом.
/// Новий HttpClient = нове TCP-з'єднання (handshake) і сокет у TIME_WAIT після закриття; спільний — повторно
/// використовує keep-alive з'єднання з пулу.
/// [InvocationCount(16)] обмежує кількість викликів за ітерацію: інакше підбір кількості викликів створив би
/// десятки тисяч з'єднань і вичерпав би ефемерні порти.
/// Чого очікувати: спільний клієнт у рази швидший і алокує менше.
/// </summary>
[MemoryDiagnoser]
[InvocationCount(16)]
public class HttpClientReuseBenchmarks
{
    private HttpListener _listener = null!;
    private Task _serverLoop = Task.CompletedTask;
    private HttpClient _sharedClient = null!;
    private Uri _uri = null!;

    [GlobalSetup]
    public void Setup()
    {
        int port = 50_000 + Random.Shared.Next(10_000);
        _uri = new Uri($"http://127.0.0.1:{port}/ping/");
        _listener = new HttpListener();
        _listener.Prefixes.Add(_uri.ToString());
        _listener.Start();
        _serverLoop = Task.Run(ServeAsync);
        _sharedClient = new HttpClient();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _sharedClient.Dispose();
        _listener.Stop();
        _listener.Close();
    }

    [Benchmark(Baseline = true)]
    public async Task<int> NewClientPerRequest()
    {
        using var client = new HttpClient();
        string body = await client.GetStringAsync(_uri);
        return body.Length;
    }

    [Benchmark]
    public async Task<int> SharedClient()
    {
        string body = await _sharedClient.GetStringAsync(_uri);
        return body.Length;
    }

    private async Task ServeAsync()
    {
        byte[] response = "pong"u8.ToArray();
        while (_listener.IsListening)
        {
            HttpListenerContext context;
            try
            {
                context = await _listener.GetContextAsync();
            }
            catch (Exception) when (!_listener.IsListening)
            {
                return;
            }
            context.Response.ContentLength64 = response.Length;
            await context.Response.OutputStream.WriteAsync(response);
            context.Response.Close();
        }
    }
}
