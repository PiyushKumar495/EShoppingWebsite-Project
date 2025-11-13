using System.ComponentModel.DataAnnotations;

namespace EShop.Dtos
{
    public class UserRegisterDto
    {
        [Required, MaxLength(100)]
        public string? FullName { get; set; }

        [Required, EmailAddress]
        public string? Email { get; set; }

        [Required, MinLength(6), RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]).{8,}$",
        ErrorMessage = "Password must be at least 8 characters long and contain a letter, a number, and a special character.")]
        public string? Password { get; set; }

        [Required, Compare("Password", ErrorMessage = "Passwords do not match")]
        public string? ConfirmPassword { get; set; }
    }
}
