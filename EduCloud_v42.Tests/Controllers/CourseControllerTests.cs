using EduCloud_v42.Controllers;
using EduCloud_v42.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EduCloud_v42.Tests.Controllers
{
    public class CourseControllerTests
    {
        private LearningDbContext _context;
        private CoursesController _controller;
        private DbContextOptions<LearningDbContext> _options;

        // Helper to setup in-memory database
        private void SetupDbContext()
        {
            _options = new DbContextOptionsBuilder<LearningDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString()) // Унікальна база для кожного тесту
                .Options;
            _context = new LearningDbContext(_options);
        }

        private void SeedData()
        {
            var courses = new List<Course>
            {
                new Course { ID = 1, Name = "Test Course 1", Description = "Desc 1" },
                new Course { ID = 2, Name = "Test Course 2", Description = "Desc 2" }
            };
            _context.Courses.AddRange(courses);
            _context.SaveChanges();
        }

        [Fact]
        public async Task Index_ReturnsAViewResult_WithAListOfCourses()
        {
            // Arrange
            SetupDbContext();
            SeedData();
            _controller = new CoursesController(_context);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Course>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenIdIsNull()
        {
            // Arrange
            SetupDbContext();
            _controller = new CoursesController(_context);

            // Act
            var result = await _controller.Details(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenCourseNonExistent()
        {
            // Arrange
            SetupDbContext();
            _controller = new CoursesController(_context);

            // Act
            var result = await _controller.Details(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ReturnsViewResult_WithCourse()
        {
            // Arrange
            SetupDbContext();
            SeedData();
            _controller = new CoursesController(_context);
            int testId = 1;

            // Act
            var result = await _controller.Details(testId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Course>(viewResult.ViewData.Model);
            Assert.Equal(testId, model.ID);
        }

        [Fact]
        public async Task Create_Post_AddsCourseAndRedirects_WhenModelStateIsValid()
        {
            // Arrange
            SetupDbContext();
            _controller = new CoursesController(_context);
            var newCourse = new Course { ID = 3, Name = "New Course", Description = "New Desc" };

            // Act
            var result = await _controller.Create(newCourse);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal(1, _context.Courses.Count(c => c.Name == "New Course"));
        }

        [Fact]
        public async Task Create_Post_ReturnsView_WhenModelStateIsInvalid()
        {
            // Arrange
            SetupDbContext();
            _controller = new CoursesController(_context);
            var newCourse = new Course { Name = "Invalid Course" }; // ID and Description might be required implicitly
            _controller.ModelState.AddModelError("Name", "Required");

            // Act
            var result = await _controller.Create(newCourse);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(newCourse, viewResult.ViewData.Model);
        }

        // --- НОВІ ТЕСТИ ДЛЯ EDIT ---

        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WhenIdIsNull()
        {
            // Arrange
            SetupDbContext();
            _controller = new CoursesController(_context);

            // Act
            var result = await _controller.Edit(id: null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_ReturnsViewResult_WithCourse()
        {
            // Arrange
            SetupDbContext();
            SeedData();
            _controller = new CoursesController(_context);
            int testId = 1;

            // Act
            var result = await _controller.Edit(testId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Course>(viewResult.ViewData.Model);
            Assert.Equal(testId, model.ID);
        }

        [Fact]
        public async Task Edit_Post_UpdatesCourseAndRedirects_WhenModelStateIsValid()
        {
            // Arrange
            SetupDbContext();
            SeedData();
            _controller = new CoursesController(_context);
            var courseToUpdate = await _context.Courses.FindAsync(1);
            courseToUpdate.Name = "Updated Name";

            // Act
            var result = await _controller.Edit(1, courseToUpdate);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);

            var updatedCourse = await _context.Courses.FindAsync(1);
            Assert.Equal("Updated Name", updatedCourse.Name);
        }

        [Fact]
        public async Task Edit_Post_ReturnsNotFound_WhenIdMismatch()
        {
            // Arrange
            SetupDbContext();
            _controller = new CoursesController(_context);
            var course = new Course { ID = 1, Name = "Test" };

            // Act
            var result = await _controller.Edit(99, course);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Get_ReturnsNotFound_WhenIdIsNull()
        {
            // Arrange
            SetupDbContext();
            _controller = new CoursesController(_context);

            // Act
            var result = await _controller.Delete(id: null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Get_ReturnsViewResult_WithCourse()
        {
            // Arrange
            SetupDbContext();
            SeedData();
            _controller = new CoursesController(_context);
            int testId = 1;

            // Act
            var result = await _controller.Delete(testId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Course>(viewResult.ViewData.Model);
            Assert.Equal(testId, model.ID);
        }

        [Fact]
        public async Task DeleteConfirmed_Post_DeletesCourseAndRedirects()
        {
            // Arrange
            SetupDbContext();
            SeedData();
            _controller = new CoursesController(_context);
            int testId = 1;
            Assert.Equal(2, _context.Courses.Count());

            // Act
            var result = await _controller.DeleteConfirmed(testId);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal(1, _context.Courses.Count());
            Assert.Null(await _context.Courses.FindAsync(testId));
        }
    }
}