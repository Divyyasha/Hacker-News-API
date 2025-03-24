namespace TopStoriesAPI.Models
{
    public class Story
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
    }

    public class StoryListResponse   {
        public List<Story> Stories { get; set; }
        public int TotalCount { get; set; }
    }
}
