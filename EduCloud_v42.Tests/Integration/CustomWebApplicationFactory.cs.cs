using EduCloud_v42.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace EduCloud_v42.Tests.Integration
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private SqliteConnection? _connection;
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLowerInvariant();
            }
        }
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

            });
        }

        public void InitializeDbForTests()
        {
            using (var scope = this.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<LearningDbContext>();

                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();


                // 1. Користувачі з усіма полями
                var users = new List<User>
                {
                    new User {
                        ID = 1, Username = "AdminUser", FullName = "Admin FullName",
                        Email = "admin@example.com", PasswordHash = HashPassword("adminpass"),
                        Role = UserRole.Admin, Phone = "111222333"
                    },
                    new User {
                        ID = 2, Username = "TestUser", FullName = "Test FullName",
                        Email = "test@example.com", PasswordHash = HashPassword("testpass"),
                        Role = UserRole.User, Phone = "444555666"
                    }
                };
                db.Users.AddRange(users);
                db.SaveChanges();

                // 2. Курси
                var courses = new List<Course>
                {
                    new Course { ID = 1, Name = "Test Course 1", Description = "Desc 1" },
                    new Course { ID = 2, Name = "Test Course 2", Description = "Desc 2" }
                };
                db.Courses.AddRange(courses);
                db.SaveChanges();

                // 3. Елементи курсу (ID=2 є 'Task')
                var elements = new List<CourseElement>
                {
                    new CourseElement { ID = 1, Name = "Lecture 1", Description = "Integration test lecture", Type = CourseElementType.Lecture, CourseId = 1 },
                    new CourseElement { ID = 2, Name = "Task 1", Description = "Integration test task", Type = CourseElementType.Task, CourseId = 1 }
                };
                db.CourseElements.AddRange(elements);
                db.SaveChanges();

                // 4. Запис на курс
                var enrollment = new UserCourse { UserId = 2, CourseId = 1, Role = CourseRole.Student };
                db.UserCourses.Add(enrollment);

                // 5. Здача завдання (User 2 здав Task 1 (ID=2))
                var submission = new UserTask { UserId = 2, TaskId = 2, Mark = "Submitted" };
                db.UserTasks.Add(submission);

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
