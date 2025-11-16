using System.ComponentModel.DataAnnotations;

namespace EduCloud_v42.Models
{
    public enum UserRole
    {
        Admin,
        User
    }
    public class User
    {
        public int ID { get; set; }
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;
        [Required]
        [StringLength(500)]
        public string FullName { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        [StringLength(50)]
        public string Email { get; set; } = string.Empty;
        [Phone]
        public string? Phone { get; set; }
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }

        // Навігаційні властивості (для зв'язків)
        public virtual ICollection<UserCourse> UserCourses { get; set; } = new List<UserCourse>();
        public virtual ICollection<UserTask> UserTasks { get; set; } = new List<UserTask>();
    }
}
