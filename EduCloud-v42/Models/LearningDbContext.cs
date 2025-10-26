using Microsoft.EntityFrameworkCore;

namespace EduCloud_v42.Models
{
    public class LearningDbContext : DbContext
    {
        // Конструктор для ін'єкції залежностей
        public LearningDbContext(DbContextOptions<LearningDbContext> options) : base(options) { }

        // Набори DbSet для кожної таблиці
        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<UserCourse> UserCourses { get; set; }
        public DbSet<CourseElement> CourseElements { get; set; }
        public DbSet<CourseFile> CourseFiles { get; set; }
        public DbSet<UserTask> UserTasks { get; set; }
        public DbSet<TaskFile> TaskFiles { get; set; }

        // Метод для конфігурування моделей (Fluent API)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Налаштування таблиці Users ---
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.ID);

                // Налаштування Username
                entity.Property(e => e.Username)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.HasIndex(e => e.Username) 
                      .IsUnique();

                // Налаштування FullName
                entity.Property(e => e.FullName)
                      .IsRequired()
                      .HasMaxLength(500);

                entity.Property(e => e.Email)
                      .IsRequired();
                entity.HasIndex(e => e.Email)
                      .IsUnique();

                entity.Property(e => e.Phone).IsRequired(false);

                // Налаштування PasswordHash
                entity.Property(e => e.PasswordHash)
                      .IsRequired();

                // Зберігаємо Enum як рядок ("Admin", "User")
                entity.Property(e => e.Role).HasConversion<string>();
            });

            // --- Налаштування таблиці Course ---
            modelBuilder.Entity<Course>(entity =>
            {
                entity.ToTable("Course");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Description).IsRequired();
            });

            // --- Налаштування "Users to Courses" (зв'язок M-M з дод. полем) ---
            modelBuilder.Entity<UserCourse>(entity =>
            {
                entity.ToTable("Users to Courses");
                // Композитний первинний ключ
                entity.HasKey(uc => new { uc.UserId, uc.CourseId });

                // Зв'язок з User
                entity.HasOne(uc => uc.User)
                      .WithMany(u => u.UserCourses)
                      .HasForeignKey(uc => uc.UserId);

                // Зв'язок з Course
                entity.HasOne(uc => uc.Course)
                      .WithMany(c => c.UserCourses)
                      .HasForeignKey(uc => uc.CourseId);

                // Зберігаємо Enum як рядок
                entity.Property(uc => uc.Role).HasConversion<string>();
            });

            // --- Налаштування "Course Element" ---
            modelBuilder.Entity<CourseElement>(entity =>
            {
                entity.ToTable("Course Element");
                entity.HasKey(e => e.ID);

                // Мапуємо властивість CourseId на стовпець 'course'
                entity.Property(e => e.CourseId).HasColumnName("course");

                // Зв'язок з Course (Один Course має багато CourseElements)
                entity.HasOne(ce => ce.Course)
                      .WithMany(c => c.CourseElements)
                      .HasForeignKey(ce => ce.CourseId)// Використовуємо ім'я стовпця 'course' з діаграми
                      .HasPrincipalKey(c => c.ID);

                // Зберігаємо Enum як рядок
                entity.Property(e => e.Type)
                      .HasColumnName("type")
                      .HasConversion<string>();
            });

            // --- Налаштування "Course Files" ---
            modelBuilder.Entity<CourseFile>(entity =>
            {
                entity.ToTable("Course Files");
                entity.HasKey(e => e.ID); // PK
                entity.Property(e => e.Path).HasColumnName("path");

                // Зв'язок з CourseElement (Один CourseElement має багато файлів)
                entity.HasOne(cf => cf.CourseElement)
                      .WithMany(ce => ce.CourseFiles)
                      .HasForeignKey("Course element") // Використовуємо ім'я з діаграми
                      .HasPrincipalKey(ce => ce.ID);

                // Мапуємо властивість CourseElementId
                entity.Property(e => e.CourseElementId).HasColumnName("Course element");
            });

            // --- Налаштування "User to Task" (зв'язок M-M) ---
            modelBuilder.Entity<UserTask>(entity =>
            {
                entity.ToTable("User to Task");
                // Композитний первинний ключ
                entity.HasKey(ut => new { ut.UserId, ut.TaskId });

                // Мапування стовпців FK
                entity.Property(ut => ut.UserId).HasColumnName("User");
                entity.Property(ut => ut.TaskId).HasColumnName("Task");

                // Зв'язок з User
                entity.HasOne(ut => ut.User)
                      .WithMany(u => u.UserTasks)
                      .HasForeignKey(ut => ut.UserId);

                // Зв'язок з CourseElement (який є "Task")
                entity.HasOne(ut => ut.Task)
                      .WithMany(ce => ce.UserTasks)
                      .HasForeignKey(ut => ut.TaskId);
            });

            // --- Налаштування "Taks Files" ---
            modelBuilder.Entity<TaskFile>(entity =>
            {
                entity.ToTable("Taks Files"); // "Taks" як у діаграмі
                entity.HasKey(e => e.ID);

                // Мапування стовпців FK
                entity.Property(tf => tf.UserId).HasColumnName("User");
                entity.Property(tf => tf.TaskId).HasColumnName("Task");

                // Зв'язок з UserTask (Один UserTask має багато TaskFile)
                entity.HasOne(tf => tf.UserTask)
                      .WithMany(ut => ut.TaskFiles)
                      // Використовуємо композитний зовнішній ключ
                      .HasForeignKey(tf => new { tf.UserId, tf.TaskId });
            });
        }
    }
}
