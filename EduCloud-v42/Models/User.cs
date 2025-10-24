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
        public string Name { get; set; }
        public UserRole Role { get; set; }

        // Навігаційні властивості (для зв'язків)
        public virtual ICollection<UserCourse> UserCourses { get; set; }
        public virtual ICollection<UserTask> UserTasks { get; set; }
    }
}
