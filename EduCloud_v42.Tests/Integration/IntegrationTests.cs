using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using EduCloud_v42;
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
        [InlineData("/Home/Privacy")]
        [InlineData("/Courses")]
        [InlineData("/Courses/Create")]
        [InlineData("/Users")]
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

        [Fact]
        public async Task Post_CreateCourse_RedirectsToIndex_WhenValid()
        {
            // Arrange
            var initialResponse = await _client.GetAsync("/Courses/Create");
            var antiForgeryToken = await ExtractAntiForgeryToken(initialResponse);

            var formData = new Dictionary<string, string>
            {
                { "__RequestVerificationToken", antiForgeryToken ?? string.Empty },
                { "Name", "Integration Test Course" },
                { "Description", "Created via integration test" }
            };
            var formContent = new FormUrlEncodedContent(formData);

            // Act
            var postResponse = await _client.PostAsync("/Courses/Create", formContent);

            // Debug: Check what we actually got
            var responseContent = await postResponse.Content.ReadAsStringAsync();

            // Assert
            if (postResponse.StatusCode == HttpStatusCode.OK)
            {
                // Model validation failed - output the response for debugging
                throw new Exception($"Expected redirect but got OK. Response content:\n{responseContent}");
            }

            Assert.Equal(HttpStatusCode.Redirect, postResponse.StatusCode);
            Assert.Equal("/Courses", postResponse.Headers.Location?.OriginalString);

            // Перевіряємо, що курс дійсно з'явився у списку
            var indexResponse = await _client.GetAsync("/Courses");
            var indexContent = await indexResponse.Content.ReadAsStringAsync();
            Assert.Contains("Integration Test Course", indexContent);
        }

        // --- Тести для UsersController ---

        [Fact]
        public async Task Get_UserDetails_ReturnsSuccess_ForValidId()
        {
            // Act
            var response = await _client.GetAsync("/Users/Details/1"); // Використовуємо ID з SeedData

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Admin User", content);
        }

        // --- Тести для EnrollmentController ---

        [Fact]
        public async Task Get_CoursesForUser_ReturnsSuccess_ForValidId()
        {
            // Act
            var response = await _client.GetAsync("/Enrollment/CoursesForUser?userId=2"); // User ID з SeedData

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Test Course 1", content); // Курс, на який записаний User 2
        }

        // --- Тести для SubmissionsController ---

        [Fact]
        public async Task Get_GradeSubmissionPage_ReturnsSuccess_ForValidIds()
        {
            // Act
            var response = await _client.GetAsync("/Submissions/Grade?taskId=2&userId=2"); // Task ID та User ID з SeedData

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Grade Submission", content);
            Assert.Contains("Task 1", content); // Назва завдання
            Assert.Contains("Test User", content); // Ім'я користувача
        }


        // --- Допоміжна функція для витягування AntiForgeryToken ---
        private async Task<string?> ExtractAntiForgeryToken(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            var match = Regex.Match(content, @"<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)"" />");
            return match.Success ? match.Groups[1].Value : null;
        }
    }
}
