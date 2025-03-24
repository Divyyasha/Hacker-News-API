using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using TopStoriesAPI.Business;
using TopStoriesAPI.Configuration;
using TopStoriesAPI.Models;

namespace TopStoriesTests
{
    public class StoryServiceTests
    {
        private readonly Mock<IMemoryCache> _mockMemoryCache;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<IOptions<APIConfigurations>> _mockApiConfigurations;
        private readonly Mock<ILogger<StoryService>> _mockLogger;
        private StoryService _storyService;
        private APIConfigurations apiConfig = new APIConfigurations { ApiUrl = "https://api.example.com", TopStoriesEndpoint = "/topstories", ItemEndpoint = "/item/{0}", TotalStoriesCount = 100 };


        public StoryServiceTests()
        {
            _mockMemoryCache = new Mock<IMemoryCache>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILogger<StoryService>>();
            _mockApiConfigurations = new Mock<IOptions<APIConfigurations>>();
            _mockApiConfigurations.Setup(x => x.Value).Returns(apiConfig);
            _storyService = new StoryService(_mockMemoryCache.Object, _mockHttpClientFactory.Object, _mockApiConfigurations.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetStoriesAsync_Should_ReturnCachedStories_WhenAvailable()
        {
            //Arrange
            _mockMemoryCache.Setup(c => c.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny)).Callback((object key, out object value) =>
                    {
                        value = new StoryListResponse
                        {
                            TotalCount = 2,
                            Stories = new List<Story> { new Story { Id = 1, Title = "Test Story 1", Url = "http://teststory1.com" }, new Story { Id = 2, Title = "Test Story 2", Url = "http://teststory2.com" } }
                        };
                    }).Returns(true);

            //Act
            var result = await _storyService.GetStoriesAsync(1, 10, "test");

            //Assert
            Assert.Equal(2, result.Stories.Count);
            Assert.Equal("Test Story 1", result.Stories[0].Title);
        }

        [Fact]
        public async Task GetStoriesAsync_Should_ReturnStories_FromAPI_WhenNotInCache()
        {
            //Arrange
            var storyIdsResponse = new StringContent(@"[1,2,3]");
            var storyResponse1 = new StringContent(@"{""Id"": 1, ""Title"": ""Test Story 1"", ""Url"" : ""http://teststory1.com""}");
            var storyResponse2 = new StringContent(@"{""Id"": 2, ""Title"": ""Test Story 2"", ""Url"" : ""http://teststory2.com""}");
            var storyResponse3 = new StringContent(@"{""Id"": 3, ""Title"": ""Test Story 3"", ""Url"" : ""http://teststory3.com""}");
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(httpMessageHandlerMock.Object);

            httpMessageHandlerMock
              .Protected()
              .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
              .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = storyIdsResponse })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = storyResponse1 })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = storyResponse2 })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = storyResponse3 });

            _mockHttpClientFactory.Setup(factory => factory.CreateClient(string.Empty)).Returns(httpClient).Verifiable();
            _mockMemoryCache.Setup(c => c.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny)).Callback((object key, out object value) =>
            {
                value = new StoryListResponse
                {
                    TotalCount = 1,
                    Stories = new List<Story> { new Story { Id = 1, Title = "Test Story 1", Url = "http://teststory1.com" } }
                };
            }).Returns(true);


            var storyService = new StoryService(_mockMemoryCache.Object, _mockHttpClientFactory.Object, _mockApiConfigurations.Object, _mockLogger.Object);

            //Act
            var result = await storyService.GetStoriesAsync(1, 10, "Test");

            //Assert
            Assert.Single(result.Stories);
            Assert.Equal("Test Story 1", result.Stories[0].Title);
        }

        [Fact]
        public async Task GetStoriesAsync_ShouldReturnEmptyList_WhenNoStoriesMatchSearchTitle()
        {
            //Arrange
            var storyIdsResponse = new StringContent(@"[1,2,3]");
            var storyResponse1 = new StringContent(@"{""Id"": 1, ""Title"": ""Test Story 1"", ""Url"" : ""http://teststory1.com""}");
            var storyResponse2 = new StringContent(@"{""Id"": 2, ""Title"": ""Test Story 2"", ""Url"" : ""http://teststory2.com""}");
            var storyResponse3 = new StringContent(@"{""Id"": 3, ""Title"": ""Test Story 3"", ""Url"" : ""http://teststory3.com""}");

            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(httpMessageHandlerMock.Object);

            httpMessageHandlerMock
              .Protected()
              .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
              .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = storyIdsResponse })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = storyResponse1 })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = storyResponse2 })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = storyResponse3 });

            _mockHttpClientFactory.Setup(factory => factory.CreateClient(string.Empty)).Returns(httpClient).Verifiable();
            var storyService = new StoryService(_mockMemoryCache.Object, _mockHttpClientFactory.Object, _mockApiConfigurations.Object, _mockLogger.Object);

            //Act
            var result = await storyService.GetStoriesAsync(1, 10, "Testing");

            //Assert
            Assert.Empty(result.Stories);
        }

        [Fact]
        public async Task GetStoriesAsync_ShouldThrowApplicationException_WhenApiFails()
        {
            //Arrange
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(httpMessageHandlerMock.Object);

            httpMessageHandlerMock
              .Protected()
              .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
              .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent("API Error") });

            _mockHttpClientFactory.Setup(factory => factory.CreateClient(string.Empty)).Returns(httpClient).Verifiable();

            //Act
            var exception = await Assert.ThrowsAsync<ApplicationException>(async () => await _storyService.GetStoriesAsync(1, 10, null));

            //Assert
            Assert.Equal("Error fetching top stories.", exception.Message);
        }

        [Fact]
        public async Task GetStoriesAsync_ShouldReturnAllStories_WhenSearchTitleIsNull()
        {
            //Arrange
            var storyIdsResponse = new StringContent(@"[1,2,3]");
            var storyResponse1 = new StringContent(@"{""Id"": 1, ""Title"": ""Test Story 1"", ""Url"" : ""http://teststory1.com""}");
            var storyResponse2 = new StringContent(@"{""Id"": 2, ""Title"": ""Test Story 2"", ""Url"" : ""http://teststory2.com""}");
            var storyResponse3 = new StringContent(@"{""Id"": 3, ""Title"": ""Test Story 3"", ""Url"" : ""http://teststory3.com""}");

            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(httpMessageHandlerMock.Object);

            httpMessageHandlerMock
              .Protected()
              .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
              .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = storyIdsResponse })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = storyResponse1 })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = storyResponse2 })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = storyResponse3 });

            _mockHttpClientFactory.Setup(factory => factory.CreateClient(string.Empty)).Returns(httpClient).Verifiable();
            _mockMemoryCache.Setup(c => c.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny)).Callback((object key, out object value) =>
            {
                value = new StoryListResponse
                {
                    TotalCount = 3,
                    Stories = new List<Story> { new Story { Id = 1, Title = "Test Story 1", Url = "http://teststory1.com" }, new Story { Id = 2, Title = "Test Story 2", Url = "http://teststory1.com" }, new Story { Id = 3, Title = "Test Story 3", Url = "http://teststory3.com" } }
                };
            }).Returns(true);


            var storyService = new StoryService(_mockMemoryCache.Object, _mockHttpClientFactory.Object, _mockApiConfigurations.Object, _mockLogger.Object);

            //Act
            var result = await _storyService.GetStoriesAsync(1, 10, null);

            //Assert
            Assert.Equal(3,result.Stories.Count);
            Assert.Equal("Test Story 1", result.Stories[0].Title);
        }
    }
}
