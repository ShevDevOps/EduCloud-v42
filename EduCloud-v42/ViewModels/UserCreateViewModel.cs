using EduCloud_v42.Models;
using System.ComponentModel.DataAnnotations;
using static EduCloud_v42.Controllers.UsersController;

namespace EduCloud_v42.ViewModels
{
    public class UserCreateViewModel
    {
        [Required]
        [StringLength(50)]
        [Unique("Username", ErrorMessage = "This username is already registered.")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Unique("Email", ErrorMessage = "This email is already registered.")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string? Phone { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }

        [Required]
        public UserRole Role { get; set; }
    }
}