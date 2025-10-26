namespace EduCloud_v42.Models
{
    public enum CourseElementType
    {
        Task,
        Lecture
    }
    public class CourseElement
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public CourseElementType Type { get; set; }

        // Зовнішній ключ до Course
        public int CourseId { get; set; }
        public virtual Course? Course { get; set; }

        // Навігаційні властивості
        public virtual List<CourseFile> CourseFiles { get; set; } = new();
        public virtual List<UserTask> UserTasks { get; set; } = new();
    }
}
