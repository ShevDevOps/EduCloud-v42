namespace EduCloud_v42.Models
{
    public class TaskFile
    {
        public int ID { get; set; } // PK
        public string Path { get; set; }

        // Композитний зовнішній ключ до UserTask
        public int UserId { get; set; }
        public int TaskId { get; set; }

        // Навігаційна властивість (одна здача роботи може мати багато файлів)
        public virtual UserTask UserTask { get; set; }
    }
}
