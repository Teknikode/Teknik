using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using Teknik.Configuration;
using Teknik.Models;
using Teknik.Utilities;
using System.Text;
using Teknik.Utilities.Cryptography;
using Teknik.Data;
using Teknik.StorageService;
using Teknik.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Teknik.Areas.Upload
{
    public static class UploadHelper
    {
        private static object _cacheLock = new object();
        private readonly static ObjectCache _uploadCache = new ObjectCache(300);

        public static async Task<Models.Upload> SaveFile(TeknikEntities db, Config config, Stream file, string contentType, long contentLength, bool encrypt, ExpirationUnit expirationUnit, int expirationLength)
        {
            return await SaveFile(db, config, file, contentType, contentLength, encrypt, expirationUnit, expirationLength, string.Empty, null, null, 256, 128);
        }

        public static async Task<Models.Upload> SaveFile(TeknikEntities db, Config config, Stream file, string contentType, long contentLength, bool encrypt, ExpirationUnit expirationUnit, int expirationLength, string fileExt)
        {
            return await SaveFile(db, config, file, contentType, contentLength, encrypt, expirationUnit, expirationLength, fileExt, null, null, 256, 128);
        }

        public static async Task<Models.Upload> SaveFile(TeknikEntities db, Config config, Stream file, string contentType, long contentLength, bool encrypt, ExpirationUnit expirationUnit, int expirationLength, string fileExt, string iv)
        {
            return await SaveFile(db, config, file, contentType, contentLength, encrypt, expirationUnit, expirationLength, fileExt, iv, null, 256, 128);
        }

        public static async Task<Models.Upload> SaveFile(TeknikEntities db, Config config, Stream file, string contentType, long contentLength, bool encrypt, ExpirationUnit expirationUnit, int expirationLength, string fileExt, string iv, string key)
        {
            return await SaveFile(db, config, file, contentType, contentLength, encrypt, expirationUnit, expirationLength, fileExt, iv, key, 256, 128);
        }

        public static async Task<Models.Upload> SaveFile(TeknikEntities db, Config config, Stream file, string contentType, long contentLength, bool encrypt, ExpirationUnit expirationUnit, int expirationLength, string fileExt, string iv, string key, int keySize, int blockSize)
        {
            var storageService = StorageServiceFactory.GetStorageService(config.UploadConfig.StorageConfig);

            // Generate a unique file name that does not currently exist
            var fileName = storageService.GetUniqueFileName();

            // once we have the filename, lets save the file
            if (encrypt)
            {
                // Generate a key and iv
                if (string.IsNullOrEmpty(key))
                    key = StringHelper.RandomString(config.UploadConfig.KeySize / 8);
                if (string.IsNullOrEmpty(iv))
                    iv = StringHelper.RandomString(config.UploadConfig.BlockSize / 8);

                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                byte[] ivBytes = Encoding.UTF8.GetBytes(iv);

                using (var keyArray = new PooledArray(keyBytes))
                using (var ivArray = new PooledArray(ivBytes))
                {
                    await storageService.SaveEncryptedFile(fileName, file, keyArray, ivArray);
                }
            }
            else
            {
                await storageService.SaveFile(fileName, file);
            }

            // Generate a unique url
            string extension = (config.UploadConfig.IncludeExtension) ? fileExt : string.Empty;
            string url = StringHelper.RandomString(config.UploadConfig.UrlLength) + extension;
            while (db.Uploads.Where(u => u.Url == url).FirstOrDefault() != null)
            {
                url = StringHelper.RandomString(config.UploadConfig.UrlLength) + extension;
            }

            // Generate a deletion key
            string delKey = StringHelper.RandomString(config.UploadConfig.DeleteKeyLength);

            // Now we need to update the database with the new upload information
            Models.Upload upload = new Models.Upload();
            upload.DateUploaded = DateTime.Now;
            upload.Url = url;
            upload.FileName = fileName;
            upload.ContentType = (!string.IsNullOrEmpty(contentType)) ? contentType : "application/octet-stream";
            upload.ContentLength = contentLength;
            upload.Key = key;
            upload.IV = iv;
            upload.KeySize = keySize;
            upload.BlockSize = blockSize;
            upload.DeleteKey = delKey;

            if (expirationUnit == ExpirationUnit.Views)
            {
                upload.MaxDownloads = expirationLength;
            }
            else
            {
                switch (expirationUnit)
                {
                    case ExpirationUnit.Minutes:
                        upload.ExpireDate = DateTime.Now.AddMinutes(expirationLength);
                        break;
                    case ExpirationUnit.Hours:
                        upload.ExpireDate = DateTime.Now.AddHours(expirationLength);
                        break;
                    case ExpirationUnit.Days:
                        upload.ExpireDate = DateTime.Now.AddDays(expirationLength);
                        break;
                    case ExpirationUnit.Months:
                        upload.ExpireDate = DateTime.Now.AddMonths(expirationLength);
                        break;
                    case ExpirationUnit.Years:
                        upload.ExpireDate = DateTime.Now.AddYears(expirationLength);
                        break;
                }
            }

            db.Uploads.Add(upload);
            await db.SaveChangesAsync();

            return upload;
        }

        public static string GenerateDeleteKey(TeknikEntities db, Config config, string url)
        {
            var upload = db.Uploads.FirstOrDefault(up => up.Url == url);
            if (upload != null)
            {
                string delKey = StringHelper.RandomString(config.UploadConfig.DeleteKeyLength);
                upload.DeleteKey = delKey;
                ModifyUpload(db, upload);
                return delKey;
            }
            return null;
        }

        public static bool CheckExpiration(Models.Upload upload)
        {
            if (upload.ExpireDate != null && DateTime.Now >= upload.ExpireDate)
                return true;
            if (upload.MaxDownloads > 0 && upload.Downloads >= upload.MaxDownloads)
                return true;

            return false;
        }

        public static void IncrementDownloadCount(IBackgroundTaskQueue queue, Config config, string url)
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
                        var upload = GetUpload(db, url);
                        if (upload != null)
                        {
                            upload.Downloads++;
                            ModifyUpload(db, upload);
                        }
                    }
                });
            });
        }

        public static Models.Upload GetUpload(TeknikEntities db, string url)
        {
            lock (_cacheLock)
            {
                var upload = _uploadCache.GetObject(url, (key) => db.Uploads.FirstOrDefault(up => up.Url == key));

                if (upload != null &&
                    !db.Exists(upload))
                    db.Attach(upload);

                return upload;
            }
        }

        public static void DeleteFile(TeknikEntities db, Config config, ILogger<Logger> logger, string url)
        {
            var upload = GetUpload(db, url);
            if (upload != null)
            {
                try
                {
                    var storageService = StorageServiceFactory.GetStorageService(config.UploadConfig.StorageConfig);
                    storageService.DeleteFile(upload.FileName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unable to delete file: {0}", upload.FileName);
                }

                // Delete from the DB
                db.Uploads.Remove(upload);
                db.SaveChanges();
            }

            // Remove from the cache
            lock (_cacheLock)
            {
                _uploadCache.DeleteObject(url);
            }
        }

        public static void ModifyUpload(TeknikEntities db, Models.Upload upload)
        {
            // Update the cache's copy
            lock (_cacheLock)
            {
                _uploadCache.UpdateObject(upload.Url, upload);
            }

            if (upload != null)
            {
                if (!db.Exists(upload))
                    db.Attach(upload);

                // Update the database
                db.Entry(upload).State = EntityState.Modified;
                db.SaveChanges();
            }
        }
    }
}
