namespace EduCloud_v42.Models
{
    public class Course
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        // Навігаційні властивості
        public virtual ICollection<UserCourse> UserCourses { get; set; }
        public virtual ICollection<CourseElement> CourseElements { get; set; }
    }
}
