namespace Gymify.Tests;

public class BaseTests : PageTest
{
    protected const string BaseUrl = "https://localhost:7102";
    protected const string TestEmail = "user@gmail.com";
    protected const string TestPassword = "user123!";

    protected async Task PerformLoginWithTestData()
    {
        await Page.GotoAsync($"{BaseUrl}/Login");

        await Page.FillAsync("#Email", TestEmail);
        await Page.FillAsync("#Password", TestPassword);

        await Page.ClickAsync("form[action='/login'] button[type='submit']");
    }
}
