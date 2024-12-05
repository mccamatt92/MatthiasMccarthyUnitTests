
namespace Services
{
    public interface IWalletServices
    {
        Task<HttpResponseMessage> DepositAsync(string amount);
        Task<HttpResponseMessage> GetBalanceAsync();
        Task<HttpResponseMessage> WithdrawAsync(string amount);
    }
}