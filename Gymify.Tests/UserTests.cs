using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gymify.Tests;

public class UserTests : BaseTests
{
    [Test]
    public async Task UserCanRegisterLoginAndViewDashboard()
    {
        var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
        var testEmail = $"user_{uniqueId}@test.com";
        var testUser = $"GymUser_{uniqueId}";
        var testPassword = "Password123!";

        await Page.GotoAsync($"{BaseUrl}/Register");

        await Page.FillAsync("#Email", testEmail);
        await Page.FillAsync("#UserName", testUser);
        await Page.FillAsync("#Password", testPassword);
        await Page.FillAsync("#ConfirmPassword", testPassword);

        await Page.ClickAsync("form[action='/register'] button[type='submit']");

        await Page.GotoAsync($"{BaseUrl}/Login");

        await Page.FillAsync("#Email", testEmail);
        await Page.FillAsync("#Password", testPassword);

        await Page.ClickAsync("form[action='/login'] button[type='submit']");

        var dashboardTitle = Page.Locator(".dashboard-title");
        await Expect(dashboardTitle).ToBeVisibleAsync();

        await Expect(Page.Locator(".level-badge")).ToBeVisibleAsync();
    }

    [Test]
    public async Task UserCanEditNicknameAndPostCommentOnProfile()
    {
        await PerformLoginWithTestData();

        await Expect(Page.Locator(".dashboard-wrapper")).ToBeVisibleAsync();

        await Page.ClickAsync("#userDropdown");
        await Page.ClickAsync("a[href^='/profile?userId=']");

        await Expect(Page.Locator("#profileRoot")).ToBeVisibleAsync();

        var newUserName = "User_" + Guid.NewGuid().ToString("N").Substring(0, 5);

        await Page.ClickAsync("#editToggleBtn");

        var nameInput = Page.Locator("#nameInput");
        await Expect(nameInput).ToBeVisibleAsync();

        await nameInput.FillAsync(newUserName);

        await Page.ClickAsync("#editToggleBtn");

        await Expect(nameInput).ToBeHiddenAsync();

        await Expect(Page.Locator("#nameView")).ToHaveTextAsync(newUserName);

        await Expect(Page.Locator("#navbarUserName")).ToHaveTextAsync(newUserName);

        var commentText = $"Profile check {Guid.NewGuid().ToString().Substring(0, 8)}";

        await Page.FillAsync("#coContentInput", commentText);

        var postBtn = Page.Locator("#coBtnPost");
        await Expect(postBtn).ToBeEnabledAsync(); 
        await postBtn.ClickAsync();

        var newComment = Page.Locator($".co-item:has-text('{commentText}')").First;

        await Expect(newComment).ToBeVisibleAsync();
    }
}
