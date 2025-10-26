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
    public class CourseElementsControllerTests
    {
        private LearningDbContext _context;
        private CourseElementsController _controller;
        private DbContextOptions<LearningDbContext> _options;

        private void SetupDbContext()
        {
            _options = new DbContextOptionsBuilder<LearningDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            _context = new LearningDbContext(_options, null);
        }

        private void SeedData()
        {
            var course = new Course { ID = 1, Name = "Test Course", Description = "Course for testing elements" };
            _context.Courses.Add(course);

            var elements = new List<CourseElement>
            {
                new CourseElement { ID = 1, Name = "Lecture 1", Description = "Integration test lecture", Type = CourseElementType.Lecture, CourseId = 1 },
                new CourseElement { ID = 2, Name = "Task 1", Description = "Integration test task", Type = CourseElementType.Task, CourseId = 1 }
            };
            _context.CourseElements.AddRange(elements);
            _context.SaveChanges();
        }

        [Fact]
        public async Task Index_ReturnsNotFound_WhenCourseNonExistent()
        {
            // Arrange
            SetupDbContext();
            _controller = new CourseElementsController(_context);

            // Act
            var result = await _controller.Index(courseId: 999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithElementsForCourse()
        {
            // Arrange
            SetupDbContext();
            SeedData();
            _controller = new CourseElementsController(_context);
            int courseId = 1;

            // Act
            var result = await _controller.Index(courseId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<CourseElement>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count());
            Assert.Equal(courseId, (int)viewResult.ViewData["CourseId"]);
        }

        [Fact]
        public async Task Create_Post_AddsElementAndRedirects_WhenModelStateIsValid()
        {
            // Arrange
            SetupDbContext();
            SeedData();
            _controller = new CourseElementsController(_context);
            var newElement = new CourseElement
            {
                ID = 3,
                Name = "New Lecture",
                Description = "Description for new lecture",
                Type = CourseElementType.Lecture,
                CourseId = 1
            };
            // Act
            var result = await _controller.Create(newElement);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal(1, redirectToActionResult.RouteValues["courseId"]);
            Assert.Equal(3, _context.CourseElements.Count());
        }

        [Fact]
        public async Task DeleteConfirmed_Post_DeletesElementAndRedirects()
        {
            // Arrange
            SetupDbContext();
            SeedData();
            _controller = new CourseElementsController(_context);
            int testId = 1;
            Assert.Equal(2, _context.CourseElements.Count());

            // Act
            var result = await _controller.DeleteConfirmed(testId);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal(1, _context.CourseElements.Count());
            Assert.Null(await _context.CourseElements.FindAsync(testId));
        }
    }
}