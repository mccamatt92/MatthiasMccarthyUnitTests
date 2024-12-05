﻿using FluentAssertions;
using Moq;
using Services;
using System.Net;

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
        // Arrange
        var currentBalance = "100";
        _mockClient.Setup(x => x.GetBalanceAsync()).ReturnsAsync(new HttpResponseMessage
        {
            Content = new StringContent($"{{\"amount\": {currentBalance}}}"),
            StatusCode = HttpStatusCode.OK
        });

        var balance = await _walletServices.GetBalanceAsync();

        // Assert
        balance.ToString().Should().Be(currentBalance);
        _mockClient.Verify(x => x.GetBalanceAsync(), Times.Once);
    }

    [Fact]
    public async Task GetBalanceAsync_ShouldThrowExceptionWhenServiceFails()
    {
        // Arrange
        _mockClient.Setup(x => x.GetBalanceAsync()).ThrowsAsync(new HttpRequestException("Service unavailable"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => _walletServices.GetBalanceAsync());
        ex.Message.Should().Be("Service unavailable");
        _mockClient.Verify(x => x.GetBalanceAsync(), Times.Once);
    }

    // --- TopUp Service Tests ---
    [Fact]
    public async Task TopUpAsync_ShouldTopUpBalanceSuccessfully()
    {
        // Arrange
        var topUpAmount = "50";
        _mockClient.Setup(x => x.DepositAsync(topUpAmount)).ReturnsAsync(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK
        });

        // Act
        var response = await _walletServices.DepositAsync(topUpAmount);

        // Assert
        _mockClient.Verify(x => x.DepositAsync(It.Is<string>(d => d.Equals(topUpAmount))), Times.Once);

    }

    [Fact]
    public async Task TopUpAsync_ShouldThrowExceptionForNegativeAmount()
    {
        // Arrange
        var negativeAmount = "-50";

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _walletServices.DepositAsync(negativeAmount));
        ex.Message.Should().Be("Top-up amount must be positive.");
        _mockClient.Verify(x => x.DepositAsync(It.IsAny<string>()), Times.Never);
    }

    // --- Withdraw Service Tests ---
    [Fact]
    public async Task WithdrawAsync_ShouldWithdrawSuccessfullyWithSufficientBalance()
    {
        // Arrange
        var currentBalance = "200";
        var withdrawalAmount = "100";
        _mockClient.Setup(x => x.GetBalanceAsync()).ReturnsAsync(new HttpResponseMessage
        {
            Content = new StringContent($"{{\"amount\": {currentBalance}}}"),
            StatusCode = HttpStatusCode.OK
        });
        _mockClient.Setup(x => x.WithdrawAsync(It.IsAny<string>())).ReturnsAsync(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK
        });

        // Act
        var response = await _walletServices.WithdrawAsync(withdrawalAmount);

        // Assert
        _mockClient.Verify(x => x.WithdrawAsync(It.Is<string>(w => w.Equals(withdrawalAmount))), Times.Once);
    }

    [Fact]
    public async Task WithdrawAsync_ShouldFailWithInsufficientBalance()
    {
        // Arrange
        var currentBalance = "50";
        var withdrawalAmount = "100";
        _mockClient.Setup(x => x.GetBalanceAsync()).ReturnsAsync(new HttpResponseMessage
        {
            Content = new StringContent($"{{\"amount\": {currentBalance}}}"),
            StatusCode = HttpStatusCode.OK
        });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _walletServices.WithdrawAsync(withdrawalAmount));
        ex.Message.Should().Be("Insufficient balance for withdrawal.");
        _mockClient.Verify(x => x.WithdrawAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task WithdrawAsync_ShouldThrowExceptionForNegativeAmount()
    {
        // Arrange
        var currentBalance = "200";
        var negativeAmount = "-50";
        _mockClient.Setup(x => x.GetBalanceAsync()).ReturnsAsync(new HttpResponseMessage
        {
            Content = new StringContent($"{{\"amount\": {currentBalance}}}"),
            StatusCode = HttpStatusCode.OK
        });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _walletServices.WithdrawAsync(negativeAmount));
        ex.Message.Should().Be("Withdrawal amount must be positive.");
        _mockClient.Verify(x => x.WithdrawAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task WithdrawAsync_ShouldNotWithdrawWithZeroAmountAndReturnCurrentBalance()
    {
        // Arrange
        var currentBalance = "200";
        var withdrawalAmount = "0"; // Withdrawal amount is zero

        _mockClient.Setup(x => x.GetBalanceAsync()).ReturnsAsync(new HttpResponseMessage
        {
            Content = new StringContent($"{{\"amount\": {currentBalance}}}"),
            StatusCode = HttpStatusCode.OK
        });

        // Act
        var result = await _walletServices.WithdrawAsync(withdrawalAmount);

        // Assert
        result.Should().Be(decimal.Parse(currentBalance)); // Ensure the returned amount is the same as the current balance
        _mockClient.Verify(x => x.WithdrawAsync(It.IsAny<string>()), Times.Never); // Ensure WithdrawAsync was not called
    }
}
