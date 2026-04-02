using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace InvestCore.QA.E2E.Tests.Tests;

public class InvestmentValidationTests : PageTest
{
    [Fact]
    [Trait("Category", "E2E")]
    [Trait("Interview", "Playwright")]
    public async Task UserCannotCreateInvestment_WhenAmountExceedsBalance()
    {
        await Page.GotoAsync("/login");
        await SignInAsync();

        await Page.GotoAsync("/investments/new");

        await Expect(Page.GetByTestId("available-balance")).ToHaveTextAsync("100.00 USD");

        await Page.GetByTestId("manager-select").SelectOptionAsync(PlaywrightSettings.ManagerId);
        await Page.GetByTestId("investment-amount").FillAsync("150");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Create investment" }).ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Alert)).ToContainTextAsync("Amount exceeds available balance");
        await Expect(Page.GetByTestId("available-balance")).ToHaveTextAsync("100.00 USD");
        await Expect(Page.GetByTestId("investment-history-row")).ToHaveCountAsync(0);
    }

    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            BaseURL = PlaywrightSettings.BaseUrl,
            ViewportSize = new ViewportSize
            {
                Width = 1440,
                Height = 900
            }
        };
    }

    private async Task SignInAsync()
    {
        await Page.GetByTestId("username").FillAsync(PlaywrightSettings.Username);
        await Page.GetByTestId("password").FillAsync(PlaywrightSettings.Password);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign in" }).ClickAsync();
    }
}
