using EduCloud_v42;
using EduCloud_v42.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;


namespace EduCloud_v42.Tests.Integration
{
    public class IntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public IntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
            _factory.InitializeDbForTests();
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/Home/Index")]
        [InlineData("/Courses")]
        [InlineData("/Courses/Create")]
        [InlineData("/Users")]
        [InlineData("/Users/Create")]
        [InlineData("/CourseElements?courseId=1")]
        [InlineData("/Enrollment/UsersInCourse?courseId=1")]
        [InlineData("/Submissions/ViewSubmissions?taskId=2")]
        public async Task Get_Endpoints_ReturnSuccessStatusCode(string url)
        {
            // Arrange
            //var client = _factory.CreateClient();

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            Assert.NotNull(response); 
            response.EnsureSuccessStatusCode();
            Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());

        }

        [Fact]
        public async Task Get_NonExistentPage_ReturnsNotFound()
        {
             // Arrange
            //var client = _factory.CreateClient();

            // Act
            var response = await _client.GetAsync("/ThisPageDoesNotExist");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Get_CourseDetails_ReturnsSuccess_ForValidId()
        {
            // Act
            var response = await _client.GetAsync("/Courses/Details/1"); // Використовуємо ID з SeedData

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Test Course 1", content); // Перевіряємо вміст сторінки
        }

        [Fact]
        public async Task Get_CourseDetails_ReturnsNotFound_ForInvalidId()
        {
            // Act
            var response = await _client.GetAsync("/Courses/Details/999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        // --- Тести для UsersController ---

        [Fact]
        public async Task Get_UserDetails_ReturnsSuccess_ForValidId()
        {
            // Act
            var response = await _client.GetAsync("/Users/Details/1");
            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("AdminUser", content);
            Assert.Contains("Admin FullName", content);
        }

        [Fact]
        public async Task Post_CreateUser_RedirectsToIndex_WhenViewModelIsValid()
        {
            // Arrange
            var initialResponse = await _client.GetAsync("/Users/Create");
            var antiForgeryToken = await ExtractAntiForgeryToken(initialResponse);
            Assert.NotNull(antiForgeryToken);

            var formData = new Dictionary<string, string>
            {
                { "__RequestVerificationToken", antiForgeryToken },
                { "Username", "IntegUser" },
                { "FullName", "Integration User Full Name" },
                { "Email", "integ@example.com" },
                { "Phone", "555-1234" },
                { "Password", "StrongPassword123!" },
                { "ConfirmPassword", "StrongPassword123!" },
                { "Role", UserRole.User.ToString() }
            };
            var formContent = new FormUrlEncodedContent(formData);

            // Act
            var postResponse = await _client.PostAsync("/Users/Create", formContent);

            // Assert
            if (postResponse.StatusCode != HttpStatusCode.Redirect)
            {
                var responseBody = await postResponse.Content.ReadAsStringAsync();
                throw new Xunit.Sdk.XunitException($"Expected Redirect but got {postResponse.StatusCode}. Response body:\n{responseBody}");
            }

            Assert.Equal(HttpStatusCode.Redirect, postResponse.StatusCode);
            Assert.NotNull(postResponse.Headers.Location);
            Assert.Equal("/Users", postResponse.Headers.Location.OriginalString);

            var indexResponse = await _client.GetAsync("/Users");
            var indexContent = await indexResponse.Content.ReadAsStringAsync();
            Assert.Contains("IntegUser", indexContent);
            Assert.Contains("Integration User Full Name", indexContent);
        }

        // --- Тести для EnrollmentController ---

        [Fact]
        public async Task Get_CoursesForUser_ReturnsSuccess_ForValidId()
        {
            // Act
            var response = await _client.GetAsync("/Enrollment/CoursesForUser?userId=2");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Test Course 1", content);
        }

        // --- Тести для SubmissionsController ---

        [Fact]
        public async Task Get_GradeSubmissionPage_ReturnsSuccess_ForValidIds()
        {
            // Act
            var response = await _client.GetAsync("/Submissions/Grade?taskId=2&userId=2");
            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Grade Submission", content);
            Assert.Contains("Task 1", content);
            Assert.Contains("TestUser", content);
        }

        private async Task<string?> ExtractAntiForgeryToken(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var match = Regex.Match(content, @"<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)"" />");
            return match.Success ? match.Groups[1].Value : null;
        }
    }
}
