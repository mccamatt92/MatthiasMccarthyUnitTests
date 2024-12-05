using Testing;

public class WalletApiBase
{
    protected readonly HttpClient _client;

    public WalletApiBase()
    {
        // Use the HttpClient from the helper class
        _client = HttpClientHelper.Client;
    }

    protected async Task<HttpResponseMessage> GetAsync(string endpoint)
    {
        return await _client.GetAsync(endpoint);
    }

    protected async Task<HttpResponseMessage> PostAsync(string endpoint, object data)
    {
        var jsonContent = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(data),
            System.Text.Encoding.UTF8,
            "application/json"
        );
        return await _client.PostAsync(endpoint, jsonContent);
    }
}
