using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Gymify.Data.Entities;
using Gymify.Application.ViewModels.Home;
using Gymify.Application.Services.Interfaces;
using Gymify.Application.Services.Implementation;
using Gymify.Data.Enums;
using Gymify.Application.ViewModels.ExerciseLibrary;
using Microsoft.AspNetCore.Localization;

namespace Gymify.Web.Controllers
{
    public class MainController(SignInManager<ApplicationUser> signInManager, IUserProfileService userProfileService, ILeaderboardService leaderboardService, IExerciseService exerciseService) : BaseController
    {
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly IUserProfileService _userProfileService = userProfileService;
        private readonly ILeaderboardService _leaderboardService = leaderboardService;
        private readonly IExerciseService _exerciseService = exerciseService;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (_signInManager.IsSignedIn(User))
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Home", "Admin");
                }
                else if (User.IsInRole("User"))
                {
                    var user = Guid.Parse(User.FindFirst("UserProfileId")?.Value ?? Guid.Empty.ToString());

                    var viewModel = await _userProfileService.ReceiveUserLevelWorkouts(user);

                    return View("Home", viewModel);
                }

                return View("Start");
            }
            else
            {
                return View("Start");
            }
        }


        [HttpGet("faq")]
        public IActionResult FAQ()
        {
            return View("FAQ");
        }
        [HttpGet("leaderboard")]
        public async Task<IActionResult> Leaderboard(int page = 1)
        {
            var userId = Guid.Parse(User.FindFirst("UserProfileId")?.Value ?? Guid.Empty.ToString());

            var model = await _leaderboardService.GetLeaderboardAsync(userId, page);

            return View(model);
        }

        [HttpGet("exerciselibrary")]
        public async Task<IActionResult> Exercises(string search, ExerciseType? type, bool pendingOnly = false, int page = 1)
        {
            var result = await _exerciseService.GetFilteredExercisesAsync(search, type, pendingOnly, page, 20, IsUkrainian);

            var model = new ExerciseLibraryViewModel
            {
                Exercises = result.Exercises, 
                SearchTerm = search,
                TypeFilter = type,
                ShowPendingOnly = pendingOnly,
                CurrentPage = page,
                TotalPages = result.TotalPages
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> FilterExercises(string search, ExerciseType? type, bool pendingOnly, int page = 1)
        {
            var result = await _exerciseService.GetFilteredExercisesAsync(search, type, pendingOnly, page, 20, IsUkrainian);

            var model = new ExerciseLibraryViewModel
            {
                Exercises = result.Exercises,
                SearchTerm = search,
                TypeFilter = type,
                ShowPendingOnly = pendingOnly,
                CurrentPage = page,
                TotalPages = result.TotalPages
            };

            return PartialView("_ExerciseGrid", model);
        }

        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }
    }
}
