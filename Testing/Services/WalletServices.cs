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
        var balance = jsonHelper.JsonBody(await response.Content.ReadAsStringAsync());

        return balance;
    }

    public async Task<decimal> DepositAsync(string amount)
    {
        var response = await _client.DepositAsync(amount);
        var balance = jsonHelper.JsonBody(await response.Content.ReadAsStringAsync());

        return balance;
    }

    public async Task<decimal> WithdrawAsync(string amount)
    {
        var response = await _client.WithdrawAsync(amount);
        var balance = jsonHelper.JsonBody(await response.Content.ReadAsStringAsync());

        return balance;
    }

}
