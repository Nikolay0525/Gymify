using Gymify.Application.DTOs.Achievement;
using Gymify.Application.DTOs.Workout;
using Gymify.Application.Services.Interfaces;
using Gymify.Application.ViewModels.Comment;
using Gymify.Application.ViewModels.Home;
using Gymify.Application.ViewModels.UserProfile;
using Gymify.Data.Entities;
using Gymify.Data.Enums;
using Gymify.Data.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Gymify.Application.Services.Implementation;

public class UserProfileService
    (IUnitOfWork unitOfWork, ILevelingService levelingService, IUserEquipmentService userEquipmentService, ICommentService commentService, UserManager<ApplicationUser> userManager) : IUserProfileService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILevelingService _levelingService = levelingService;
    private readonly IUserEquipmentService _userEquipmentService = userEquipmentService;
    private readonly ICommentService _commentService = commentService;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<HomeViewModel> ReceiveUserLevelWorkouts(Guid userId)
    {
        var user = await _unitOfWork.UserProfileRepository.GetByIdAsync(userId);
        var userWorkouts = await _unitOfWork.WorkoutRepository.GetLastWorkouts(userId);

        List<WorkoutDto> workoutsDtos = new();

        foreach(var workout in userWorkouts)
        {
            workoutsDtos.Add(new WorkoutDto
            {
                Id = workout.Id,
                Name = workout.Name,
                Description = workout.Description,
                CreatedAt = workout.CreatedAt,
                TotalXP = workout.TotalXP
            });
        }

        int currentLevel = _levelingService.CalculateLevel(user.CurrentXP);

        double totalXpForCurrentLevel = _levelingService.GetTotalXpForLevel(currentLevel);
        double totalXpForNextLevel = _levelingService.GetTotalXpForLevel(currentLevel + 1);

        double xpNeededForThisLevel = totalXpForNextLevel - totalXpForCurrentLevel;
        double xpEarnedInThisLevel = user.CurrentXP - totalXpForCurrentLevel;

        double progressPercentage = (xpNeededForThisLevel > 0)
            ? (xpEarnedInThisLevel / xpNeededForThisLevel) * 100
            : 0;

        var viewModel = new HomeViewModel
        {
            Level = currentLevel, 
            XpEarnedInThisLevel = (int)xpEarnedInThisLevel,
            XpNeededForThisLevel = (int)xpNeededForThisLevel,
            ProgressPercentage = progressPercentage,
            LastWorkouts = workoutsDtos
        };

        return viewModel;
    }

    public async Task AddXPAsync(Guid userProfileId, int earnedXp)
    {
        var user = await _unitOfWork.UserProfileRepository.GetByIdAsync(userProfileId);

        user.CurrentXP += earnedXp;

        int newLevel = _levelingService.CalculateLevel(user.CurrentXP);
        user.Level = newLevel;

        await _unitOfWork.UserProfileRepository.UpdateAsync(user);
        await _unitOfWork.SaveAsync();
    }

    public async Task UpdateStatsAsync(Guid userProfileId, Guid workoutId)
    {
        var user = await _unitOfWork.UserProfileRepository.GetByIdAsync(userProfileId);
        var workout = await _unitOfWork.WorkoutRepository.GetByIdWithDetailsAsync(workoutId);

        if (user == null || workout == null)
            throw new Exception("User or workout not found.");

        user.TotalWorkouts += 1;
        user.TotalWeightLifted += workout.Exercises.Sum(e => e.Weight ?? 0);

        user.TotalKmRunned += workout.Exercises
            .Where(e => e.Type == ExerciseType.Cardio)
            .Sum(e => (int)(e.Duration?.TotalMinutes ?? 0) / 10);

        int strengthCount = workout.Exercises.Count(e => e.Type == ExerciseType.Strength);
        int cardioCount = workout.Exercises.Count(e => e.Type == ExerciseType.Cardio);
        int flexibilityCount = workout.Exercises.Count(e => e.Type == ExerciseType.Flexibility);
        int balanceCount = workout.Exercises.Count(e => e.Type == ExerciseType.Balance);
        int enduranceCount = workout.Exercises.Count(e => e.Type == ExerciseType.Endurance);
        int mobilityCount = workout.Exercises.Count(e => e.Type == ExerciseType.Mobility);

        user.StrengthExercisesCompleted += strengthCount;
        user.CardioExercisesCompleted += cardioCount;
        user.FlexibilityExercisesCompleted += flexibilityCount;
        user.BalanceExercisesCompleted += balanceCount;
        user.EnduranceExercisesCompleted += enduranceCount;
        user.MobilityExercisesCompleted += mobilityCount;

        if (workout.CreatedAt.Date == DateTime.UtcNow.Date)
            user.WorkoutStreak += 1;
        else
            user.WorkoutStreak = 0;

        await _unitOfWork.UserProfileRepository.UpdateAsync(user);
        await _unitOfWork.SaveAsync();
    }

    public async Task<List<AchievementDto>> GetCompletedAchivementsOfUser(Guid userProfileId, bool ukranianVer)
    {
        var userAchievements = await _unitOfWork.UserAchievementRepository.GetAllByUserId(userProfileId);

        List<AchievementDto> achievementDtos = new();
        foreach (var userAchievement in userAchievements)
        {
            if (!userAchievement.IsCompleted) continue;
            achievementDtos.Add(new AchievementDto
            {
                AchievementId = userAchievement.AchievementId,
                Name = ukranianVer ? userAchievement.Achievement.NameUk : userAchievement.Achievement.NameEn,
                Description = ukranianVer ? userAchievement.Achievement.DescriptionUk : userAchievement.Achievement.DescriptionEn,
                IconUrl = userAchievement.Achievement.IconUrl,
                ComparisonType = userAchievement.Achievement.ComparisonType,
                RewardItemId = userAchievement.Achievement.RewardItemId,
                Progress = userAchievement.Progress,
                TargetProperty = userAchievement.Achievement.TargetProperty,
                TargetValue = userAchievement.Achievement.TargetValue,
                IsCompleted = userAchievement.IsCompleted,
                UnlockedAt = userAchievement.UnlockedAt
            });
        }

        return achievementDtos;
    }

    public async Task<List<WorkoutDto>> GetLastWorkoutsOfUser(Guid userProfileId)
    {
        var userWorkouts = await _unitOfWork.WorkoutRepository.GetLastWorkouts(userProfileId, 28);

        List<WorkoutDto> workoutsDtos = new();

        foreach (var workout in userWorkouts)
        {
            workoutsDtos.Add(new WorkoutDto
            {
                Id = workout.Id,
                Name = workout.Name,
                Description = workout.Description,
                CreatedAt = workout.CreatedAt,
                TotalXP = workout.TotalXP
            });
        }

        return workoutsDtos;
    }


    public async Task<UserProfileViewModel> GetUserProfileModel(Guid currentUserProfileId,Guid userProfileId, bool ukranianVer)
    {
        var currentUser = await _unitOfWork.UserProfileRepository.GetAllCredentialsAboutUserByIdAsync(currentUserProfileId);
        if (currentUser == null) throw new Exception("Current user profile not found");         var avatar = await _unitOfWork.ItemRepository.GetByIdAsync(currentUser.Equipment.AvatarId);

        var userCredentials = await _unitOfWork.UserProfileRepository.GetAllCredentialsAboutUserByIdAsync(userProfileId);
        if (userCredentials == null) throw new NullReferenceException($"When we were looking for userCredentials by '{userProfileId}' id we not found according application user");

        var userEquipment = await _userEquipmentService.GetUserEquipmentAsync(userProfileId, ukranianVer);
        var userAchievements = await GetCompletedAchivementsOfUser(userProfileId, ukranianVer);
        var userWorkouts = await GetLastWorkoutsOfUser(userProfileId);

        return new UserProfileViewModel
        {
            UserProfileId = userProfileId,
            Level = userCredentials.Level,
            UserName = userCredentials.ApplicationUser!.UserName ?? "Name",
            Title = userEquipment.TitleText,
            Achievements = userAchievements,
            Workouts = userWorkouts,
            UserEquipmentDto = userEquipment,
            UpdateUserEquipmentDto = new(),
            CurrentUserAvatarUrl = avatar?.ImageURL ?? "https://localhost:7102/Images/DefaultAvatar.png",
            Comments = new CommentsSectionViewModel
            {
                TargetId = userProfileId,
                TargetType = Data.Enums.CommentTargetType.UserProfile,
                CommentDtos = await _commentService.GetCommentDtos(currentUserProfileId, userProfileId, Data.Enums.CommentTargetType.UserProfile),
                CurrentUserAvatarUrl = avatar?.ImageURL ?? "https://localhost:7102/Images/DefaultAvatar.png",
            }
        };
    }

    public async Task UpdateUserNameAsync(Guid userProfileId, string userName)
    {
        userName = userName.Trim();

        var userProfile = await _unitOfWork.UserProfileRepository.GetAllCredentialsAboutUserByIdAsync(userProfileId);
        if (userProfile == null) throw new Exception("User not found");

        var user = await _userManager.FindByIdAsync(userProfile.ApplicationUserId.ToString());
        if (user == null) throw new Exception("User not found");

        if (user.UserName == userName) return;

        var existingUser = await _userManager.FindByNameAsync(userName);
        if (existingUser != null)
        {
            throw new Exception($"Username '{userName}' is already taken.");
        }

        var result = await _userManager.SetUserNameAsync(user, userName);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"Failed: {errors}");
        }
    }

}
