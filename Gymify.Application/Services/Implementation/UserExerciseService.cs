using Gymify.Application.DTOs.UserExercise;
using Gymify.Application.Services.Interfaces;
using Gymify.Data.Entities;
using Gymify.Data.Enums;
using Gymify.Data.Interfaces.Repositories;

namespace Gymify.Application.Services.Implementation;

public class UserExerciseService(IUnitOfWork unitOfWork, INotificationService notificationService) : IUserExersiceService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly INotificationService _notificationService = notificationService;
    private const int DefaultPendingExerciseXP = 10;

    public async Task SyncWorkoutExercisesAsync(Guid workoutId, List<AddUserExerciseDto> dtos, Guid userId, bool ukranianVer)
    {
        var currentExercises = await _unitOfWork.UserExerciseRepository
            .GetAllByWorkoutIdAsync(workoutId);

        var incomingIds = dtos.Select(d => d.Id).ToHashSet();

        var toDelete = currentExercises
            .Where(e => !incomingIds.Contains(e.Id))
            .ToList();

        if (toDelete.Any())
        {
            await _unitOfWork.UserExerciseRepository.DeleteRangeAsync(toDelete);
        }

        foreach (var dto in dtos)
        {
            var existingEntity = currentExercises.FirstOrDefault(e => e.Id == dto.Id);
            var durationTimespan = TimeSpan.FromMinutes(dto.Duration ?? 0);

            var calcModel = new AddUserExerciseDto
            {
                Sets = dto.Sets,
                Reps = dto.Reps ?? 0,
                Weight = dto.Weight ?? 0.0, 
                Duration = dto.Duration ?? 0
            };

            if (existingEntity != null)
            {
                existingEntity.Sets = dto.Sets;
                existingEntity.Reps = dto.Reps ?? 0;
                existingEntity.Weight = dto.Weight ?? 0.0;
                existingEntity.Duration = durationTimespan;

                if (existingEntity.Exercise != null)
                {
                    existingEntity.EarnedXP = CalculateXp(calcModel, existingEntity.Exercise);
                }

                await _unitOfWork.UserExerciseRepository.UpdateAsync(existingEntity);
            }
            else
            {
                var baseExercise = await _unitOfWork.ExerciseRepository.GetByNameAsync(dto.Name, ukranianVer);

                if (baseExercise == null)
                {
                    baseExercise = new Exercise
                    {
                        Id = Guid.NewGuid(),
                        NameEn = ukranianVer ? string.Empty : dto.Name,
                        DescriptionEn = string.Empty,
                        NameUk = ukranianVer ? dto.Name : string.Empty,
                        DescriptionUk = string.Empty,
                        Type = (ExerciseType)dto.ExerciseType,
                        BaseXP = DefaultPendingExerciseXP, 
                        DifficultyMultiplier = 1.0,
                        IsApproved = false,
                        IsRejected = false
                    };
                    await _unitOfWork.ExerciseRepository.CreateAsync(baseExercise);

                    await _notificationService.SendNotificationAsync(
                        userId,
                        $"Exercise '{dto.Name}' was sent for a moderation.",
                        $"Вправа '{dto.Name}' була відправлена на модерацію.",
                        "#"
                    );
                }

                var newEntity = new UserExercise
                {
                    Id = dto.Id,
                    WorkoutId = workoutId,
                    ExerciseId = baseExercise.Id,
                    NameEn = baseExercise.NameEn,
                    NameUk = baseExercise.NameUk,
                    Type = baseExercise.Type,
                    Sets = dto.Sets,
                    Reps = dto.Reps ?? 0,
                    Weight = dto.Weight ?? 0.0,
                    Duration = durationTimespan,
                    EarnedXP = CalculateXp(calcModel, baseExercise)
                };

                await _unitOfWork.UserExerciseRepository.CreateAsync(newEntity);
            }
        }

        await _unitOfWork.SaveAsync();
    }

    public async Task<List<UserExerciseDto>> GetAllWorkoutExercisesAsync(Guid workoutId, bool ukranianVer)
    {
        var userExercises = await _unitOfWork.UserExerciseRepository.GetAllByWorkoutIdAsync(workoutId);

        List<UserExerciseDto> userExerciseDtos = new();

        foreach(var userExercise in userExercises)
        {
            userExerciseDtos.Add(new UserExerciseDto
            {
                Id = userExercise.Id,
                WorkoutId = userExercise.WorkoutId,
                Name = ukranianVer ? userExercise.NameUk : userExercise.NameEn,
                Type = (int)userExercise.Type,
                Sets = userExercise.Sets,
                Reps = userExercise.Reps,
                Weight = userExercise.Weight,
                Duration = userExercise.Duration,
                EarnedXP = userExercise.EarnedXP,
            });
        }
        return userExerciseDtos;
    }

    public static int CalculateXp(AddUserExerciseDto exerciseModel, Exercise userExercise)
    {
        int sets = exerciseModel.Sets ?? 0;
        int reps = exerciseModel.Reps ?? 0;
        double weight = exerciseModel.Weight ?? 0;
        double minutes = exerciseModel.Duration ?? 0;

        double factor = 1.0;

        if (weight > 0 && minutes == 0)
        {
            factor += (double)weight / 50.0; 
        }
        else if (minutes > 0 && weight == 0)
        {
            factor += minutes / 10.0; 
        }

        double xp = userExercise.DifficultyMultiplier * sets * Math.Max(reps, 1) * factor;

        return (int)Math.Max(xp, userExercise.BaseXP);
    }
}

