namespace Gymify.Tests;

[TestFixture]
public class WorkoutCreationTests : BaseTests
{
    [Test]
    public async Task UserCanCreateAndCompleteWorkout()
    {
        await PerformLoginWithTestData();

        await Expect(Page.Locator(".dashboard-wrapper")).ToBeVisibleAsync();

        await PerformFullWorkoutFlow();
    }

    [Test]
    public async Task UserCanRenameWorkoutAndAddExercisesToExisting()
    {
        await PerformLoginWithTestData();

        await PerformFullWorkoutFlow();

        await Page.GotoAsync(BaseUrl);

        await Page.Locator(".workout-card").First.ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex(@".*/Workout/Details.*"));

        await RenameWorkout();
        await AddExerciseToExistingWorkout();
    }

    private async Task RenameWorkout()
    {
        var uniqueSuffix = Guid.NewGuid().ToString("N").Substring(0, 5);
        var currentName = await Page.InputValueAsync("#inputName"); 
        var newName = $"{currentName} {uniqueSuffix}";

        await Page.ClickAsync("#btnEditInfo");

        await Page.FillAsync("#inputName", newName);

        await Page.ClickAsync("#btnEditInfo");

        await Expect(Page.Locator("#viewName")).ToHaveTextAsync(newName);
    }

    private async Task AddExerciseToExistingWorkout()
    {
        await Page.ClickAsync("#btnToggleEdit");

        await Expect(Page.Locator("#editModeContainer")).ToBeVisibleAsync();

        await Page.FillAsync("#mgrName", "Extra Exercise");
        await Page.FillAsync("#mgrSets", "5");
        await Page.FillAsync("#mgrReps", "5");
        await Page.FillAsync("#mgrWeight", "100");

        await Page.ClickAsync("#mgrBtnAdd");

        await Expect(Page.Locator("#mgrList .ae-item-card").Last).ToContainTextAsync("Extra Exercise");

        await Page.ClickAsync("#mgrBtnSave");

        await Expect(Page.Locator("#editModeContainer")).ToBeHiddenAsync();
        await Expect(Page.Locator("#viewModeContainer")).ToBeVisibleAsync();

        await Expect(Page.Locator("#viewModeContainer")).ToContainTextAsync("Extra Exercise");
    }

    private async Task PerformFullWorkoutFlow()
    {
        await Page.GotoAsync($"{BaseUrl}/Workout/Create");

        await Page.FillAsync("#Name", "Test Workout");
        await Page.FillAsync("#Description", "Automated test description");

        if (await Page.IsVisibleAsync("#isPrivateCheck"))
        {
            await Page.CheckAsync("#isPrivateCheck");
        }

        await Page.ClickAsync("form[action='/Workout/GenerateWorkout'] button[type='submit']");

        await Expect(Page).ToHaveURLAsync(new Regex(@".*/Workout/AddExercise.*"));

        await Page.FillAsync("#mgrName", "Bench Press");
        await Page.FillAsync("#mgrSets", "4");
        await Page.FillAsync("#mgrReps", "10");
        await Page.FillAsync("#mgrWeight", "60");
        await Page.FillAsync("#mgrDuration", "0");

        await Page.ClickAsync("#mgrBtnAdd");

        await Expect(Page.Locator(".ae-item-card")).ToHaveCountAsync(1);

        await Page.ClickAsync("#btnFinish");

        await Expect(Page).ToHaveURLAsync(new Regex(@".*/Workout/Finish.*"));

        await Page.FillAsync("#Conclusions", "Workout completed successfully.");

        await Page.ClickAsync("form[action*='/Workout/Finish'] button[type='submit']");

        var dashboardTitle = Page.Locator(".dashboard-title");
        await Expect(dashboardTitle).ToBeVisibleAsync();

        await Expect(Page.Locator(".level-badge")).ToBeVisibleAsync();
    }
}
