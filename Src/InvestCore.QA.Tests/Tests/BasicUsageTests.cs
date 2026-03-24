using AwesomeAssertions;
using InvestCore.QA.Tests.Api;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace InvestCore.QA.Tests.Tests;

public class BasicUsageTests
{
    private const string BaseApiUrl = "http://api.investcore.local";

    [Fact]
    public async Task CreateInvestment_Should_Decrease_Investor_Balance()
    {
        // Arrange
        var api = CreateInvestorClient();
        string investorId = "test_user_001";
        string managerId = "top_manager_01";
        decimal investAmount = 500m;

        // Act
        var response = await api.Investments.InvestAsync(investorId, managerId, investAmount);

        // Wait for the trading core to process the transaction
        // Requirements: Processing should not take more than 3 seconds.
        Thread.Sleep(3000);

        // Assert
        var updatedAccount = await api.Accounts.GetAccountAsync(investorId);

        // Expecting exactly 9500 assuming the user always starts with 10000
        updatedAccount.Balance.Should().Be(9500m);
        response.Status.Should().Be("Success");
    }

    // Helpers
    private static IInvestApiClient CreateInvestorClient()
    {
        var services = new ServiceCollection();
        services.AddInvestApiClient();

        return services.BuildServiceProvider().GetRequiredService<IInvestApiClient>();
    }
}