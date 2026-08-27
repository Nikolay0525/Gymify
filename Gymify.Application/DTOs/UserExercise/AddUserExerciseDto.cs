using System.ComponentModel.DataAnnotations;

namespace Gymify.Application.DTOs.UserExercise;

public class AddUserExerciseDto
{
    [Required]
    public Guid Id { get; set; }
    
    [Required]
    public Guid WorkoutId { get; set; }

    [Required(ErrorMessage = "Msg_ExerciseNameRequired")]
    [MinLength(3, ErrorMessage = "Msg_NameMinLength")]   
    [MaxLength(100, ErrorMessage = "Msg_NameMaxLength")] 
    public string Name { get; set; }

    [Range(0, 100, ErrorMessage = "Msg_SetsRange")]
    public int? Sets { get; set; }

    [Range(0, 1000, ErrorMessage = "Msg_RepsRange")]
    public int? Reps { get; set; } = 0;

    [Range(0.0, 500.0, ErrorMessage = "Msg_WeightRange")]
    public double? Weight { get; set; } = 0.0;

    [Range(0.0, 360, ErrorMessage = "Msg_DurationRange")]
    public double? Duration { get; set; } = 0.0;

    public int ExerciseType { get; set; } = 1;

}
