namespace Teknik.Configuration
{
    public class ShortenerConfig
    {
        public bool Enabled { get; set; }
        public string ShortenerHost { get; set; }

        public int UrlLength { get; set; }

        public ShortenerConfig()
        {
            Enabled = true;
            ShortenerHost = string.Empty;
            UrlLength = 4;
        }
    }
}