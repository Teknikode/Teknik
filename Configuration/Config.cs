using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using Teknik.Utilities.Cryptography;

namespace Teknik.Configuration
{
    public class Config
    {
        private const string _ConfigCacheKey = "ConfigCache";
        private const string _ConfigFileName = "Config.json";

        private static Config _Config { get; set; }
        private static string _FileHash { get; set; }

        private ReaderWriterLockSlim _ConfigRWLock;
        private ReaderWriterLockSlim _ConfigFileRWLock;
        private JsonSerializerSettings _JsonSettings;

        private bool                _DevEnvironment;
        private bool                _Migrate;
        private bool                _UseCdn;
        private string              _Title;
        private string              _Description;
        private string              _Author;
        private string              _Host;
        private string              _SupportEmail;
        private string              _NoReplyEmail;
        private string              _BitcoinAddress;
        private string              _Salt1;
        private string              _Salt2;
        private string              _CdnHost;
        private string              _IPBlacklistFile;
        private string              _ReferrerBlacklistFile;
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
        private VaultConfig         _VaultConfig;
        private StatsConfig         _StatsConfig;
        private LoggingConfig       _LoggingConfig;
        private PiwikConfig         _PiwikConfig;
        private IRCConfig           _IRCConfig;

        public bool         DevEnvironment          { get { return _DevEnvironment; }       set { _DevEnvironment = value; } }
        public bool         Migrate                 { get { return _Migrate; }              set { _Migrate = value; } }
        public bool         UseCdn                  { get { return _UseCdn; }               set { _UseCdn = value; } }

        // Site Information
        public string       Title                   { get { return _Title; }                set { _Title = value; } }
        public string       Description             { get { return _Description; }          set { _Description = value; } }
        public string       Author                  { get { return _Author; }               set { _Author = value; } }
        public string       Host                    { get { return _Host; }                 set { _Host = value; } }
        public string       SupportEmail            { get { return _SupportEmail; }         set { _SupportEmail = value; } }
        public string       NoReplyEmail            { get { return _NoReplyEmail; }         set { _NoReplyEmail = value; } }
        public string       BitcoinAddress          { get { return _BitcoinAddress; }       set { _BitcoinAddress = value; } }
        public string       Salt1                   { get { return _Salt1; }                set { _Salt1 = value; } }
        public string       Salt2                   { get { return _Salt2; }                set { _Salt2 = value; } }
        public string       CdnHost                 { get { return _CdnHost; }              set { _CdnHost = value; } }
        public string       IPBlacklistFile         { get { return _IPBlacklistFile;}       set { _IPBlacklistFile = value; }}
        public string       ReferrerBlacklistFile   { get { return _ReferrerBlacklistFile;} set { _ReferrerBlacklistFile = value; }}

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

        // Vault Configuration
        public VaultConfig          VaultConfig         { get { return _VaultConfig; }          set { _VaultConfig = value; } }

        // Status Configuration
        public StatsConfig          StatsConfig         { get { return _StatsConfig; }          set { _StatsConfig = value; } }

        // Logging Configuration
        public LoggingConfig        LoggingConfig       { get { return _LoggingConfig; }        set { _LoggingConfig = value; } }

        // Piwik Configuration
        public PiwikConfig          PiwikConfig         { get { return _PiwikConfig; }          set { _PiwikConfig = value; } }

        // Piwik Configuration
        public IRCConfig            IRCConfig           { get { return _IRCConfig; }            set { _IRCConfig = value; } }

        public Config()
        {
            _ConfigRWLock = new ReaderWriterLockSlim();
            _ConfigFileRWLock = new ReaderWriterLockSlim();
            _JsonSettings               = new JsonSerializerSettings();
            _JsonSettings.Formatting    = Formatting.Indented;

            SetDefaults();
        }

        public void SetDefaults()
        {
            DevEnvironment          = false;
            Migrate                 = false;
            UseCdn                  = false;
            Title                   = string.Empty;
            Description             = string.Empty;
            Author                  = string.Empty;
            Host                    = string.Empty;
            SupportEmail            = string.Empty;
            NoReplyEmail            = string.Empty;
            BitcoinAddress          = string.Empty;
            Salt1                   = string.Empty;
            Salt2                   = string.Empty;
            CdnHost                 = string.Empty;
            IPBlacklistFile         = string.Empty;
            ReferrerBlacklistFile   = string.Empty;
            UserConfig              = new UserConfig();
            EmailConfig             = new EmailConfig();
            ContactConfig           = new ContactConfig();
            GitConfig               = new GitConfig();
            BlogConfig              = new BlogConfig();
            UploadConfig            = new UploadConfig();
            PasteConfig             = new PasteConfig();
            ApiConfig               = new ApiConfig();
            PodcastConfig           = new PodcastConfig();
            StreamConfig            = new StreamConfig();
            ShortenerConfig         = new ShortenerConfig();
            VaultConfig             = new VaultConfig();
            StatsConfig             = new StatsConfig();
            LoggingConfig           = new LoggingConfig();
            PiwikConfig             = new PiwikConfig();
            IRCConfig               = new IRCConfig();
        }

        public static Config Deserialize(string text)
        {
            return JsonConvert.DeserializeObject<Config>(text);
        }

        public static string Serialize(Config config)
        {
            return JsonConvert.SerializeObject(config, Formatting.Indented);
        }

        public static Config Load(string path)
        {
            string newHash = string.Empty;
            string fullPath = Path.Combine(path, _ConfigFileName);

            if (!File.Exists(fullPath))
            {
                Config config = new Config();
                Save(fullPath, config);
            }

            newHash = MD5.FileHash(fullPath);
            
            if (_Config == null || _FileHash == null || newHash != _FileHash)
            {
                string configContents = File.ReadAllText(fullPath);
                _Config = Deserialize(configContents);
                _FileHash = newHash;
            }
            return _Config;
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
