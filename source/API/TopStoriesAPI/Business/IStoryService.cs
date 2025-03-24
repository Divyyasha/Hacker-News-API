using TopStoriesAPI.Models;

namespace TopStoriesAPI.Business
{
    public interface IStoryService
    {
        Task<StoryListResponse> GetStoriesAsync(int page = 1, int pageSize = 10, string searchTitle = null);
    }
}
