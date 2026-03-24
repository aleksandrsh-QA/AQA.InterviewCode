using Microsoft.Extensions.DependencyInjection;

namespace InvestCore.QA.Tests.Api;

// Domain Models
public record AccountInfo(string AccountId, decimal Balance);
public record InvestmentInfo(string Id, string Status, decimal Amount);

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

// DI Setup
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInvestApiClient(this IServiceCollection services)
    {
        // Do some work
        return services;
    }
}