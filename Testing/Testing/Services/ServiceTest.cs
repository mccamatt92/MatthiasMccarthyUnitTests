using FluentAssertions;
using Moq;
using Services;
using System.Net;
using System.Text;

namespace Testing.Services;

public class ServiceTest
{
    private readonly Mock<IHttpClients> _mockClient;
    private readonly WalletServices _walletServices;

    public ServiceTest()
    {
        _mockClient = new Mock<IHttpClients>(); // IClient represents a dependency that interacts with external services.
        _walletServices = new WalletServices(_mockClient.Object);
    }

    // --- Balance Service Tests ---
    [Fact]
    public async Task GetBalanceAsync_ShouldReturnBalanceSuccessfully()
    {
        var currentBalance = "100";
        _mockClient.Setup(x => x.GetBalanceAsync()).ReturnsAsync(new HttpResponseMessage
        {
            Content = new StringContent($"{{\"amount\": {currentBalance}}}"),
            StatusCode = HttpStatusCode.OK
        });
        var balance = await _walletServices.GetBalanceAsync();

        balance.Should().Be(decimal.Parse(currentBalance));
        _mockClient.Verify(x => x.GetBalanceAsync(), Times.Once);
    }

    [Fact]
    public async Task GetBalanceAsync_ShouldThrowExceptionWhenServiceFails()
    {
        _mockClient.Setup(x => x.GetBalanceAsync()).ThrowsAsync(new HttpRequestException("Service unavailable"));

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => _walletServices.GetBalanceAsync());
        ex.Message.Should().Be("Service unavailable");
        _mockClient.Verify(x => x.GetBalanceAsync(), Times.Once);
    }

    // --- Top-Up Service Tests ---
    [Fact]
    public async Task DepositAsync_ShouldTopUpBalanceSuccessfully()
    {
        var topUpAmount = "1";
        var jsonResponse = "{\"amount\":1}";
        _mockClient.Setup(x => x.DepositAsync(topUpAmount)).ReturnsAsync(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        });

        var response = await _walletServices.DepositAsync(topUpAmount);

        response.Should().Be(1);
        _mockClient.Verify(x => x.DepositAsync(It.Is<string>(d => d == topUpAmount)), Times.Once);
    }

    [Fact]
    public async Task DepositAsync_ShouldThrowExceptionForNegativeAmount()
    {
        var negativeAmount = "-50";

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _walletServices.DepositAsync(negativeAmount));
        ex.Message.Should().Be("Top-up amount must be positive");
        _mockClient.Verify(x => x.DepositAsync(It.IsAny<string>()), Times.Never);
    }

    // --- Withdrawal Service Tests ---
    [Fact]
    public async Task WithdrawAsync_ShouldWithdrawSuccessfullyWithSufficientBalance()
    {
        var currentBalance = "200";
        var withdrawalAmount = "50";
        var updatedBalance = "150";

        _mockClient.Setup(x => x.GetBalanceAsync()).ReturnsAsync(new HttpResponseMessage
        {
            Content = new StringContent($"{{\"amount\": {currentBalance}}}"),
            StatusCode = HttpStatusCode.OK
        });

        _mockClient.Setup(x => x.WithdrawAsync(withdrawalAmount)).ReturnsAsync(new HttpResponseMessage
        {
            Content = new StringContent($"{{\"amount\": {updatedBalance}}}"),
            StatusCode = HttpStatusCode.OK
        });

        var response = await _walletServices.WithdrawAsync(withdrawalAmount);

        response.Should().Be(decimal.Parse(updatedBalance));
        _mockClient.Verify(x => x.WithdrawAsync(It.Is<string>(w => w == withdrawalAmount)), Times.Once);
    }

    [Fact]
    public async Task WithdrawAsync_ShouldFailWithInsufficientBalance()
    {
        var currentBalance = "50";
        var withdrawalAmount = "100";
        _mockClient.Setup(x => x.GetBalanceAsync()).ReturnsAsync(new HttpResponseMessage
        {
            Content = new StringContent($"{{\"amount\": {currentBalance}}}"),
            StatusCode = HttpStatusCode.OK
        });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _walletServices.WithdrawAsync(withdrawalAmount));
        ex.Message.Should().Be("Insufficient balance for withdrawal");
        _mockClient.Verify(x => x.WithdrawAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task WithdrawAsync_ShouldThrowExceptionForNegativeAmount()
    {
        var negativeAmount = "-50";

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _walletServices.WithdrawAsync(negativeAmount));
        ex.Message.Should().Be("Withdrawal amount must be positive.");
        _mockClient.Verify(x => x.WithdrawAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task WithdrawAsync_ShouldNotWithdrawWithZeroAmountAndReturnCurrentBalance()
    {
        var currentBalance = "200";
        var zeroWithdrawalAmount = "0";

        _mockClient.Setup(x => x.GetBalanceAsync()).ReturnsAsync(new HttpResponseMessage
        {
            Content = new StringContent($"{{\"amount\": {currentBalance}}}"),
            StatusCode = HttpStatusCode.OK
        });

        var result = await _walletServices.WithdrawAsync(zeroWithdrawalAmount);

        result.Should().Be(decimal.Parse(currentBalance));
        _mockClient.Verify(x => x.WithdrawAsync(It.IsAny<string>()), Times.Never);
    }
}