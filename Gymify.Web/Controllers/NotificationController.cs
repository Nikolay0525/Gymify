using Gymify.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gymify.Web.Controllers
{
    [Authorize]
    public class NotificationController : BaseController
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost]
        public async Task<IActionResult> Read(Guid id)
        {
            try
            {
                await _notificationService.MarkAsReadAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReadAll()
        {
            try
            {
                var userIdClaim = User.FindFirst("UserProfileId");

                if (userIdClaim == null)
                {
                    return Unauthorized();
                }

                var userId = Guid.Parse(userIdClaim.Value);

                await _notificationService.MarkAllAsReadAsync(userId);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}