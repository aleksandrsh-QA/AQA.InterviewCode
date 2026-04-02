using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace InvestCore.QA.Tests.Api;

// Domain Models
public record AccountInfo(string AccountId, decimal Balance);
public record InvestmentInfo(string Id, string Status, decimal Amount);

public record InvestApiOptions
{
    public string BaseUrl { get; init; } = string.Empty;
    public string TestManagerId { get; init; } = string.Empty;
}

// API Route Interfaces
public interface IAccountsClient
{
    Task<AccountInfo> GetAccountAsync(string accountId);
    Task<string> CreateDemoAccountAsync(decimal initialBalance);
    Task DeleteAsync(string accountId);
}

public interface IInvestmentsClient
{
    Task<InvestmentInfo> InvestAsync(string investorId, string managerId, decimal amount);
    Task CancelAsync(string investmentId);
    Task<InvestmentInfo> GetByIdAsync(string investmentId);
}

// API Facade
public interface IInvestApiClient
{
    IAccountsClient Accounts { get; }
    IInvestmentsClient Investments { get; }
}

// API Factory
public interface IApiClientFactory
{
    IInvestApiClient CreateClient(string role);
}

internal sealed class InMemoryInvestStore
{
    public ConcurrentDictionary<string, decimal> Accounts { get; } = new();
    public ConcurrentDictionary<string, InvestmentRecord> Investments { get; } = new();
}

internal sealed class InvestmentRecord
{
    public required string Id { get; init; }
    public required string InvestorId { get; init; }
    public required string ManagerId { get; init; }
    public required decimal Amount { get; init; }
    public string Status { get; set; } = "Pending";
}

internal sealed class AccountsClient(InMemoryInvestStore store) : IAccountsClient
{
    public Task<AccountInfo> GetAccountAsync(string accountId)
    {
        if (!store.Accounts.TryGetValue(accountId, out var balance))
        {
            throw new InvalidOperationException($"Account '{accountId}' was not found.");
        }

        return Task.FromResult(new AccountInfo(accountId, balance));
    }

    public Task<string> CreateDemoAccountAsync(decimal initialBalance)
    {
        if (initialBalance < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(initialBalance), "Initial balance cannot be negative.");
        }

        var accountId = $"demo_{Guid.NewGuid():N}";
        store.Accounts[accountId] = initialBalance;

        return Task.FromResult(accountId);
    }

    public Task DeleteAsync(string accountId)
    {
        store.Accounts.TryRemove(accountId, out _);
        return Task.CompletedTask;
    }
}

internal sealed class InvestmentsClient(InMemoryInvestStore store) : IInvestmentsClient
{
    public Task<InvestmentInfo> InvestAsync(string investorId, string managerId, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(investorId))
        {
            throw new ArgumentException("Investor id is required.", nameof(investorId));
        }

        if (string.IsNullOrWhiteSpace(managerId))
        {
            throw new ArgumentException("Manager id is required.", nameof(managerId));
        }

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Investment amount should be greater than zero.");
        }

        if (!store.Accounts.TryGetValue(investorId, out var balance))
        {
            throw new InvalidOperationException($"Account '{investorId}' was not found.");
        }

        if (balance < amount)
        {
            throw new InvalidOperationException("Insufficient funds for investment.");
        }

        var investment = new InvestmentRecord
        {
            Id = Guid.NewGuid().ToString("N"),
            InvestorId = investorId,
            ManagerId = managerId,
            Amount = amount
        };

        store.Investments[investment.Id] = investment;

        _ = ProcessInvestmentAsync(investment);

        return Task.FromResult(new InvestmentInfo(investment.Id, investment.Status, investment.Amount));
    }

    public Task CancelAsync(string investmentId)
    {
        if (!store.Investments.TryGetValue(investmentId, out var investment))
        {
            throw new InvalidOperationException($"Investment '{investmentId}' was not found.");
        }

        investment.Status = "Cancelled";
        return Task.CompletedTask;
    }

    public Task<InvestmentInfo> GetByIdAsync(string investmentId)
    {
        if (!store.Investments.TryGetValue(investmentId, out var investment))
        {
            throw new InvalidOperationException($"Investment '{investmentId}' was not found.");
        }

        return Task.FromResult(new InvestmentInfo(investment.Id, investment.Status, investment.Amount));
    }

    private async Task ProcessInvestmentAsync(InvestmentRecord investment)
    {
        await Task.Delay(150);

        if (investment.Status == "Cancelled")
        {
            return;
        }

        store.Accounts.AddOrUpdate(
            investment.InvestorId,
            _ => throw new InvalidOperationException($"Account '{investment.InvestorId}' was not found."),
            (_, currentBalance) => currentBalance - investment.Amount);

        investment.Status = "Success";
    }
}

internal sealed class InvestApiClient(
    IAccountsClient accountsClient,
    IInvestmentsClient investmentsClient) : IInvestApiClient
{
    public IAccountsClient Accounts { get; } = accountsClient;
    public IInvestmentsClient Investments { get; } = investmentsClient;
}

internal sealed class ApiClientFactory(IServiceProvider serviceProvider) : IApiClientFactory
{
    public IInvestApiClient CreateClient(string role)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role);
        return serviceProvider.GetRequiredService<IInvestApiClient>();
    }
}

// DI Setup
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInvestApiClient(this IServiceCollection services)
    {
        services
            .AddOptions<InvestApiOptions>()
            .BindConfiguration("InvestApi")
            .Validate(options => !string.IsNullOrWhiteSpace(options.BaseUrl), "InvestApi:BaseUrl is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.TestManagerId), "InvestApi:TestManagerId is required.");

        services.AddSingleton<InMemoryInvestStore>();
        services.AddSingleton<IAccountsClient, AccountsClient>();
        services.AddSingleton<IInvestmentsClient, InvestmentsClient>();
        services.AddSingleton<IInvestApiClient, InvestApiClient>();
        services.AddSingleton<IApiClientFactory, ApiClientFactory>();

        return services;
    }
}
