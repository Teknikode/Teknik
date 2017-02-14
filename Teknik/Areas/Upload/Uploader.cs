using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using Teknik.Configuration;
using Teknik.Models;
using Teknik.Utilities;
using System.Text;
using Org.BouncyCastle.Utilities.Encoders;

namespace Teknik.Areas.Upload
{
    public static class Uploader
    {
        public static Models.Upload SaveFile(TeknikEntities db, Config config, System.IO.Stream file, string contentType, int contentLength, bool encrypt)
        {
            return SaveFile(db, config, file, contentType, contentLength, encrypt, string.Empty, null, null, 256, 128);
        }
        public static Models.Upload SaveFile(TeknikEntities db, Config config, System.IO.Stream file, string contentType, int contentLength, bool encrypt, string defaultExtension)
        {
            return SaveFile(db, config, file, contentType, contentLength, encrypt, defaultExtension, null, null, 256, 128);
        }

        public static Models.Upload SaveFile(TeknikEntities db, Config config, System.IO.Stream file, string contentType, int contentLength, bool encrypt, string defaultExtension, string iv)
        {
            return SaveFile(db, config, file, contentType, contentLength, encrypt, defaultExtension, iv, null, 256, 128);
        }

        public static Models.Upload SaveFile(TeknikEntities db, Config config, System.IO.Stream file, string contentType, int contentLength, bool encrypt, string defaultExtension, string iv, string key)
        {
            return SaveFile(db, config, file, contentType, contentLength, encrypt, defaultExtension, iv, key, 256, 128);
        }

        public static Models.Upload SaveFile(TeknikEntities db, Config config, System.IO.Stream file, string contentType, int contentLength, bool encrypt, string defaultExtension, string iv, string key, int keySize, int blockSize)
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
                key = StringHelper.RandomString(config.UploadConfig.KeySize / 8);
                iv = StringHelper.RandomString(config.UploadConfig.BlockSize / 8);

                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                byte[] ivBytes = Encoding.UTF8.GetBytes(iv);

                // Encrypt the file to disk
                AES.EncryptToFile(filePath, file, config.UploadConfig.ChunkSize, keyBytes, ivBytes, "CTR", "NoPadding");
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
            string extension = (config.UploadConfig.IncludeExtension) ? FileHelper.GetDefaultExtension(contentType, defaultExtension) : string.Empty;
            string url = StringHelper.RandomString(config.UploadConfig.UrlLength) + extension;
            while (db.Uploads.Where(u => u.Url == url).FirstOrDefault() != null)
            {
                url = StringHelper.RandomString(config.UploadConfig.UrlLength) + extension;
            }

            // Generate a deletion key
            string delKey = StringHelper.RandomString(config.UploadConfig.DeleteKeyLength);

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
            upload.DeleteKey = delKey;

            db.Uploads.Add(upload);
            db.SaveChanges();

            return upload;
        }
    }
}