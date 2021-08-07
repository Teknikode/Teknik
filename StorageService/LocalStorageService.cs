using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teknik.Configuration;
using Teknik.Utilities;
using Teknik.Utilities.Cryptography;

namespace StorageService
{
    public class LocalStorageService : StorageService
    {
        public LocalStorageService(StorageConfig config) : base(config)
        {
        }

        public override string GetUniqueFileName()
        {
            string filePath = FileHelper.GenerateRandomFileName(_config.LocalDirectory, _config.FileExtension, _config.FileNameLength);
            return Path.GetFileName(filePath);
        }

        public override List<string> GetFileNames()
        {
            return Directory.GetFiles(_config.LocalDirectory, "*.*", SearchOption.AllDirectories).Select(f => Path.GetFileName(f).ToLower()).ToList();
        }

        public override Stream GetFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;

            string filePath = GetFilePath(fileName);
            if (File.Exists(filePath))
                return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return null;
        }

        public override void SaveEncryptedFile(string fileName, Stream file, int chunkSize, byte[] key, byte[] iv)
        {
            if (!Directory.Exists(_config.LocalDirectory))
                Directory.CreateDirectory(_config.LocalDirectory);

            string filePath = GetFilePath(fileName);
            AesCounterManaged.EncryptToFile(filePath, file, chunkSize, key, iv);
        }

        public override void SaveFile(string fileName, Stream file)
        {
            if (!Directory.Exists(_config.LocalDirectory))
                Directory.CreateDirectory(_config.LocalDirectory);

            string filePath = GetFilePath(fileName);
            // Just write the stream to the file
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                file.Seek(0, SeekOrigin.Begin);
                file.CopyTo(fileStream);
            }
        }

        public override void DeleteFile(string fileName)
        {
            string filePath = GetFilePath(fileName);

            // Delete the File
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        private string GetFilePath(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;

            string subDir = fileName[0].ToString().ToLower();
            return Path.Combine(_config.LocalDirectory, subDir, fileName);
        }
    }
}
