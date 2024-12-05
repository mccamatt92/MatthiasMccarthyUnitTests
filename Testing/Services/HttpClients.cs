using System.Net.Http.Json;

namespace Services
{
    public class HttpClients : IHttpClients
    {
        private readonly HttpClient _httpClient;

        public HttpClients(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> GetBalanceAsync()
        {
            return await _httpClient.GetAsync("/onlinewallet/balance");
        }

        public async Task<HttpResponseMessage> DepositAsync(string amount)
        {
            var requestContent = new { amount };
            var response = await _httpClient.PostAsJsonAsync("/onlinewallet/deposit", requestContent);
            return response;
        }

        public async Task<HttpResponseMessage> WithdrawAsync(string amount)
        {
            var requestContent = new { amount };
            var response = await _httpClient.PostAsJsonAsync("/onlinewallet/withdraw", requestContent);
            return response;
        }
    }
}
