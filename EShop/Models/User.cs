using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EShop.Models
{
    public enum UserRole
    {
        Admin,
        Customer
    }

    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required, MaxLength(100)]
        public string? FullName { get; set; }

        [Required, EmailAddress, MaxLength(150)]
        public string? Email { get; set; }

        [Required]
        public string? PasswordHash { get; set; } 

        [Required]
        [Column(TypeName = "nvarchar(20)")]
        public UserRole Role { get; set; } = UserRole.Customer;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLogin { get; set; }
    }
}
