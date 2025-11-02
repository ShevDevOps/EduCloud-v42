using System.ComponentModel.DataAnnotations;

namespace EduCloud_v42.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Необхідно вказати ім'я користувача")]
        [StringLength(50, ErrorMessage = "Ім'я користувача не може перевищувати 50 символів.")]
        [Display(Name = "Ім'я користувача")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Необхідно вказати повне ім'я")]
        [StringLength(500, ErrorMessage = "Повне ім'я не може перевищувати 500 символів.")]
        [Display(Name = "Повне ім'я")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Необхідно вказати Email")]
        [EmailAddress(ErrorMessage = "Некоректний формат Email.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Неправильний формат номеру телефону")]
        [RegularExpression(@"^\+?3?8?(0\d{9})$", ErrorMessage = "Некоректний формат телефону. Очікується +380XXXXXXXXX або 0XXXXXXXXX.")]
        [Display(Name = "Телефон (необов'язково)")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Необхідно вказати пароль")]
        [StringLength(16, MinimumLength = 8, ErrorMessage = "Пароль має бути від 8 до 16 символів.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$",
            ErrorMessage = "Пароль має містити принаймні одну велику літеру, одну малу, одну цифру та один спеціальний символ.")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Підтвердження паролю")]
        [Compare("Password", ErrorMessage = "Паролі не співпадають.")]
        public string ConfirmPassword { get; set; }
    }
}
