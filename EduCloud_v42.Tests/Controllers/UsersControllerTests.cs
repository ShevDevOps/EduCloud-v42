using EduCloud_v42.Controllers;
using EduCloud_v42.Models;
using EduCloud_v42.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
            _context = new LearningDbContext(_options, null);
        }

        private void SeedData()
        {
            var users = new List<User>
            {
                new User {
                    ID = 1, Username = "AdminUser", FullName = "Admin FullName",
                    Email = "admin@example.com", PasswordHash = "adminhash", Role = UserRole.Admin
                },
                new User {
                    ID = 2, Username = "TestUser", FullName = "Test FullName",
                    Email = "test@example.com", PasswordHash = "testhash", Role = UserRole.User
                }
            };
            _context.Users.AddRange(users);
            _context.SaveChanges();
        }
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLowerInvariant();
            }
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
            Assert.Equal("AdminUser", model.Username);
            Assert.Equal("Admin FullName", model.FullName);
        }

        [Fact]
        public async Task Create_Post_AddsUserAndRedirects_WhenModelStateIsValid()
        {
            // Arrange
            SetupDbContext();
            _controller = new UsersController(_context);
            var viewModel = new UserCreateViewModel
            {
                Username = "NewUser",
                FullName = "New User FullName",
                Email = "new@example.com",
                Phone = "1234567890",
                Password = "Password123",
                Role = UserRole.User
            };

            // Act
            var result = await _controller.Create(viewModel);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);

            var createdUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "NewUser");
            Assert.NotNull(createdUser);
            Assert.Equal("New User FullName", createdUser.FullName);
            Assert.Equal("new@example.com", createdUser.Email);
            Assert.Equal(UserRole.User, createdUser.Role);
            Assert.NotEmpty(createdUser.PasswordHash);
            Assert.Equal(HashPassword("Password123"), createdUser.PasswordHash);
        }
        [Fact]
        public async Task Create_Post_ReturnsView_WhenPasswordIsEmpty()
        {
            // Arrange
            SetupDbContext();
            Assert.NotNull(_context);
            _controller = new UsersController(_context);
            var viewModel = new UserCreateViewModel
            {
                Username = "NoPassUser",
                FullName = "No Pass",
                Email = "nopass@ex.com",
                Role = UserRole.User,
                Password = ""
            };
            _controller.ModelState.AddModelError("Password", "Password is required.");


            // Act
            var result = await _controller.Create(viewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Equal(viewModel, viewResult.Model);
            Assert.Equal(0, await _context.Users.CountAsync(u => u.Username == "NoPassUser"));
        }

        [Fact]
        public async Task Create_Post_ReturnsView_WhenModelStateIsInvalid()
        {
            // Arrange
            SetupDbContext();
            Assert.NotNull(_context);
            _controller = new UsersController(_context);
            var viewModel = new UserCreateViewModel { Password = "somepassword"};
            _controller.ModelState.AddModelError("Username", "Username is required");

            // Act
            var result = await _controller.Create(viewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Equal(viewModel, viewResult.Model);
            Assert.Equal(0, await _context.Users.CountAsync());
        }

        [Fact]
        public async Task Edit_Post_UpdatesUserAndRedirects_WhenModelStateIsValid()
        {
            // Arrange
            SetupDbContext();
            SeedData();
            Assert.NotNull(_context);
            _controller = new UsersController(_context);

            var userToUpdate = await _context.Users.FindAsync(2);
            Assert.NotNull(userToUpdate);

            var formData = new User
            {
                ID = 2,
                Username = "UpdatedUser",
                FullName = "Updated FullName",
                Email = "updated@example.com",
                Phone = "987654321",
                Role = UserRole.Admin,
                PasswordHash = userToUpdate.PasswordHash
            };
            _context.Entry(userToUpdate).State = EntityState.Detached;

            // Act
            var result = await _controller.Edit(2, formData);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);

            var updatedUser = await _context.Users.FindAsync(2);
            Assert.NotNull(updatedUser);
            Assert.Equal("UpdatedUser", updatedUser.Username);
            Assert.Equal("Updated FullName", updatedUser.FullName);
            Assert.Equal("updated@example.com", updatedUser.Email);
            Assert.Equal(UserRole.Admin, updatedUser.Role);
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