using FluentAssertions;
using Services;
using System.Net;
using System.Text.Json;

namespace Testing.ClientTests;

public class DepositTests : WalletApiTestBase
{
    private readonly IHttpClients _clients;

    public DepositTests()
    {
        _clients = new HttpClients(HttpClientHelper.Client);
    }

    [Theory]
    [InlineData("100")]
    public async Task DepositAsync_ShouldReturnUpdatedBalance(string depositAmount)
    {
        var response = await _clients.DepositAsync(depositAmount);

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
    [InlineData("-100")]
    [InlineData("A")]
    [InlineData("")]
    public async Task DepositAsync_ShouldReturnErrorForInvalidRequest(string negativeAmount)
    {
        var response = await _clients.DepositAsync(negativeAmount);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }


}
