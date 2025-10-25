using EduCloud_v42.Controllers;
using EduCloud_v42.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EduCloud_v42.Tests.Controllers
{
    public class CoursesControllerTests
    {
        private LearningDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<LearningDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            var context = new LearningDbContext(options);

            context.Courses.Add(new Course { ID = 1, Name = "Test Course 1", Description = "Desc 1" });
            context.Courses.Add(new Course { ID = 2, Name = "Test Course 2", Description = "Desc 2" });
            context.SaveChanges();

            return context;
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithListOfCourses()
        {
            // Arrange
            await using var context = GetInMemoryDbContext();
            var controller = new CoursesController(context);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Course>>(viewResult.Model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenIdIsNull()
        {
            // Arrange
            await using var context = GetInMemoryDbContext();
            var controller = new CoursesController(context);

            // Act
            var result = await controller.Details(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenCourseDoesNotExist()
        {
            // Arrange
            await using var context = GetInMemoryDbContext();
            var controller = new CoursesController(context);

            // Act
            var result = await controller.Details(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ReturnsViewResult_WithCourse()
        {
            // Arrange
            await using var context = GetInMemoryDbContext();
            var controller = new CoursesController(context);
            var testCourseId = 1;

            // Act
            var result = await controller.Details(testCourseId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Course>(viewResult.Model);
            Assert.Equal(testCourseId, model.ID);
        }
    }
}
