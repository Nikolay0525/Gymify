using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gymify.Tests;

public class CommentTests : BaseTests
{
    [Test]
    public async Task UserCanPostEditAndDeleteCommentFromFeed()
    {
        await PerformLoginWithTestData();

        await Expect(Page.Locator(".dashboard-wrapper")).ToBeVisibleAsync();

        await Page.GotoAsync($"{BaseUrl}/WorkoutsFeed");

        await Page.SelectOptionAsync("#filter-type", "all");

        var firstWorkoutCard = Page.Locator(".workout-card-row").First;
        await Expect(firstWorkoutCard).ToBeVisibleAsync();

        await firstWorkoutCard.ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex(@".*/Workout/Details.*"));

        var commentText = $"Test Comment {Guid.NewGuid().ToString().Substring(0, 6)}";

        await Page.FillAsync("#coContentInput", commentText);

        var postBtn = Page.Locator("#coBtnPost");
        await Expect(postBtn).ToBeEnabledAsync();
        await postBtn.ClickAsync();

        var commentItem = Page.Locator($".co-item:has-text('{commentText}')").First;
        await Expect(commentItem).ToBeVisibleAsync();

        await commentItem.Locator("button:has-text('Edit')").ClickAsync();

        var editTextarea = commentItem.Locator(".co-edit-textarea");
        await Expect(editTextarea).ToBeVisibleAsync();

        var updatedText = commentText + " (Edited)";
        await editTextarea.FillAsync(updatedText);

        await commentItem.Locator("button:has-text('Save')").ClickAsync();

        await Expect(commentItem.Locator(".co-text")).ToHaveTextAsync(updatedText);

        await commentItem.Locator("button.delete").ClickAsync();

        await Page.ClickAsync("button.ajs-button.ajs-ok");

        await Expect(commentItem).Not.ToBeVisibleAsync();
    }
}
