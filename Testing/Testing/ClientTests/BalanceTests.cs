using FluentAssertions;
using Services;
using System.Net;
using System.Text.Json;

namespace Testing.Clients;

public class BalanceTests : WalletApiTestBase
{
    private IHttpClients client;

    public BalanceTests()
    {
        client = new HttpClients(HttpClientHelper.Client);
    }

    [Fact]
    public async Task GetBalance_ShouldReturn200AndBalance_WhenValid()
    {
        var response = await client.GetBalanceAsync();
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