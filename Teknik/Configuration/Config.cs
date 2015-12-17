﻿using System;
using System.IO;
using System.Threading;
using System.Web;
using Newtonsoft.Json;

namespace Teknik
{
    public class Config
    {
        private ReaderWriterLockSlim _ConfigRWLock;
        private ReaderWriterLockSlim _ConfigFileRWLock;
        private JsonSerializerSettings _JsonSettings;

        private bool        _DevEnvironment;
        private string      _Title;
        private string      _Description;
        private string      _Author;
        private string      _Host;
        private SMTPConfig  _SMTPConfig;
        private string      _SupportEmail;
        private string      _BitcoinAddress;
        private int         _PostsToLoad;
        private int         _CommentsToLoad;

        public bool         DevEnvironment  { get { return _DevEnvironment; }   set { _DevEnvironment = value; } }

        public string       Title           { get { return _Title; }            set { _Title = value; } }
        public string       Description     { get { return _Description; }      set { _Description = value; } }
        public string       Author          { get { return _Author; }           set { _Author = value; } }

        public string       Host            { get { return _Host; }             set { _Host = value; } }

        public SMTPConfig   SMTPConfig      { get { return _SMTPConfig; }       set { _SMTPConfig = value; } }

        public string       SupportEmail    { get { return _SupportEmail; }     set { _SupportEmail = value; } }

        public string       BitcoinAddress  { get { return _BitcoinAddress; }   set { _BitcoinAddress = value; } }

        public int          PostsToLoad     { get { return _PostsToLoad; }      set { _PostsToLoad = value; } }

        public int          CommentsToLoad  { get { return _CommentsToLoad; }   set { _CommentsToLoad = value; } }

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
            Title           = String.Empty;
            Description     = String.Empty;
            Author          = String.Empty;
            Host            = String.Empty;
            SMTPConfig      = new SMTPConfig();
            SupportEmail    = string.Empty;
            BitcoinAddress  = string.Empty;
            PostsToLoad     = 10;
            CommentsToLoad  = 10;
        }

        public static Config Deserialize(string text)
        {
            return JsonConvert.DeserializeObject<Config>(text);
        }

        public static string Serialize(Config config)
        {
            return JsonConvert.SerializeObject(config);
        }

        public static Config Load()
        {
            Config config = new Config();
            string path = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            if (File.Exists(Path.Combine(path, "Config.json")))
            {
                string configContents = File.ReadAllText(Path.Combine(path, "Config.json"));
                config = Config.Deserialize(configContents);
            }
            return config;
        }
    }
}