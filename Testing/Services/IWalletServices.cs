
namespace Services
{
    public interface IWalletServices
    {
        Task<decimal> DepositAsync(string amount);
        Task<decimal> GetBalanceAsync();
        Task<decimal> WithdrawAsync(string amount);
    }
}