using Gymify.Application.DTOs.UserExercise;
using Gymify.Application.DTOs.Workout;
using Gymify.Application.Services.Interfaces;
using Gymify.Application.ViewModels.Workout;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;

namespace Gymify.Web.Controllers
{
    [Authorize]
    public class WorkoutController : BaseController
    {
        private readonly IWorkoutService _workoutService;
        private readonly IExerciseService _exerciseService;
        private readonly IUserExersiceService _userExerciseService;

        public WorkoutController(
            IWorkoutService workoutService,
            IExerciseService exerciseService,
            IUserExersiceService userExerciseService)
        {
            _workoutService = workoutService;
            _exerciseService = exerciseService;
            _userExerciseService = userExerciseService;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("Create");
        }

        [HttpPost]
        public async Task<IActionResult> GenerateWorkout(CreateWorkoutRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                NotifyModelStateErrors();

                return View("Create", dto);
            }

            var userProfileId = Guid.Parse(User.FindFirst("UserProfileId")?.Value ?? Guid.Empty.ToString());
            var workout = await _workoutService.CreateWorkoutAsync(dto, userProfileId);

            TempData["WorkoutId"] = workout.Id.ToString();
            return RedirectToAction("AddExercise");
        }
        
        [HttpPost]
        public async Task<IActionResult> RemoveWorkout(Guid workoutId)
        {
            try
            {
                var userProfileId = Guid.Parse(User.FindFirst("UserProfileId")?.Value ?? Guid.Empty.ToString());

                await _workoutService.RemoveWorkoutAsync(userProfileId, workoutId);

                return RedirectToAction("Index", "Main");
            }
            catch
            {
                return RedirectToAction("Index", "Main");
            }
        }

        [HttpGet]
        public IActionResult AddExercise(Guid? workoutId)
        {
            var id = workoutId ?? (TempData["WorkoutId"] != null ? Guid.Parse(TempData["WorkoutId"].ToString()) : Guid.Empty);

            if (id == Guid.Empty) return RedirectToAction("Create");

            ViewBag.WorkoutId = id;
            TempData.Keep("WorkoutId");

            return View(new List<UserExerciseDto>());
        }


        [HttpPost]
        public async Task<IActionResult> AddExercisesBatch(Guid workoutId, [FromForm] List<AddUserExerciseDto> exercises)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Any())
                                       .ToDictionary(
                                           kvp => kvp.Key,
                                           kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                                       );

                return BadRequest(new { success = false, message = "Validation Error", errors = errors });
            }

            try
            {
                var currentUserId = Guid.Parse(User.FindFirst("UserProfileId")?.Value ?? Guid.Empty.ToString());

                await _userExerciseService.SyncWorkoutExercisesAsync(workoutId, exercises, currentUserId, IsUkrainian);

                return Ok(new { success = true, message = "Exercise saved!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Error when saving: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchExercises(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Json(new List<string>());

            var exercises = await _exerciseService.FindByNameAsync(query, IsUkrainian);
            return Json(exercises.Select(e => e.Name));
        }

        [HttpGet]
        public IActionResult Finish(Guid workoutId)
        {
            var model = new CompleteWorkoutRequestDto
            {
                WorkoutId = workoutId,
                UserProfileId = Guid.Parse(User.FindFirst("UserProfileId")?.Value ?? Guid.Empty.ToString()),
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Finish(CompleteWorkoutRequestDto dto)
        {
            await _workoutService.CompleteWorkoutAsync(dto, IsUkrainian);
            return RedirectToAction("Index", "Main");
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid workoutId)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst("UserProfileId")?.Value);
                var model = await _workoutService.GetWorkoutDetailsViewModel(userId, workoutId, IsUkrainian);
                return View(model);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetExercisesPartial(Guid workoutId)
        {
            var exerciseDtos = await _userExerciseService.GetAllWorkoutExercisesAsync(workoutId, IsUkrainian);

            return PartialView("_ExerciseListReadOnly", exerciseDtos);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateWorkoutInfo([FromBody] UpdateWorkoutRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Any())
                                       .ToDictionary(
                                           kvp => kvp.Key,
                                           kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                                       );

                return BadRequest(new { success = false, message = "Validation Error", errors = errors });
            }

            try
            {
                var currentUserId = Guid.Parse(User.FindFirst("UserProfileId")?.Value ?? Guid.Empty.ToString());
                await _workoutService.UpdateWorkoutInfoAsync(dto, currentUserId);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}