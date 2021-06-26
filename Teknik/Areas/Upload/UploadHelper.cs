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

namespace Teknik.Areas.Upload
{
    public static class UploadHelper
    {
        public static Models.Upload SaveFile(TeknikEntities db, Config config, Stream file, string contentType, long contentLength, bool encrypt, ExpirationUnit expirationUnit, int expirationLength)
        {
            return SaveFile(db, config, file, contentType, contentLength, encrypt, expirationUnit, expirationLength, string.Empty, null, null, 256, 128);
        }

        public static Models.Upload SaveFile(TeknikEntities db, Config config, Stream file, string contentType, long contentLength, bool encrypt, ExpirationUnit expirationUnit, int expirationLength, string fileExt)
        {
            return SaveFile(db, config, file, contentType, contentLength, encrypt, expirationUnit, expirationLength, fileExt, null, null, 256, 128);
        }

        public static Models.Upload SaveFile(TeknikEntities db, Config config, Stream file, string contentType, long contentLength, bool encrypt, ExpirationUnit expirationUnit, int expirationLength, string fileExt, string iv)
        {
            return SaveFile(db, config, file, contentType, contentLength, encrypt, expirationUnit, expirationLength, fileExt, iv, null, 256, 128);
        }

        public static Models.Upload SaveFile(TeknikEntities db, Config config, Stream file, string contentType, long contentLength, bool encrypt, ExpirationUnit expirationUnit, int expirationLength, string fileExt, string iv, string key)
        {
            return SaveFile(db, config, file, contentType, contentLength, encrypt, expirationUnit, expirationLength, fileExt, iv, key, 256, 128);
        }

        public static Models.Upload SaveFile(TeknikEntities db, Config config, Stream file, string contentType, long contentLength, bool encrypt, ExpirationUnit expirationUnit, int expirationLength, string fileExt, string iv, string key, int keySize, int blockSize)
        {
            if (!Directory.Exists(config.UploadConfig.UploadDirectory))
            {
                Directory.CreateDirectory(config.UploadConfig.UploadDirectory);
            }

            // Generate a unique file name that does not currently exist
            string filePath = FileHelper.GenerateRandomFileName(config.UploadConfig.UploadDirectory, config.UploadConfig.FileExtension, 10);
            string fileName = Path.GetFileName(filePath);

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

                // Encrypt the file to disk
                AesCounterManaged.EncryptToFile(filePath, file, config.UploadConfig.ChunkSize, keyBytes, ivBytes);
            }
            else
            {
                // Just write the stream to the file
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    file.Seek(0, SeekOrigin.Begin);
                    file.CopyTo(fileStream);
                }
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
            db.SaveChanges();

            return upload;
        }

        public static bool CheckExpiration(Models.Upload upload)
        {
            if (upload.ExpireDate != null && DateTime.Now >= upload.ExpireDate)
                return true;
            if (upload.MaxDownloads > 0 && upload.Downloads >= upload.MaxDownloads)
                return true;

            return false;
        }

        public static Models.Upload GetUpload(TeknikEntities db, string url)
        {
            Models.Upload upload = db.Uploads.Where(up => up.Url == url).FirstOrDefault();

            return upload;
        }
    }
}
