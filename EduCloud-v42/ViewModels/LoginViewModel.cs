using System.ComponentModel.DataAnnotations;

namespace EduCloud_v42.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Необхідно вказати ім'я користувача")]
        [Display(Name = "Ім'я користувача")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Необхідно вказати пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }
    }
}
