using InvestCore.QA.Tests.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace InvestCore.QA.Tests.Fixtures;

public class InvestApiFixture : IAsyncLifetime
{
    public IServiceProvider ServiceProvider { get; private set; }
    public IConfiguration Configuration { get; private set; }

    public ValueTask InitializeAsync()
    {
        Configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton(Configuration);
        services.AddInvestApiClient();

        ServiceProvider = services.BuildServiceProvider();

        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (ServiceProvider is IAsyncDisposable disposable)
        {
            await disposable.DisposeAsync();
        }
    }
}