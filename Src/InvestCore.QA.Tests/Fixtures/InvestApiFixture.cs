using InvestCore.QA.Tests.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace InvestCore.QA.Tests.Fixtures;

public class InvestApiFixture : IAsyncLifetime
{
    public IServiceProvider ServiceProvider { get; private set; } = default!;
    public IConfiguration Configuration { get; private set; } = default!;
    public InvestApiOptions Options => ServiceProvider.GetRequiredService<IOptions<InvestApiOptions>>().Value;

    public IInvestApiClient CreateClient() => ServiceProvider.GetRequiredService<IInvestApiClient>();

    public Task InitializeAsync()
    {
        Configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton(Configuration);
        services.AddInvestApiClient();

        ServiceProvider = services.BuildServiceProvider();

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (ServiceProvider is IAsyncDisposable disposable)
        {
            await disposable.DisposeAsync();
        }
    }
}
