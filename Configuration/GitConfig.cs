namespace Teknik.Configuration
{
    public class GitConfig
    {
        public bool Enabled { get; set; }

        public string Host { get; set; }

        public string AccessToken { get; set; }

        public string SecretKey { get; set; }

        public int SourceId { get; set; }

        public DatabaseConfig Database { get; set; }

        public GitConfig()
        {
            Enabled = true;
            Host = string.Empty;
            AccessToken = string.Empty;
            SecretKey = string.Empty;
            SourceId = 1;
            Database = new DatabaseConfig();
        }
    }
}
