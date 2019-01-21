using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Teknik.Configuration;
using Teknik.Utilities;
using Teknik.Models;
using Teknik.Utilities.Cryptography;
using Teknik.Data;
using System.IO;

namespace Teknik.Areas.Paste
{
    public static class PasteHelper
    {
        public static Models.Paste CreatePaste(Config config, TeknikEntities db, string content, string title = "", string syntax = "text", string expireUnit = "never", int expireLength = 1, string password = "")
        {
            Models.Paste paste = new Models.Paste();
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

            if (!Directory.Exists(config.PasteConfig.PasteDirectory))
            {
                Directory.CreateDirectory(config.PasteConfig.PasteDirectory);
            }

            // Generate a unique file name that does not currently exist
            string filePath = FileHelper.GenerateRandomFileName(config.PasteConfig.PasteDirectory, config.PasteConfig.FileExtension, 10);
            string fileName = Path.GetFileName(filePath);

            string key = GenerateKey(config.PasteConfig.KeySize);
            string iv = GenerateIV(config.PasteConfig.BlockSize);

            if (!string.IsNullOrEmpty(password))
            {
                paste.HashedPassword = HashPassword(key, password);
            }

            // Encrypt the contents to the file
            EncryptContents(content, filePath, password, key, iv, config.PasteConfig.KeySize, config.PasteConfig.ChunkSize);

            // Generate a deletion key
            string delKey = StringHelper.RandomString(config.PasteConfig.DeleteKeyLength);

            paste.Key = key;
            paste.KeySize = config.PasteConfig.KeySize;
            paste.IV = iv;
            paste.BlockSize = config.PasteConfig.BlockSize;

            paste.FileName = fileName;
            //paste.Content = content;
            paste.Title = title;
            paste.Syntax = syntax;
            paste.DeleteKey = delKey;

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

        public static string GenerateKey(int keySize)
        {
            return StringHelper.RandomString(keySize / 8);
        }

        public static string GenerateIV(int ivSize)
        {
            return StringHelper.RandomString(ivSize / 16);
        }

        public static string HashPassword(string key, string password)
        {
            return SHA384.Hash(key, password).ToHex();
        }

        public static void EncryptContents(string content, string filePath, string password, string key, string iv, int keySize, int chunkSize)
        {
            byte[] ivBytes = Encoding.Unicode.GetBytes(iv);
            byte[] keyBytes = AesCounterManaged.CreateKey(key, ivBytes, keySize);

            // Set the hashed password if one is provided and modify the key
            if (!string.IsNullOrEmpty(password))
            {
                keyBytes = AesCounterManaged.CreateKey(password, ivBytes, keySize);
            }

            // Encrypt Content
            byte[] data = Encoding.Unicode.GetBytes(content);
            using (MemoryStream ms = new MemoryStream(data))
            {
                AesCounterManaged.EncryptToFile(filePath, ms, chunkSize, keyBytes, ivBytes);
            }
        }
    }
}