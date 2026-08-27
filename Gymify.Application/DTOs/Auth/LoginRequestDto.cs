using System.ComponentModel.DataAnnotations;

namespace Gymify.Application.DTOs.Auth
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Required")]
        [EmailAddress(ErrorMessage = "EmailInvalid")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "PasswordLength")]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}
