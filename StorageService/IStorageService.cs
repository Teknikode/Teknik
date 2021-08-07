using System;
using System.Collections.Generic;
using System.IO;
using Teknik.Configuration;

namespace StorageService
{
    public interface IStorageService
    {
        public string GetUniqueFileName();
        public Stream GetFile(string fileName);
        public List<string> GetFileNames();
        public void SaveFile(string fileName, Stream file);
        public void SaveEncryptedFile(string fileName, Stream file, int chunkSize, byte[] key, byte[] iv);
        public void DeleteFile(string fileName);
    }
}
