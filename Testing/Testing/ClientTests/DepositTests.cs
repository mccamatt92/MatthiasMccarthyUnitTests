using FluentAssertions;
using Services;
using System.Net;
using System.Text.Json;

namespace Testing.Clients;

public class DepositTests : WalletApiTestBase
{
    private readonly IHttpClients _clients;

    public DepositTests()
    {
        _clients = new HttpClients(HttpClientHelper.Client);
    }

    [Theory]
    [InlineData("100")]
    [InlineData("50")]
    public async Task DepositAsync_ShouldReturnUpdatedBalance(string depositAmount)
    {
        var response = await _clients.DepositAsync(depositAmount);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().NotBeNullOrWhiteSpace();

        using var doc = JsonDocument.Parse(responseBody);
        doc.RootElement
           .GetProperty("amount")
           .GetDecimal()
           .Should().BeGreaterThanOrEqualTo(0);
    }

    [Theory]
    [InlineData("-100")] // Example: Deposit 100, expect balance to be 200
    [InlineData("-50")]  // Example: Deposit 50, expect balance to be 150
    public async Task DepositAsync_ShouldReturnErrorForInvalidRequest(string negativeAmount)
    {
        var response = await _clients.DepositAsync(negativeAmount);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }


}
