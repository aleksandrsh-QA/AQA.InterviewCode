using AwesomeAssertions;
using InvestCore.QA.Tests.Api;
using InvestCore.QA.Tests.Fixtures;
using Xunit;

namespace InvestCore.QA.Tests.Tests;

[Collection("Api Integration Tests")]
public class InvestmentLifecycleTests(InvestApiFixture fixture)
{
    [Fact]
    public async Task Cancel_Should_Stop_Investment()
    {
        // Arrange
        var api = fixture.CreateClient();
        var investorId = await api.Accounts.CreateDemoAccountAsync(1_000m);
        var managerId = fixture.Options.TestManagerId;

        // Act
        var pending = await api.Investments.InvestAsync(investorId, managerId, 200m);
        await api.Investments.CancelAsync(pending.Id);

        Thread.Sleep(500);

        var result = await api.Investments.GetByIdAsync(pending.Id);

        // Assert
        result.Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task Concurrent_Investments_Should_Reduce_Balance_Correctly()
    {
        // Arrange
        var api = fixture.CreateClient();
        var investorId = await api.Accounts.CreateDemoAccountAsync(1_000m);
        var managerId = fixture.Options.TestManagerId;

        // Act
        var tasks = Enumerable
            .Range(0, 5)
            .Select(_ => api.Investments.InvestAsync(investorId, managerId, 200m))
            .ToArray();

        await Task.WhenAll(tasks);
        await Task.Delay(500);

        var account = await api.Accounts.GetAccountAsync(investorId);

        // Assert
        account.Balance.Should().Be(0m);
    }

    [Theory]
    [InlineData("", "MGR-1", 100, "Investor id is required.")]
    [InlineData("inv-1", "", 100, "Manager id is required.")]
    [InlineData("inv-1", "MGR-1", 0, "amount should be greater than zero")]
    [InlineData("inv-1", "MGR-1", -50, "amount should be greater than zero")]
    public async Task CreateInvestment_WithInvalidInputs_Should_Throw(
        string investorId,
        string managerId,
        int amount,
        string expectedMessage)
    {
        // Arrange
        var api = fixture.CreateClient();

        // Act
        var action = async () => await api.Investments.InvestAsync(investorId, managerId, amount);

        // Assert
        var ex = await action.Should().ThrowAsync<Exception>();
        ex.Which.Message.Should().Contain(expectedMessage);
    }
}
