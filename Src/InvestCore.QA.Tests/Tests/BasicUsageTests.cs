using AwesomeAssertions;
using InvestCore.QA.Tests.Api;
using InvestCore.QA.Tests.Fixtures;
using Xunit;

namespace InvestCore.QA.Tests.Tests;

[Collection("Api Integration Tests")]
public class BasicUsageTests(InvestApiFixture fixture)
{
    [Fact]
    public async Task CreateInvestment_Should_Decrease_Investor_Balance()
    {
        // Arrange
        var api = fixture.CreateClient();
        var investorId = await api.Accounts.CreateDemoAccountAsync(10_000m);
        var managerId = fixture.Options.TestManagerId;
        const decimal investAmount = 500m;

        // Act
        var response = await api.Investments.InvestAsync(investorId, managerId, investAmount);
        var completedInvestment = await WaitUntilAsync(
            () => api.Investments.GetByIdAsync(response.Id),
            investment => investment.Status == "Success",
            TimeSpan.FromSeconds(3));

        // Assert
        var updatedAccount = await api.Accounts.GetAccountAsync(investorId);

        updatedAccount.Balance.Should().Be(9500m);
        completedInvestment.Status.Should().Be("Success");
    }

    [Fact]
    public async Task CreateInvestment_WithAmountGreaterThanBalance_Should_Fail()
    {
        // Arrange
        var api = fixture.CreateClient();
        var investorId = await api.Accounts.CreateDemoAccountAsync(100m);

        // Act
        var action = async () => await api.Investments.InvestAsync(investorId, fixture.Options.TestManagerId, 150m);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Insufficient funds*");
    }

    private static async Task<T> WaitUntilAsync<T>(
        Func<Task<T>> action,
        Func<T, bool> condition,
        TimeSpan timeout)
    {
        var startedAt = DateTime.UtcNow;

        while (DateTime.UtcNow - startedAt < timeout)
        {
            var result = await action();
            if (condition(result))
            {
                return result;
            }

            await Task.Delay(100);
        }

        throw new TimeoutException($"Condition was not met within {timeout.TotalSeconds} seconds.");
    }
}
