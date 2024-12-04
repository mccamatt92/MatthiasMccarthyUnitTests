namespace Testing;

public static class HttpClientHelper
{
    private static readonly Lazy<HttpClient> _httpClient = new Lazy<HttpClient>(() =>
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:8080/")
        };
        return client;
    });

    public static HttpClient Client => _httpClient.Value;
}
