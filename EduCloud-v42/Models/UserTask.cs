namespace EduCloud_v42.Models
{
    public class UserTask
    {
        // Композитний первинний ключ
        public int UserId { get; set; } // PK, FK1
        public int TaskId { get; set; } // PK, FK2 (посилається на CourseElement.ID)

        public string Mark { get; set; } // Оцінка

        // Навігаційні властивості
        public virtual User User { get; set; }
        public virtual CourseElement Task { get; set; } // Посилається на CourseElement
        public virtual ICollection<TaskFile> TaskFiles { get; set; }
    }
}
