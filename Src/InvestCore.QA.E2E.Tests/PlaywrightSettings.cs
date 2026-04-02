namespace InvestCore.QA.E2E.Tests;

internal static class PlaywrightSettings
{
    public static string BaseUrl =>
        Environment.GetEnvironmentVariable("PLAYWRIGHT_BASE_URL") ?? "http://localhost:3000";

    public static string Username =>
        Environment.GetEnvironmentVariable("PLAYWRIGHT_USERNAME") ?? "aqa_candidate";

    public static string Password =>
        Environment.GetEnvironmentVariable("PLAYWRIGHT_PASSWORD") ?? "Password123!";

    public static string ManagerId =>
        Environment.GetEnvironmentVariable("PLAYWRIGHT_MANAGER_ID") ?? "MGR-555000";
}
