using Gymify.Application.Services.Implementation;
using Gymify.Application.Services.Interfaces;
using Gymify.Application.ViewModels.Friends;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gymify.Web.Controllers
{
    [Authorize]
    public class FriendsController : BaseController
    {
        private readonly IFriendsService _friendsService;

        public FriendsController(IFriendsService friendsService)
        {
            _friendsService = friendsService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = Guid.Parse(User.FindFirst("UserProfileId").Value);

            var model = new FriendsViewModel
            {
                Friends = await _friendsService.GetFriendsAsync(userId),
                IncomingRequests = await _friendsService.GetIncomingInvitesAsync(userId),
                OutgoingRequests = await _friendsService.GetOutgoingInvitesAsync(userId) 
            };

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            var userId = Guid.Parse(User.FindFirst("UserProfileId").Value);
            var results = await _friendsService.SearchPotentialFriendsAsync(query, userId);
            return Ok(results);
        }

        [HttpPost]
        public async Task<IActionResult> SendRequest(Guid receiverId)
        {
            var userId = Guid.Parse(User.FindFirst("UserProfileId").Value);
            try
            {
                await _friendsService.SendFriendRequestAsync(userId, receiverId);
                return Ok();
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }
        [HttpPost]
        public async Task<IActionResult> Cancel(Guid receiverId)
        {
            var userId = Guid.Parse(User.FindFirst("UserProfileId").Value);
            await _friendsService.CancelFriendRequestAsync(receiverId, userId);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Accept(Guid senderId)
        {
            var userId = Guid.Parse(User.FindFirst("UserProfileId").Value);
            await _friendsService.AcceptFriendRequestAsync(senderId, userId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Decline(Guid senderId)
        {
            var userId = Guid.Parse(User.FindFirst("UserProfileId").Value);
            await _friendsService.DeclineFriendRequestAsync(senderId, userId);
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> RemoveFriend(Guid friendId)
        {
            try
            {
                var currentUserId = Guid.Parse(User.FindFirst("UserProfileId").Value);
                await _friendsService.RemoveFriendAsync(currentUserId, friendId);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
