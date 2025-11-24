using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using EduCloud_v42.Models; // Додайте посилання на моделі

namespace EduCloud_v42.Tests.Integration
{
    public class ApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ApiIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
            factory.InitializeDbForTests();
        }

        [Fact]
        public async Task Get_CoursesV1_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/courses");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            // Перевіряємо, що це "стара" структура (без ElementsCount явно, або просто масив курсів)
            Assert.Contains("Test Course 1", content);
        }

        [Fact]
        public async Task Get_CoursesV2_ReturnsSuccess_AndStructure()
        {
            // Act
            var response = await _client.GetAsync("/api/v2/courses");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("Test Course 1", content);
            // Перевіряємо наявність нового поля, специфічного для V2
            Assert.Contains("elementsCount", content, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("2.0", content);
        }

        [Fact]
        public async Task Get_UsersApi_ReturnsJson()
        {
            var response = await _client.GetAsync("/api/usersapi");
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }
    }
}