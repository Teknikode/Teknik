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

        private bool            _DevEnvironment;
        private string          _Title;
        private string          _Description;
        private string          _Author;
        private string          _Host;
        private SMTPConfig      _SMTPConfig;
        private UploadConfig    _UploadConfig;
        private PasteConfig     _PasteConfig;
        private BlogConfig      _BlogConfig;
        private ApiConfig       _ApiConfig;
        private PodcastConfig   _PodcastConfig;
        private string          _SupportEmail;
        private string          _BitcoinAddress;

        public bool         DevEnvironment  { get { return _DevEnvironment; }   set { _DevEnvironment = value; } }

        // Site Information
        public string       Title           { get { return _Title; }            set { _Title = value; } }
        public string       Description     { get { return _Description; }      set { _Description = value; } }
        public string       Author          { get { return _Author; }           set { _Author = value; } }
        public string       Host            { get { return _Host; }             set { _Host = value; } }

        // Mail Server Configuration
        public SMTPConfig       SMTPConfig      { get { return _SMTPConfig; }       set { _SMTPConfig = value; } }

        // Contact Configuration
        public string           SupportEmail    { get { return _SupportEmail; }     set { _SupportEmail = value; } }

        // About Configuration
        public string           BitcoinAddress  { get { return _BitcoinAddress; }   set { _BitcoinAddress = value; } }

        // Blog Configuration
        public BlogConfig       BlogConfig      { get { return _BlogConfig; }       set { _BlogConfig = value; } }

        // Upload Configuration
        public UploadConfig     UploadConfig    { get { return _UploadConfig; }     set { _UploadConfig = value; } }

        // Paste Configuration
        public PasteConfig      PasteConfig     { get { return _PasteConfig; }      set { _PasteConfig = value; } }

        // API Configuration
        public ApiConfig        ApiConfig       { get { return _ApiConfig; }        set { _ApiConfig = value; } }

        // Podcast Configuration
        public PodcastConfig    PodcastConfig   { get { return _PodcastConfig; }    set { _PodcastConfig = value; } }

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
            DevEnvironment  = false;
            Title           = string.Empty;
            Description     = string.Empty;
            Author          = string.Empty;
            Host            = string.Empty;
            SMTPConfig      = new SMTPConfig();
            BlogConfig      = new BlogConfig();
            UploadConfig    = new UploadConfig();
            PasteConfig     = new PasteConfig();
            ApiConfig       = new ApiConfig();
            PodcastConfig   = new PodcastConfig();
            SupportEmail    = string.Empty;
            BitcoinAddress  = string.Empty;
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
            Config config = new Config();
            string path = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            if (!File.Exists(Path.Combine(path, "Config.json")))
            {
                Config.Save(Path.Combine(path, "Config.json"), config);
            }
            else
            {
                string configContents = File.ReadAllText(Path.Combine(path, "Config.json"));
                config = Config.Deserialize(configContents);
            }
            return config;
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