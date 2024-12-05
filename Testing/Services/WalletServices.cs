namespace Services;

public class WalletServices : IWalletServices
{
    private readonly Clients _client;

    public WalletServices(Clients client)
    {
        _client = client;
    }

    public async Task<HttpResponseMessage> GetBalanceAsync()
    {
        return await _client.GetBalanceAsync();

    }

    public async Task<HttpResponseMessage> DepositAsync(string amount)
    {
        return await _client.DepositAsync(amount); ;
    }

    public async Task<HttpResponseMessage> WithdrawAsync(string amount)
    {
        return await _client.WithdrawAsync(amount);
    }
}
