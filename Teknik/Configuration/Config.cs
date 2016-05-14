using System;
using System.IO;
using System.Threading;
using System.Web;
using Newtonsoft.Json;

namespace Teknik.Configuration
{
    public class Config
    {
        private ReaderWriterLockSlim _ConfigRWLock;
        private ReaderWriterLockSlim _ConfigFileRWLock;
        private JsonSerializerSettings _JsonSettings;

        private bool                _DevEnvironment;
        private bool                _Migrate;
        private string              _Title;
        private string              _Description;
        private string              _Author;
        private string              _Host;
        private string              _SupportEmail;
        private string              _BitcoinAddress;
        private string              _Salt1;
        private string              _Salt2;
        private UserConfig          _UserConfig;
        private ContactConfig       _ContactConfig;
        private EmailConfig         _EmailConfig;
        private GitConfig           _GitConfig;
        private UploadConfig        _UploadConfig;
        private PasteConfig         _PasteConfig;
        private BlogConfig          _BlogConfig;
        private ApiConfig           _ApiConfig;
        private PodcastConfig       _PodcastConfig;
        private StreamConfig        _StreamConfig;
        private ShortenerConfig     _ShortenerConfig;
        private TransparencyConfig  _TransparencyConfig;
        private DatabaseConfig      _DatabaseConfig;
        private PiwikConfig         _PiwikConfig;

        public bool         DevEnvironment  { get { return _DevEnvironment; }   set { _DevEnvironment = value; } }
        public bool         Migrate         { get { return _Migrate; }          set { _Migrate = value; } }

        // Site Information
        public string       Title           { get { return _Title; }            set { _Title = value; } }
        public string       Description     { get { return _Description; }      set { _Description = value; } }
        public string       Author          { get { return _Author; }           set { _Author = value; } }
        public string       Host            { get { return _Host; }             set { _Host = value; } }
        public string       SupportEmail    { get { return _SupportEmail; }     set { _SupportEmail = value; } }
        public string       BitcoinAddress  { get { return _BitcoinAddress; }   set { _BitcoinAddress = value; } }
        public string       Salt1           { get { return _Salt1; }            set { _Salt1 = value; } }
        public string       Salt2           { get { return _Salt2; }            set { _Salt2 = value; } }

        // User Configuration
        public UserConfig           UserConfig          { get { return _UserConfig; }           set { _UserConfig = value; } }

        // Contact Configuration
        public ContactConfig        ContactConfig       { get { return _ContactConfig; }        set { _ContactConfig = value; } }

        // Mail Server Configuration
        public EmailConfig          EmailConfig         { get { return _EmailConfig; }          set { _EmailConfig = value; } }

        // Git Service Configuration
        public GitConfig            GitConfig           { get { return _GitConfig; }            set { _GitConfig = value; } }

        // Blog Configuration
        public BlogConfig           BlogConfig          { get { return _BlogConfig; }           set { _BlogConfig = value; } }

        // Upload Configuration
        public UploadConfig         UploadConfig        { get { return _UploadConfig; }         set { _UploadConfig = value; } }

        // Paste Configuration
        public PasteConfig          PasteConfig         { get { return _PasteConfig; }          set { _PasteConfig = value; } }

        // API Configuration
        public ApiConfig            ApiConfig           { get { return _ApiConfig; }            set { _ApiConfig = value; } }

        // Podcast Configuration
        public PodcastConfig        PodcastConfig       { get { return _PodcastConfig; }        set { _PodcastConfig = value; } }

        // Stream Configuration
        public StreamConfig         StreamConfig        { get { return _StreamConfig; }         set { _StreamConfig = value; } }

        // Shortener Configuration
        public ShortenerConfig      ShortenerConfig     { get { return _ShortenerConfig; }      set { _ShortenerConfig = value; } }

        // Shortener Configuration
        public TransparencyConfig   TransparencyConfig  { get { return _TransparencyConfig; }   set { _TransparencyConfig = value; } }

        // Database Configuration
        public DatabaseConfig       DatabaseConfig      { get { return _DatabaseConfig; }       set { _DatabaseConfig = value; } }

        // Piwik Configuration
        public PiwikConfig          PiwikConfig         { get { return _PiwikConfig; }          set { _PiwikConfig = value; } }

        public Config()
        {
            _ConfigRWLock               = new ReaderWriterLockSlim();
            _ConfigFileRWLock           = new ReaderWriterLockSlim();
            _JsonSettings               = new JsonSerializerSettings();
            _JsonSettings.Formatting    = Formatting.Indented;

            SetDefaults();
        }

        public void SetDefaults()
        {
            DevEnvironment      = false;
            Migrate             = false;
            Title               = string.Empty;
            Description         = string.Empty;
            Author              = string.Empty;
            Host                = string.Empty;
            SupportEmail        = string.Empty;
            BitcoinAddress      = string.Empty;
            Salt1               = string.Empty;
            Salt2               = string.Empty;
            UserConfig          = new UserConfig();
            EmailConfig         = new EmailConfig();
            ContactConfig       = new ContactConfig();
            GitConfig           = new GitConfig();
            BlogConfig          = new BlogConfig();
            UploadConfig        = new UploadConfig();
            PasteConfig         = new PasteConfig();
            ApiConfig           = new ApiConfig();
            PodcastConfig       = new PodcastConfig();
            StreamConfig        = new StreamConfig();
            ShortenerConfig     = new ShortenerConfig();
            TransparencyConfig  = new TransparencyConfig();
            DatabaseConfig      = new DatabaseConfig();
            PiwikConfig         = new PiwikConfig();
        }

        public static Config Deserialize(string text)
        {
            return JsonConvert.DeserializeObject<Config>(text);
        }

        public static string Serialize(Config config)
        {
            return JsonConvert.SerializeObject(config, Formatting.Indented);
        }

        public static Config Load()
        {
            string path = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            return Load(path);
        }

        public static Config Load(string path)
        {
            Config config = new Config();
            if (!File.Exists(Path.Combine(path, "Config.json")))
            {
                Save(Path.Combine(path, "Config.json"), config);
            }
            else
            {
                string configContents = File.ReadAllText(Path.Combine(path, "Config.json"));
                config = Deserialize(configContents);
            }
            return config;
        }

        public static void Save(Config config)
        {
            string path = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            Save(Path.Combine(path, "Config.json"), config);
        }

        public static void Save(string path, Config config)
        {
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            string configContents = Config.Serialize(config);
            File.WriteAllText(path, configContents);
        }
    }
}