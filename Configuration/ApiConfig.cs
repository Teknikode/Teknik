namespace Teknik.Configuration
{
    public class ApiConfig
    {
        public bool Enabled { get; set; }

        public int Version { get; set; }

        public ApiConfig()
        {
            Enabled = true;
            Version = 1;
        }
    }
}
