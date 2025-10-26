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
    public class UsersControllerTests
    {
        private LearningDbContext _context;
        private UsersController _controller;
        private DbContextOptions<LearningDbContext> _options;

        private void SetupDbContext()
        {
            _options = new DbContextOptionsBuilder<LearningDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            _context = new LearningDbContext(_options);
        }

        private void SeedData()
        {
            var users = new List<User>
            {
                new User { ID = 1, Username = "Admin User", Role = UserRole.Admin },
                new User { ID = 2, Username = "Test User", Role = UserRole.User }
            };
            _context.Users.AddRange(users);
            _context.SaveChanges();
        }

        [Fact]
        public async Task Index_ReturnsAViewResult_WithAListOfUsers()
        {
            // Arrange
            SetupDbContext();
            SeedData();
            _controller = new UsersController(_context);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<User>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenIdIsNull()
        {
            // Arrange
            SetupDbContext();
            _controller = new UsersController(_context);

            // Act
            var result = await _controller.Details(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ReturnsViewResult_WithUser()
        {
            // Arrange
            SetupDbContext();
            SeedData();
            _controller = new UsersController(_context);
            int testId = 1;

            // Act
            var result = await _controller.Details(testId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<User>(viewResult.ViewData.Model);
            Assert.Equal(testId, model.ID);
            Assert.Equal("Admin User", model.Username);
        }

        [Fact]
        public async Task Create_Post_AddsUserAndRedirects_WhenModelStateIsValid()
        {
            // Arrange
            SetupDbContext();
            _controller = new UsersController(_context);
            var newUser = new User { ID = 3, Username = "New User", Role = UserRole.User };

            // Act
            var result = await _controller.Create(newUser);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal(1, _context.Users.Count(u => u.Username == "New User"));
        }

        [Fact]
        public async Task Edit_Post_UpdatesUserAndRedirects_WhenModelStateIsValid()
        {
            // Arrange
            SetupDbContext();
            SeedData();
            _controller = new UsersController(_context);
            var userToUpdate = await _context.Users.FindAsync(2);
            userToUpdate.Username = "Updated User Name";

            // Act
            var result = await _controller.Edit(2, userToUpdate);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);

            var updatedUser = await _context.Users.FindAsync(2);
            Assert.Equal("Updated User Name", updatedUser.Username);
        }

        [Fact]
        public async Task DeleteConfirmed_Post_DeletesUserAndRedirects()
        {
            // Arrange
            SetupDbContext();
            SeedData();
            _controller = new UsersController(_context);
            int testId = 1;
            Assert.Equal(2, _context.Users.Count());

            // Act
            var result = await _controller.DeleteConfirmed(testId);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal(1, _context.Users.Count());
            Assert.Null(await _context.Users.FindAsync(testId));
        }
    }
}