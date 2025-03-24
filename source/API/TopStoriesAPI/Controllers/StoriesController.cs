using Microsoft.AspNetCore.Mvc;
using TopStoriesAPI.Business;

namespace TopStoriesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StoriesController : Controller
    {
        private readonly IStoryService _storyService;
        public StoriesController(IStoryService storyService)
        {
            _storyService = storyService;
        }

        /// <summary>
        /// Get stories from Hacker News with pagination and title search.
        /// </summary>
        /// <param name="page">Page number for pagination.</param>
        /// <param name="pageSize">Number of records a page will display.</param>
        /// <param name="searchTitle">Search term for filtering stories by title.</param>
        /// <returns>A list of stories with total count.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetStories([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string searchTitle = null)
        {
            try
            {
                if(page <=0 || pageSize <= 0)
                {
                    return BadRequest("An error occurred: Invalid page or pageSize");
                }
                var stories = await _storyService.GetStoriesAsync(page, pageSize, searchTitle);
                return Ok(stories);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }
    }
}
