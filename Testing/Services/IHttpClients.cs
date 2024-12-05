
namespace Services
{
    public interface IHttpClients
    {
        Task<HttpResponseMessage> DepositAsync(string amount);
        Task<HttpResponseMessage> GetBalanceAsync();
        Task<HttpResponseMessage> WithdrawAsync(string amount);
    }
}