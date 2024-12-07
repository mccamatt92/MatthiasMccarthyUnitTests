namespace Services;

public class WalletServices : IWalletServices
{
    private readonly IHttpClients _client;
    private readonly JsonHelper jsonHelper;

    public WalletServices(IHttpClients client)
    {
        _client = client;
        jsonHelper = new JsonHelper();
    }

    public async Task<decimal> GetBalanceAsync()
    {
        var response = await _client.GetBalanceAsync();
        ValidateResponse(response, "Failed to retrieve current balance");

        var balance = ParseJsonBody(await response.Content.ReadAsStringAsync(), "The balance response is empty");
        return balance;
    }

    public async Task<decimal> DepositAsync(string amount)
    {
        if (string.IsNullOrWhiteSpace(amount) || decimal.Parse(amount) <= 0)
        {
            throw new ArgumentException("Top-up amount must be positive");
        }

        var response = await _client.DepositAsync(amount);
        ValidateResponse(response, "Failed to get a valid response from the server");

        var balance = ParseJsonBody(await response.Content.ReadAsStringAsync(), "The server response is empty");
        return balance;
    }

    public async Task<decimal> WithdrawAsync(string amount)
    {
        if (string.IsNullOrWhiteSpace(amount) || decimal.Parse(amount) < 0)
        {
            throw new ArgumentException("Withdrawal amount must be positive.");
        }

        if (decimal.Parse(amount) == 0)
        {
            return await GetBalanceAsync();
        }

        var currentBalance = await GetBalanceAsync();
        if (decimal.Parse(amount) > currentBalance)
        {
            throw new InvalidOperationException("Insufficient balance for withdrawal");
        }

        var response = await _client.WithdrawAsync(amount);
        ValidateResponse(response, "Failed to get a valid response from the server");

        var updatedBalance = ParseJsonBody(await response.Content.ReadAsStringAsync(), "The server response is empty");
        return updatedBalance;
    }

    private void ValidateResponse(HttpResponseMessage response, string errorMessage)
    {
        if (response == null || response.Content == null)
        {
            throw new InvalidOperationException(errorMessage);
        }
    }

    private decimal ParseJsonBody(string content, string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException(errorMessage);
        }

        return jsonHelper.JsonBody(content);
    }
}