﻿using System;
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
using Teknik.StorageService;
using Teknik.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Teknik.Areas.Paste
{
    public static class PasteHelper
    {
        public static async Task<Models.Paste> CreatePaste(Config config, TeknikEntities db, string content, string title = "", string syntax = "text", ExpirationUnit expireUnit = ExpirationUnit.Never, int expireLength = 1, string password = "")
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
            switch (expireUnit)
            {
                case ExpirationUnit.Never:
                    break;
                case ExpirationUnit.Views:
                    paste.MaxViews = expireLength;
                    break;
                case ExpirationUnit.Minutes:
                    paste.ExpireDate = paste.DatePosted.AddMinutes(expireLength);
                    break;
                case ExpirationUnit.Hours:
                    paste.ExpireDate = paste.DatePosted.AddHours(expireLength);
                    break;
                case ExpirationUnit.Days:
                    paste.ExpireDate = paste.DatePosted.AddDays(expireLength);
                    break;
                case ExpirationUnit.Months:
                    paste.ExpireDate = paste.DatePosted.AddMonths(expireLength);
                    break;
                case ExpirationUnit.Years:
                    paste.ExpireDate = paste.DatePosted.AddYears(expireLength);
                    break;
                default:
                    break;
            }

            string key = Crypto.GenerateKey(config.PasteConfig.KeySize);
            string iv = Crypto.GenerateIV(config.PasteConfig.BlockSize);

            if (!string.IsNullOrEmpty(password))
            {
                paste.HashedPassword = Crypto.HashPassword(key, password);
            }

            // Generate a unique file name that does not currently exist
            var storageService = StorageServiceFactory.GetStorageService(config.PasteConfig.StorageConfig);
            var fileName = storageService.GetUniqueFileName();

            // Encrypt the contents to the file
            await EncryptContents(storageService, content, fileName, password, key, iv, config.PasteConfig.KeySize, config.PasteConfig.ChunkSize);

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

        public static async Task EncryptContents(IStorageService storageService, string content, string fileName, string password, string key, string iv, int keySize, int chunkSize)
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
            using (var keyArray = new PooledArray(keyBytes))
            using (var ivArray = new PooledArray(ivBytes))
            {
                await storageService.SaveEncryptedFile(fileName, ms, keyArray, ivArray);
            }
        }

        public static void IncrementViewCount(IBackgroundTaskQueue queue, ObjectCache cache, Config config, string url)
        {
            // Fire and forget updating of the download count
            queue.QueueBackgroundWorkItem(async token =>
            {
                await Task.Run(() =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder<TeknikEntities>();
                    optionsBuilder.UseSqlServer(config.DbConnection);

                    using (TeknikEntities db = new TeknikEntities(optionsBuilder.Options))
                    {
                        var paste = GetPaste(db, cache, url);
                        if (paste != null)
                        {
                            paste.Views++;
                            ModifyPaste(db, cache, paste);
                        }
                    }
                });
            });
        }

        public static Models.Paste GetPaste(TeknikEntities db, ObjectCache cache, string url)
        {
            var paste = cache.AddOrGetObject(url, (key) => db.Pastes.FirstOrDefault(up => up.Url == key));

            if (paste != null &&
                !db.Exists(paste))
                db.Attach(paste);

            return paste;
        }

        public static void DeleteFile(TeknikEntities db, ObjectCache cache, Config config, ILogger<Logger> logger, string url)
        {
            var paste = GetPaste(db, cache, url);
            if (paste != null)
            {
                try
                {
                    var storageService = StorageServiceFactory.GetStorageService(config.PasteConfig.StorageConfig);
                    storageService.DeleteFile(paste.FileName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unable to delete file: {0}", paste.FileName);
                }

                // Delete from the DB
                db.Pastes.Remove(paste);
                db.SaveChanges();
            }

            // Remove from the cache
            cache.DeleteObject<PasteConfig>(url);
        }

        public static void ModifyPaste(TeknikEntities db, ObjectCache cache, Models.Paste paste)
        {
            // Update the cache's copy
            cache.UpdateObject(paste.Url, paste);

            if (paste != null)
            {
                if (!db.Exists(paste))
                    db.Attach(paste);

                // Update the database
                db.Entry(paste).State = EntityState.Modified;
                db.SaveChanges();
            }
        }
    }
}