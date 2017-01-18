namespace Teknik.Configuration
{
    public class DatabaseConfig
    {
        public bool Migrate { get; set; }
        public string Server { get; set; }
        public int Port { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public DatabaseConfig()
        {
            SetDefaults();
        }

        public void SetDefaults()
        {
            Migrate = false;
            Server = "localhost";
            Port = 3306;
            Database = string.Empty;
            Username = string.Empty;
            Password = string.Empty;
        }
    }
}