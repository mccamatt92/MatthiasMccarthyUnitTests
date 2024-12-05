using FluentAssertions;
using Services;
using System.Net;
using System.Text.Json;

namespace Testing.ClientTests;

public class WithdrawTests
{
    private readonly IHttpClients _clients;

    public WithdrawTests()
    {
        _clients = new HttpClients(HttpClientHelper.Client);
    }

    [Theory]
    [InlineData("200", "150")]
    public async Task WithdrawAsync_ShouldHandleZeroBalanceByDepositingFirst(string topUpAmount, string withdrawalAmount)
    {
        // Step 1: Fetch the current balance
        var balanceResponse = await _clients.GetBalanceAsync();
        balanceResponse.Should().NotBeNull();
        balanceResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Parse the current balance from the response
        var currentBalance = await GetBalanceFromResponse(balanceResponse);

        // Step 2: Deposit money if the balance is insufficient for withdrawal
        if (currentBalance < Convert.ToDecimal(withdrawalAmount))
        {
            // Deposit funds to cover the withdrawal
            var depositResponse = await _clients.DepositAsync(topUpAmount);

            // Validate the deposit response
            depositResponse.Should().NotBeNull();
            depositResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Re-fetch and update the current balance after deposit
            currentBalance += Convert.ToDecimal(topUpAmount);
        }

        // Step 3: Perform the withdrawal
        var withdrawResponse = await _clients.WithdrawAsync(withdrawalAmount);
        withdrawResponse.Should().NotBeNull();
        withdrawResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 4: Fetch the new balance after withdrawal
        var newBalance = await GetBalanceFromResponse(withdrawResponse);

        // Step 5: Assertions
        newBalance.Should().BeGreaterThanOrEqualTo(0);
        newBalance.Should().Be(currentBalance - Convert.ToDecimal(withdrawalAmount));
    }

    [Theory]
    [InlineData("1000")]
    public async Task WithdrawAsync_ShouldFailIfInsufficientBalance(string withdrawalAmount)
    {
        var balanceResponse = await _clients.GetBalanceAsync();
        balanceResponse.Should().NotBeNull();
        balanceResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var currentBalance = await GetBalanceFromResponse(balanceResponse);

        var withdrawResponse = await _clients.WithdrawAsync(withdrawalAmount);
        withdrawResponse.Should().NotBeNull();
        withdrawResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest); // Expect failure status code

        var newBalance = await GetBalanceFromResponse(await _clients.GetBalanceAsync());
        newBalance.Should().Be(currentBalance);
    }

    [Theory]
    [InlineData("-100")]
    [InlineData("A")]
    public async Task WithdrawAsync_ShouldFailIfWithdrawalAmountIsNegative(string withdrawalAmount)
    {
        var withdrawResponse = await _clients.WithdrawAsync(withdrawalAmount);
        withdrawResponse.Should().NotBeNull();
        withdrawResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest); // Expect failure status code
    }

    [Theory]
    [InlineData("0")] // Attempt to withdraw zero amount
    public async Task WithdrawAsync_ShouldNotFailIfWithdrawalAmountIsZero(string withdrawalAmount)
    {
        var balanceResponse = await _clients.GetBalanceAsync();
        var orignalBalance = await GetBalanceFromResponse(balanceResponse);

        // Step 1: Attempt to withdraw zero
        var withdrawResponse = await _clients.WithdrawAsync(withdrawalAmount);
        withdrawResponse.Should().NotBeNull();
        withdrawResponse.StatusCode.Should().Be(HttpStatusCode.OK); // Expect failure status code

        var newBalance = await GetBalanceFromResponse(withdrawResponse);

        newBalance.Should().Be(orignalBalance);
    }


    // Helper method to extract the balance from a response and handle potential parsing issues
    private async Task<decimal> GetBalanceFromResponse(HttpResponseMessage response)
    {
        try
        {
            var balanceResponseBody = await response.Content.ReadAsStringAsync();
            var document = JsonDocument.Parse(balanceResponseBody);

            // Check if 'amount' property exists and is valid
            if (document.RootElement.TryGetProperty("amount", out var amountProperty) && amountProperty.ValueKind == JsonValueKind.Number)
            {
                return amountProperty.GetDecimal();
            }
            else
            {
                throw new InvalidOperationException("The response does not contain a valid 'amount' property.");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to parse balance from response", ex);
        }
    }
}