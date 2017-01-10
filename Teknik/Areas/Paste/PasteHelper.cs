using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Teknik.Configuration;
using Teknik.Helpers;
using Teknik.Models;
using Teknik.Utilities;

namespace Teknik.Areas.Paste
{
    public static class PasteHelper
    {
        public static Models.Paste CreatePaste(string content, string title = "", string syntax = "text", string expireUnit = "never", int expireLength = 1, string password = "", bool hide = false)
        {
            TeknikEntities db = new TeknikEntities();
            Config config = Config.Load();
            Models.Paste paste = db.Pastes.Create();
            paste.DatePosted = DateTime.Now;
            paste.MaxViews = 0;
            paste.Views = 0;

            // Generate random url
            string url = StringHelper.RandomString(config.PasteConfig.UrlLength);
            while (db.Pastes.Where(p => p.Url == url).FirstOrDefault() != null)
            {
                url = StringHelper.RandomString(config.PasteConfig.UrlLength);
            }
            paste.Url = url;

            // Figure out the expire date (null if 'never' or 'visit')
            switch (expireUnit.ToLower())
            {
                case "never":
                    break;
                case "view":
                    paste.MaxViews = expireLength;
                    break;
                case "minute":
                    paste.ExpireDate = paste.DatePosted.AddMinutes(expireLength);
                    break;
                case "hour":
                    paste.ExpireDate = paste.DatePosted.AddHours(expireLength);
                    break;
                case "day":
                    paste.ExpireDate = paste.DatePosted.AddDays(expireLength);
                    break;
                case "month":
                    paste.ExpireDate = paste.DatePosted.AddMonths(expireLength);
                    break;
                case "year":
                    paste.ExpireDate = paste.DatePosted.AddYears(expireLength);
                    break;
                default:
                    break;
            }

            // Set the hashed password if one is provided and encrypt stuff
            if (!string.IsNullOrEmpty(password))
            {
                string key = StringHelper.RandomString(config.PasteConfig.KeySize / 8);
                string iv = StringHelper.RandomString(config.PasteConfig.BlockSize / 16);
                paste.HashedPassword = SHA384.Hash(key, password).ToHex();

                // Encrypt Content
                byte[] data = Encoding.Unicode.GetBytes(content);
                byte[] ivBytes = Encoding.Unicode.GetBytes(iv);
                byte[] keyBytes = AES.CreateKey(password, ivBytes, config.PasteConfig.KeySize);
                byte[] encData = AES.Encrypt(data, keyBytes, ivBytes);
                content = Convert.ToBase64String(encData);

                paste.Key = key;
                paste.KeySize = config.PasteConfig.KeySize;
                paste.IV = iv;
                paste.BlockSize = config.PasteConfig.BlockSize;
            }

            paste.Content = content;
            paste.Title = title;
            paste.Syntax = syntax;
            paste.Hide = hide;

            return paste;
        }

        public static bool CheckExpiration(Models.Paste paste)
        {
            if (paste.ExpireDate != null && DateTime.Now >= paste.ExpireDate)
                return true;
            if (paste.MaxViews > 0 && paste.Views > paste.MaxViews)
                return true;

            return false;
        }
    }
}