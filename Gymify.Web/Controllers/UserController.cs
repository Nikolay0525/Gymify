using Gymify.Application.DTOs.User;
using Gymify.Application.DTOs.UserEquipment;
using Gymify.Application.Services.Implementation;
using Gymify.Application.Services.Interfaces;
using Gymify.Application.ViewModels.UserItems;
using Gymify.Data.Entities;
using Gymify.Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Threading.Tasks;

namespace Gymify.Web.Controllers
{
    [Route("")]
    [Authorize]
    public class UserController(
        IUserProfileService userProfileService,
        IItemService itemService,
        ICaseService caseService,
        IAchievementService achievementService,
        IUserEquipmentService userEquipmentService,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager) : BaseController
    {
        private readonly IUserProfileService _userProfileService = userProfileService;
        private readonly IUserEquipmentService _userEquipmentService = userEquipmentService;
        private readonly IItemService _itemService = itemService;
        private readonly ICaseService _caseService = caseService;
        private readonly IAchievementService _achievementService = achievementService;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;

        [HttpGet("profile")]
        public async Task<IActionResult> Profile(Guid userId)
        {
            var loggedUserId = Guid.Parse(User.FindFirst("UserProfileId")!.Value);
            var model = await _userProfileService.GetUserProfileModel(loggedUserId, userId, IsUkrainian);
            model.Editable = loggedUserId == userId ? true : false; 

            return View("Profile", model);
        }

        [HttpGet("userEquipment")]
        public async Task<IActionResult> GetInventory([FromQuery] string type)
        {
            var userId = Guid.Parse(User.FindFirst("UserProfileId")!.Value);

            if (string.IsNullOrWhiteSpace(type))
                return BadRequest("Missing type");

            var itemType = type.ToLower() switch
            {
                "avatar" => ItemType.Avatar,
                "frame" => ItemType.Frame,
                "background" => ItemType.Background,
                "title" => ItemType.Title,
                _ => throw new ArgumentException("Unknown item type: " + type),
            };
            var items = await _itemService.GetUserItemsWithTypeAsync(userId, itemType, true, IsUkrainian);

            var result = items.Select(i => new
            {
                id = i.Id,
                url = i.ImageURL,
                name = i.Name
            });

            return Ok(result);
        }

        [HttpPost("updateUserName")]
        public async Task<IActionResult> UpdateName([FromForm] UpdateUserNameRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errorMessage = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault();

                return BadRequest(errorMessage);
            }

            try
            {
                var userId = Guid.Parse(User.FindFirst("UserProfileId")?.Value ?? Guid.Empty.ToString());

                await _userProfileService.UpdateUserNameAsync(userId, request.UpdatedUserName);

                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    await _signInManager.RefreshSignInAsync(user);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("updateProfile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserEquipmentDto dto)
        {
            var userId = Guid.Parse(User.FindFirst("UserProfileId")!.Value);

            await _userEquipmentService.UpdateUserEquipmentAsync(userId, dto);

            return Ok();
        }

        [HttpGet("inventory")]
        public async Task<IActionResult> Inventory()
        {
            var userId = Guid.Parse(User.FindFirst("UserProfileId")!.Value);


            var items = await _itemService.GetAllUserItemsAsync(userId, true, IsUkrainian);
            var cases = await _caseService.GetAllUserCasesAsync(userId, IsUkrainian);

            var userItemsViewModel = new UserItemsViewModel()
            {
                Items = items,
                Cases = cases
            };

            return View("Inventory", userItemsViewModel);
        }

        [HttpPost]
        public IActionResult GoToCasePage(Guid caseId)
        {
            return RedirectToAction("Details", "Case", new { caseId = caseId });
        }

        [HttpGet("achievements")]
        public async Task<IActionResult> Achievements()
        {
            var userId = Guid.Parse(User.FindFirst("UserProfileId")!.Value);
            var achievements = await _achievementService.GetUserAchievementsAsync(userId, IsUkrainian);
            return View("Achievements", achievements);
        }

    }
}
