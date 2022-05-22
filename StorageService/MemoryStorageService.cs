using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teknik.Configuration;
using Teknik.Utilities;
using Teknik.Utilities.Cryptography;

namespace Teknik.StorageService
{
    public class MemoryStorageService : StorageService
    {
        private static ConcurrentDictionary<string, byte[]> _files;
        private ConcurrentDictionary<string, byte[]> Files
        {
            get
            {
                if (_files == null)
                    _files = new ConcurrentDictionary<string, byte[]>();
                return _files;
            }
            set
            {
                _files = value;
            }
        }

        public MemoryStorageService(StorageConfig config) : base(config)
        {
        }

        public override string GetUniqueFileName()
        {
            string filename = StringHelper.RandomString(_config.FileNameLength);
            while (Files.ContainsKey(string.Format("{0}.{1}", filename, _config.FileExtension)))
            {
                filename = StringHelper.RandomString(_config.FileNameLength);
            }
            return filename;
        }

        public override List<string> GetFileNames()
        {
            return Files.Keys.ToList();
        }

        public override Stream GetFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;
            if (!Files.ContainsKey(fileName))
                return null;

            return new MemoryStream(Files[fileName]);
        }

        public override async Task SaveEncryptedFile(string fileName, Stream file, byte[] key, byte[] iv)
        {
            if (file == null ||
                Files.ContainsKey(fileName))
                return;

            using (var ms = new MemoryStream())
            {
                await AesCounterManaged.EncryptToStream(file, ms, key, iv);
                Files.TryAdd(fileName, ms.ToArray());
            }
        }

        public override async Task SaveFile(string fileName, Stream file)
        {
            if (file == null ||
                Files.ContainsKey(fileName))
                return;

            using (var ms = new MemoryStream())
            {
                file.Seek(0, SeekOrigin.Begin);
                await file.CopyToAsync(ms);
                Files.TryAdd(fileName, ms.ToArray());
            }
        }

        public override void DeleteFile(string fileName)
        {
            if (Files.ContainsKey(fileName))
                Files.TryRemove(fileName, out _);
        }
    }
}
