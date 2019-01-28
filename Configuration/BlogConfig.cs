namespace Teknik.Configuration
{
    public class BlogConfig
    {
        public bool Enabled { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int PostsToLoad { get; set; }
        public int CommentsToLoad { get; set; }
        public int ServerBlogId { get; set; }

        public BlogConfig()
        {
            SetDefaults();
        }

        public void SetDefaults()
        {
            Enabled = true;
            Title = string.Empty;
            Description = string.Empty;
            PostsToLoad = 10;
            CommentsToLoad = 10;
            ServerBlogId = 1;
        }
    }
}