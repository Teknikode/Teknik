using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using Teknik.Configuration;
using Teknik.Models;

namespace Teknik.Areas.Upload
{
    public static class Uploader
    {
        public static Models.Upload SaveFile(byte[] file, string contentType, int contentLength)
        {
            return SaveFile(file, contentType, contentLength, null, null, 256, 128);
        }

        public static Models.Upload SaveFile(byte[] file, string contentType, int contentLength, string iv)
        {
            return SaveFile(file, contentType, contentLength, iv, null, 256, 128);
        }

        public static Models.Upload SaveFile(byte[] file, string contentType, int contentLength, string iv, string key)
        {
            return SaveFile(file, contentType, contentLength, iv, key, 256, 128);
        }

        public static Models.Upload SaveFile(byte[] file, string contentType, int contentLength, string iv, string key, int keySize, int blockSize)
        {
            Config config = Config.Load();
            TeknikEntities db = new TeknikEntities();

            if (!Directory.Exists(config.UploadConfig.UploadDirectory))
            {
                Directory.CreateDirectory(config.UploadConfig.UploadDirectory);
            }

            // Generate a unique file name that does not currently exist
            string fileName = Utility.GenerateUniqueFileName(config.UploadConfig.UploadDirectory, config.UploadConfig.FileExtension, 10);

            // once we have the filename, lets save the file
            File.WriteAllBytes(fileName, file);

            // Generate a unique url
            string extension = (config.UploadConfig.IncludeExtension) ? Utility.GetDefaultExtension(contentType) : string.Empty;
            string url = Utility.RandomString(config.UploadConfig.UrlLength) + extension;
            while (db.Uploads.Where(u => u.Url == url).FirstOrDefault() != null)
            {
                url = Utility.RandomString(config.UploadConfig.UrlLength) + extension;
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