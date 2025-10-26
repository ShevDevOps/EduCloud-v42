using EduCloud_v42.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;


namespace EduCloud_v42.Tests.Integration
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private SqliteConnection? _connection;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<LearningDbContext>));

                if (descriptor != null)
                    services.Remove(descriptor);

                _connection = new SqliteConnection("DataSource=:memory:");
                _connection.Open();

                services.AddDbContext<LearningDbContext>(options =>
                {
                    options.UseSqlite(_connection);
                });

                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<LearningDbContext>();
                    db.Database.EnsureCreated();
                }
            });
        }
        public void InitializeDbForTests()
        {
            // Створюємо 'scope' для отримання сервісів
            using (var scope = this.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<LearningDbContext>();

                // Перестворюємо базу даних кожного разу для чистого тесту
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                // --- Додаємо тестові дані ---

                // 1. Користувачі
                var users = new List<User>
                {
                    new User { ID = 1, Username = "Admin User", Role = UserRole.Admin },
                    new User { ID = 2, Username = "Test User", Role = UserRole.User }
                };
                db.Users.AddRange(users);

                // 2. Курси
                var courses = new List<Course>
                {
                    new Course { ID = 1, Name = "Test Course 1", Description = "Desc 1" },
                    new Course { ID = 2, Name = "Test Course 2", Description = "Desc 2" }
                };
                db.Courses.AddRange(courses);

                // 3. Елементи курсу (ID=2 є 'Task')
                var elements = new List<CourseElement>
                {
                    new CourseElement { ID = 1, Name = "Lecture 1", Description = "Integration test lecture", Type = CourseElementType.Lecture, CourseId = 1 },
                    new CourseElement { ID = 2, Name = "Task 1", Description = "Integration test task", Type = CourseElementType.Task, CourseId = 1 }
                };
                db.CourseElements.AddRange(elements);

                // 4. Запис на курс
                var enrollment = new UserCourse { UserId = 2, CourseId = 1, Role = CourseRole.Student };
                db.UserCourses.Add(enrollment);

                // 5. Здача завдання (User 2 здав Task 1 (ID=2))
                var submission = new UserTask { UserId = 2, TaskId = 2, Mark = "Submitted" };
                db.UserTasks.Add(submission);

                // Зберігаємо всі зміни
                db.SaveChanges();
            }
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _connection?.Close();
                _connection?.Dispose();
            }
        }
    }
}
