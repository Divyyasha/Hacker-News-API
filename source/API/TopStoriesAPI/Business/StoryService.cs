using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TopStoriesAPI.Configuration;
using TopStoriesAPI.Models;

namespace TopStoriesAPI.Business
{
    public class StoryService : IStoryService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly APIConfigurations _apiConfigurations;
        private readonly ILogger<StoryService> _logger;
        private const string TopStoriesCacheKey = "stories_page_{0}_title_{1}";

        public StoryService(IMemoryCache cache, IHttpClientFactory httpClientFactory, IOptions<APIConfigurations> apiConfigurations, ILogger<StoryService> logger)
        {
            _memoryCache = cache;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _apiConfigurations = apiConfigurations.Value;
        }

        public async Task<StoryListResponse> GetStoriesAsync(int page = 1, int pageSize = 10, string searchTitle = null)
        {
            string cacheKey = string.Format(TopStoriesCacheKey, page, searchTitle);
            var response = new StoryListResponse();

            try
            {
               if (_memoryCache.TryGetValue(cacheKey, out StoryListResponse cachedResponse))
                {
                    return cachedResponse;
                }

                var client = _httpClientFactory.CreateClient();
                var storyIdsResponse = await client.GetStringAsync(string.Format(_apiConfigurations.ApiUrl + _apiConfigurations.TopStoriesEndpoint));
                var responseIds = JsonConvert.DeserializeObject<List<int>>(storyIdsResponse);
                var storyIds = responseIds.GetRange(0, Math.Min(_apiConfigurations.TotalStoriesCount, responseIds.Count));
                var stories = new List<Story>();

                for (int i = (page - 1) * pageSize; i < page * pageSize && i < storyIds.Count; i++)
                {
                    var storyId = storyIds[i];
                    var storyResponse = await client.GetStringAsync(string.Format(_apiConfigurations.ApiUrl + _apiConfigurations.ItemEndpoint, storyId));
                    var story = JsonConvert.DeserializeObject<Story>(storyResponse);

                    if (string.IsNullOrEmpty(searchTitle) || story.Title.Contains(searchTitle, StringComparison.OrdinalIgnoreCase))
                    {
                        stories.Add(story);
                    }
                }

                response = new StoryListResponse
                {
                    TotalCount = string.IsNullOrEmpty(searchTitle) && string.IsNullOrWhiteSpace(searchTitle) ? storyIds.Count : stories.Count,
                    Stories = stories
                };

                if (storyIds.Count > 0)
                {
                    _memoryCache.Set(cacheKey, response, TimeSpan.FromMinutes(10));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching top stories.");
                throw new ApplicationException("Error fetching top stories.", ex);
            }
            return response;
        }
    }
}
