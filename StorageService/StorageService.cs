using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Teknik.Configuration;

namespace Teknik.StorageService
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
        public abstract Task SaveFile(string fileName, Stream file);
        public abstract Task SaveEncryptedFile(string fileName, Stream file, byte[] key, byte[] iv);
        public abstract void DeleteFile(string fileName);
    }
}
