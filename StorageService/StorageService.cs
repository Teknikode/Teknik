using System;
using System.Collections.Generic;
using System.IO;
using Teknik.Configuration;

namespace StorageService
{
    public abstract class StorageService : IStorageService
    {
        protected readonly StorageConfig _config;

        public StorageService(StorageConfig config)
        {
            _config = config;
        }

        public abstract string GetUniqueFileName();
        public abstract Stream GetFile(string fileName);
        public abstract List<string> GetFileNames();
        public abstract void SaveFile(string fileName, Stream file);
        public abstract void SaveEncryptedFile(string fileName, Stream file, int chunkSize, byte[] key, byte[] iv);
        public abstract void DeleteFile(string fileName);
    }
}
