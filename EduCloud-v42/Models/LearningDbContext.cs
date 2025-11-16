using Microsoft.EntityFrameworkCore;

namespace EduCloud_v42.Models
{
    public class LearningDbContext : DbContext
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        // Конструктор для ін'єкції залежностей, включаючи IWebHostEnvironment
        public LearningDbContext(DbContextOptions<LearningDbContext> options, IWebHostEnvironment webHostEnvironment)
            : base(options)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        // Набори DbSet для кожної таблиці
        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<UserCourse> UserCourses { get; set; }
        public DbSet<CourseElement> CourseElements { get; set; }
        public DbSet<CourseFile> CourseFiles { get; set; }
        public DbSet<UserTask> UserTasks { get; set; }
        public DbSet<TaskFile> TaskFiles { get; set; }

        #region CourseFile Management (Synchronous)

        public CourseFile AddCourseFile(IFormFile file, int courseElementId)
        {
            var relativePath = SaveFile(file, "course_files");
            if (string.IsNullOrEmpty(relativePath))
            {
                return null; // Помилка збереження файлу
            }

            var courseFile = new CourseFile
            {
                Path = relativePath,
                CourseElementId = courseElementId,
                Name = file.FileName
            };

            CourseFiles.Add(courseFile);
            SaveChanges();
            return courseFile;
        }

        public CourseFile UpdateCourseFile(int courseFileId, IFormFile newFile)
        {
            var courseFile = CourseFiles.Find(courseFileId);
            if (courseFile == null)
            {
                return null; // Запис не знайдено
            }

            // Видаляємо старий файл
            DeleteFile(courseFile.Path);

            // Зберігаємо новий файл і оновлюємо шлях
            var newRelativePath = SaveFile(newFile, "course_files");
            if (string.IsNullOrEmpty(newRelativePath))
            {
                return null; // Помилка збереження нового файлу
            }

            courseFile.Path = newRelativePath;
            courseFile.Name = newFile.FileName;
            CourseFiles.Update(courseFile);
            SaveChanges();

            return courseFile;
        }

        public bool DeleteCourseFile(int courseFileId)
        {
            var courseFile = CourseFiles.Find(courseFileId);
            if (courseFile == null)
            {
                return false;
            }

            // Видаляємо фізичний файл
            DeleteFile(courseFile.Path);

            // Видаляємо запис з БД
            CourseFiles.Remove(courseFile);
            SaveChanges();

            return true;
        }

        #endregion

        #region TaskFile Management (Synchronous)

        public TaskFile? AddTaskFile(IFormFile file, int userId, int taskId)
        {
            var relativePath = SaveFile(file, "task_files");
            if (string.IsNullOrEmpty(relativePath))
            {
                return null;
            }

            var taskFile = new TaskFile
            {
                Path = relativePath,
                UserId = userId,
                TaskId = taskId,
                Name = file.FileName
            };

            TaskFiles.Add(taskFile);
            SaveChanges();
            return taskFile;
        }

        public TaskFile? UpdateTaskFile(int taskFileId, IFormFile newFile)
        {
            var taskFile = TaskFiles.Find(taskFileId);
            if (taskFile == null)
            {
                return null;
            }

            DeleteFile(taskFile.Path);

            var newRelativePath = SaveFile(newFile, "task_files");
            if (string.IsNullOrEmpty(newRelativePath))
            {
                return null;
            }

            taskFile.Path = newRelativePath;
            taskFile.Name = newFile.FileName;
            TaskFiles.Update(taskFile);
            SaveChanges();

            return taskFile;
        }

        public bool DeleteTaskFile(int taskFileId)
        {
            var taskFile = TaskFiles.Find(taskFileId);
            if (taskFile == null)
            {
                return false;
            }

            DeleteFile(taskFile.Path);

            TaskFiles.Remove(taskFile);
            SaveChanges();

            return true;
        }

        public void DeleteAllTaskFiles(int taskId)
        {
            List<TaskFile> taskFiles = TaskFiles.Where(tf => tf.TaskId == taskId).ToList();
            foreach (TaskFile taskFile in taskFiles)
            {
                DeleteTaskFile(taskFile.ID);
            }
            List<CourseFile> courseFiles = CourseFiles.Where(cf => cf.CourseElementId == taskId).ToList();
            foreach (CourseFile courseFile in courseFiles)
            {
                DeleteCourseFile(courseFile.ID);
            }
        }

        public void DeleteAllCourseFiles(int courseId)
        {
            List<CourseElement> courseElements = CourseElements.Where(ce => ce.CourseId == courseId).Include(ce => ce.CourseFiles).ToList();
            foreach (CourseElement item in courseElements)
            {
                DeleteAllTaskFiles(item.ID);
            }

        }

        #endregion

        #region Private File Helpers

        private string? SaveFile(IFormFile file, string subfolder)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            // Шлях до папки 'wwwroot/uploads/subfolder'
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", subfolder);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Створення унікального імені файлу для уникнення конфліктів
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            // Повертаємо відносний шлях для збереження в БД
            return "/" + Path.Combine("uploads", subfolder, uniqueFileName).Replace('\\', '/');
        }

        private void DeleteFile(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return;
            }
            // Видаляємо початковий слеш, якщо він є, щоб Path.Combine працював коректно
            relativePath = relativePath.TrimStart('/');
            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        #endregion

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
                      .IsRequired()
                      .HasMaxLength(50);
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
                      .HasForeignKey(cf => cf.CourseElementId) // Використовуємо ім'я з діаграми
                      .HasPrincipalKey(ce => ce.ID);

                // Мапуємо властивість CourseElementId
                entity.Property(e => e.CourseElementId).HasColumnName("CourseElement");
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
