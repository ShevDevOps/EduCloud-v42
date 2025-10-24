namespace EduCloud_v42.Models
{
    public enum CourseRole
    {
        Student,
        Teacher
    }
    public class UserCourse
    {
        public int UserId { get; set; } // PK, FK1
        public int CourseId { get; set; } // PK, FK2

        public CourseRole Role { get; set; }

        // Навігаційні властивості
        public virtual User User { get; set; }
        public virtual Course Course { get; set; }
    }
}
