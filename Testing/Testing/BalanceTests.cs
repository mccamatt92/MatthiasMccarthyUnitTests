using FluentAssertions;
using Services;
using System.Net;
using System.Text.Json;

namespace Testing;

public class BalanceTests : WalletApiTestBase
{
    private readonly ApiServices _walletService;

    public BalanceTests()
    {
        _walletService = new ApiServices(HttpClientHelper.Client);
    }

    [Fact]
    public async Task GetBalance_ShouldReturn200AndBalance_WhenValid()
    {
        var response = await _walletService.GetBalanceAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().NotBeNullOrWhiteSpace();

        using JsonDocument doc = JsonDocument.Parse(responseBody);
        doc.RootElement
            .GetProperty("amount")
            .GetDecimal()
            .Should().BeGreaterThanOrEqualTo(0);

    }

}