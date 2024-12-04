using System.Net.Http.Json;

namespace Services
{
    public class ApiServices
    {
        private readonly HttpClient _httpClient;

        public ApiServices(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> GetBalanceAsync()
        {
            return await _httpClient.GetAsync("/onlinewallet/balance");
        }

        public async Task<HttpResponseMessage> DepositAsync(decimal amount)
        {
            var requestContent = new { amount };
            var response = await _httpClient.PostAsJsonAsync("/onlinewallet/deposit", requestContent);
            return response;
        }
    }
}
