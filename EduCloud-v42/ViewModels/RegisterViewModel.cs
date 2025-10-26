using System.ComponentModel.DataAnnotations;

namespace EduCloud_v42.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Необхідно вказати ім'я користувача")]
        [Display(Name = "Ім'я користувача")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Необхідно вказати повне ім'я")]
        [Display(Name = "Повне ім'я")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Необхідно вказати Email")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Неправильний формат номеру телефону")]
        [Display(Name = "Телефон (необов'язково)")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Необхідно вказати пароль")]
        [StringLength(100, ErrorMessage = "{0} повинен містити принаймні {2} символів.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Підтвердження паролю")]
        [Compare("Password", ErrorMessage = "Пароль та підтвердження паролю не співпадають.")]
        public string ConfirmPassword { get; set; }
    }
}
