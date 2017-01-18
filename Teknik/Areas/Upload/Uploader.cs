using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using Teknik.Configuration;
using Teknik.Models;
using Teknik.Utilities;

namespace Teknik.Areas.Upload
{
    public static class Uploader
    {
        public static Models.Upload SaveFile(TeknikEntities db, Config config, byte[] file, string contentType, int contentLength)
        {
            return SaveFile(db, config, file, contentType, contentLength, string.Empty, null, null, 256, 128);
        }
        public static Models.Upload SaveFile(TeknikEntities db, Config config, byte[] file, string contentType, int contentLength, string defaultExtension)
        {
            return SaveFile(db, config, file, contentType, contentLength, defaultExtension, null, null, 256, 128);
        }

        public static Models.Upload SaveFile(TeknikEntities db, Config config, byte[] file, string contentType, int contentLength, string defaultExtension, string iv)
        {
            return SaveFile(db, config, file, contentType, contentLength, defaultExtension, iv, null, 256, 128);
        }

        public static Models.Upload SaveFile(TeknikEntities db, Config config, byte[] file, string contentType, int contentLength, string defaultExtension, string iv, string key)
        {
            return SaveFile(db, config, file, contentType, contentLength, defaultExtension, iv, key, 256, 128);
        }

        public static Models.Upload SaveFile(TeknikEntities db, Config config, byte[] file, string contentType, int contentLength, string defaultExtension, string iv, string key, int keySize, int blockSize)
        {
            if (!Directory.Exists(config.UploadConfig.UploadDirectory))
            {
                Directory.CreateDirectory(config.UploadConfig.UploadDirectory);
            }

            // Generate a unique file name that does not currently exist
            string filePath = FileHelper.GenerateUniqueFileName(config.UploadConfig.UploadDirectory, config.UploadConfig.FileExtension, 10);
            string fileName = Path.GetFileName(filePath);

            // once we have the filename, lets save the file
            File.WriteAllBytes(filePath, file);

            // Generate a unique url
            string extension = (config.UploadConfig.IncludeExtension) ? FileHelper.GetDefaultExtension(contentType, defaultExtension) : string.Empty;
            string url = StringHelper.RandomString(config.UploadConfig.UrlLength) + extension;
            while (db.Uploads.Where(u => u.Url == url).FirstOrDefault() != null)
            {
                url = StringHelper.RandomString(config.UploadConfig.UrlLength) + extension;
            }

            // Now we need to update the database with the new upload information
            Models.Upload upload = db.Uploads.Create();
            upload.DateUploaded = DateTime.Now;
            upload.Url = url;
            upload.FileName = fileName;
            upload.ContentType = (!string.IsNullOrEmpty(contentType)) ? contentType : "application/octet-stream";
            upload.ContentLength = contentLength;
            upload.Key = key;
            upload.IV = iv;
            upload.KeySize = keySize;
            upload.BlockSize = blockSize;

            db.Uploads.Add(upload);
            db.SaveChanges();

            return upload;
        }
    }
}