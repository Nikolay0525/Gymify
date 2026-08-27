using Gymify.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gymify.Web.Controllers
{
    [Authorize]
    public class ChatController : BaseController
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst("UserProfileId");
            if (userIdClaim == null) return RedirectToAction("Login", "Account"); 

            var userId = Guid.Parse(userIdClaim.Value);

            var chats = await _chatService.GetUserChatsAsync(userId);

            return View(chats);
        }

        [HttpGet]
        public async Task<IActionResult> Start(Guid userId)
        {
            var currentUserId = Guid.Parse(User.FindFirst("UserProfileId")?.Value ?? Guid.Empty.ToString());

            var chatId = await _chatService.GetOrCreatePrivateChatAsync(currentUserId, userId);

            return RedirectToAction("Index", new { openChatId = chatId });
        }

        [HttpGet]
        public async Task<IActionResult> GetMyChats()
        {
            var userId = Guid.Parse(User.FindFirst("UserProfileId")?.Value ?? Guid.Empty.ToString());
            var chats = await _chatService.GetUserChatsAsync(userId);
            return Ok(chats);
        }

        [HttpGet]
        public async Task<IActionResult> GetHistory(Guid chatId)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst("UserProfileId")?.Value ?? Guid.Empty.ToString());
                var messages = await _chatService.GetChatHistoryAsync(chatId, userId);
                return Ok(messages);
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead([FromBody] Guid chatId)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst("UserProfileId")?.Value ?? Guid.Empty.ToString());
                await _chatService.MarkChatAsReadAsync(chatId, userId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
