using System;
using System.Threading;
using Newtonsoft.Json;

namespace Teknik
{
    public class Config
    {
        private ReaderWriterLockSlim _ConfigRWLock;
        private ReaderWriterLockSlim _ConfigFileRWLock;
        private JsonSerializerSettings _JsonSettings;

        private bool    _DevEnvironment;
        private string  _Title;
        private string  _Description;
        private string  _Author;
        private string  _Host;

        public bool     DevEnvironment  { get { return _DevEnvironment; }   set { _DevEnvironment = value; } }
        public string   Title           { get { return _Title; }            set { _Title = value; } }
        public string   Description     { get { return _Description; }      set { _Description = value; } }
        public string   Author          { get { return _Author; }           set { _Author = value; } }
        public string   Host            { get { return _Host; }             set { _Host = value; } }

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
            DevEnvironment = false;
            Title = String.Empty;
            Description = String.Empty;
            Author = String.Empty;
            Host = String.Empty;
        }

        public static Config Deserialize(string text)
        {
            return JsonConvert.DeserializeObject<Config>(text);
        }

        public static string Serialize(Config config)
        {
            return JsonConvert.SerializeObject(config);
        }
    }
}