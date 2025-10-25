using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Threading.Tasks;
using Xunit;


namespace EduCloud_v42.Tests.Integration
{
    public class IntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public IntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/Home/Index")]
        [InlineData("/Home/Privacy")]
        [InlineData("/Courses/Index")] 
        [InlineData("/Courses/Create")]
        public async Task Get_Endpoints_ReturnSuccessStatusCode(string url)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync(url);

            // Assert
            Assert.NotNull(response); 
            response.EnsureSuccessStatusCode();
            Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());

        }

        [Fact]
        public async Task Get_NonExistentPage_ReturnsNotFound()
        {
             // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/ThisPageDoesNotExist");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
