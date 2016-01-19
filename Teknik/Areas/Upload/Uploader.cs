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
        public static Models.Upload SaveFile(HttpPostedFileWrapper file, string contentType)
        {
            return SaveFile(file, contentType, null, null);
        }

        public static Models.Upload SaveFile(HttpPostedFileWrapper file, string contentType, string iv)
        {
            return SaveFile(file, contentType, iv, null);
        }

        public static Models.Upload SaveFile(HttpPostedFileWrapper file, string contentType, string iv, string key)
        {
            Config config = Config.Load();
            TeknikEntities db = new TeknikEntities();

            // Generate a unique file name that does not currently exist
            string fileName = Utility.GenerateUniqueFileName(config.UploadConfig.UploadDirectory, config.UploadConfig.FileExtension, 10);

            // once we have the filename, lets save the file
            file.SaveAs(fileName);

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
            upload.ContentLength = file.ContentLength;
            upload.ContentType = contentType;
            upload.Key = key;
            upload.IV = iv;

            db.Uploads.Add(upload);

            return upload;
        }
    }
}