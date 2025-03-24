using Microsoft.AspNetCore.Mvc;
using Moq;
using TopStoriesAPI.Business;
using TopStoriesAPI.Controllers;
using TopStoriesAPI.Models;

namespace TopStories.Tests
{
    public class StoryControllerTests
    {
        private readonly StoriesController _controller;
        private readonly Mock<IStoryService> _storyServiceMock;

        public StoryControllerTests()
        {
            _storyServiceMock = new Mock<IStoryService>();
            _controller = new StoriesController(_storyServiceMock.Object);
        }

        [Fact]
        public async Task GetStories_ShouldReturnOkResult_WhenStoriesAreFound()
        {
            // Arrange
            var mockResponse = new StoryListResponse { Stories = new List<Story> { new Story { Id = 123, Title = "Sample Story", Url = "http://www.samplestory.com" } }, TotalCount = 1 };
            _storyServiceMock.Setup(service => service.GetStoriesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(mockResponse);

            // Act
            var result = await _controller.GetStories(page: 1, pageSize: 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsAssignableFrom<StoryListResponse>(okResult.Value);
        }

        [Fact]
        public async Task GetStories_ShouldReturnOkResult_WhenNoStoriesAreFound()
        {
            // Arrange
            _storyServiceMock.Setup(service => service.GetStoriesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                            .ReturnsAsync(new StoryListResponse { Stories = [], TotalCount = 0 });

            // Act
            var result = await _controller.GetStories(page: 1, pageSize: 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<StoryListResponse>(okResult.Value);
            Assert.Empty(response.Stories);
        }

        [Fact]
        public async Task GetStories_ShouldReturnBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            _storyServiceMock.Setup(service => service.GetStoriesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                            .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetStories(page: 1, pageSize: 10);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("An error occurred: Test exception", badRequestResult.Value);
        }

        [Fact]
        public async Task GetStories_ShouldUseDefaultPaginationValues_WhenNoPageOrPageSizeIsProvided()
        {
            // Arrange
            var mockResponse = new StoryListResponse { Stories = new List<Story> { new Story { Id = 123, Title = "Sample Story", Url = "http://www.samplestory.com" } }, TotalCount = 1 };
            _storyServiceMock.Setup(service => service.GetStoriesAsync(1, 10, null)).ReturnsAsync(mockResponse);

            // Act
            var result = await _controller.GetStories();

            // Assert
            _storyServiceMock.Verify(service => service.GetStoriesAsync(1, 10, null), Times.Once);
        }

        [Fact]
        public async Task GetStories_ShouldReturnFilteredStories_WhenSearchTitleIsProvided()
        {
            // Arrange
            var searchTitle = "Sample";
            var mockResponse = new StoryListResponse { Stories = new List<Story> { new Story { Id = 123, Title = "Sample Story", Url = "http://www.samplestory.com" } }, TotalCount = 1 };

            _storyServiceMock.Setup(service => service.GetStoriesAsync(It.IsAny<int>(), It.IsAny<int>(), searchTitle))
                            .ReturnsAsync(mockResponse);

            // Act
            var result = await _controller.GetStories(page: 1, pageSize: 10, searchTitle: searchTitle);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var storiesResponse = Assert.IsAssignableFrom<StoryListResponse>(okResult.Value);
            Assert.Single(storiesResponse.Stories);
            Assert.Contains(storiesResponse.Stories, story => story.Title.Contains(searchTitle));
        }

        [Fact]
        public async Task GetStories_ShouldReturnBadRequest_WhenInvalidQueryParametersAreProvided()
        {
            // Act
            var result = await _controller.GetStories(page: -1, pageSize: -10);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("An error occurred: Invalid page or pageSize", badRequestResult.Value);
        }
    }
}
