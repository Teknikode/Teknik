using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Teknik.Configuration;
using Teknik.Utilities;

namespace Teknik.StorageService
{
    public interface IStorageService
    {
        public string GetUniqueFileName();
        public Stream GetFile(string fileName);
        public List<string> GetFileNames();
        public Task SaveFile(string fileName, Stream file);
        public Task SaveEncryptedFile(string fileName, Stream file, PooledArray key, PooledArray iv);
        public void DeleteFile(string fileName);
    }
}
