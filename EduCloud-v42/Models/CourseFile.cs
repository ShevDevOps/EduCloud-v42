namespace EduCloud_v42.Models
{
    public class CourseFile
    {
        public int ID { get; set; } // PK
        public string Path { get; set; }
        public string Name { get; set; }

        // Зовнішній ключ до Course Element
        public int CourseElementId { get; set; }
        public virtual CourseElement CourseElement { get; set; }
    }
}
