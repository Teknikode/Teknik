namespace Teknik.Configuration
{
    public class PiwikConfig
    {
        public bool Enabled { get; set; }

        public string Url { get; set; }

        public string API { get; set; }

        public int SiteId { get; set; }

        public string TokenAuth { get; set; }

        public PiwikConfig()
        {
            Enabled = false;
            Url = string.Empty;
            API = string.Empty;
            SiteId = 1;
            TokenAuth = string.Empty;
        }
    }
}
