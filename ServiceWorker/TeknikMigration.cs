using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Teknik.Areas.Paste;
using Teknik.Areas.Paste.Models;
using Teknik.Configuration;
using Teknik.Data;
using Teknik.Utilities;

namespace Teknik.ServiceWorker
{
    public static class TeknikMigration
    {
        public static bool RunMigration(TeknikEntities db, Config config)
        {
            bool success = false;

            MigratePastes(db, config);

            return success;
        }

        public static void MigratePastes(TeknikEntities db, Config config)
        {
            if (!Directory.Exists(config.PasteConfig.PasteDirectory))
            {
                Directory.CreateDirectory(config.PasteConfig.PasteDirectory);
            }

            foreach (var paste in db.Pastes)
            {
                if (!string.IsNullOrEmpty(paste.Content) && string.IsNullOrEmpty(paste.FileName) && string.IsNullOrEmpty(paste.HashedPassword))
                {
                    // Generate a unique file name that does not currently exist
                    string filePath = FileHelper.GenerateRandomFileName(config.PasteConfig.PasteDirectory, config.PasteConfig.FileExtension, 10);
                    string fileName = Path.GetFileName(filePath);

                    string key = PasteHelper.GenerateKey(config.PasteConfig.KeySize);
                    string iv = PasteHelper.GenerateIV(config.PasteConfig.BlockSize);

                    // Encrypt the contents to the file
                    PasteHelper.EncryptContents(paste.Content, filePath, null, key, iv, config.PasteConfig.KeySize, config.PasteConfig.ChunkSize);

                    // Generate a deletion key
                    paste.DeleteKey = StringHelper.RandomString(config.PasteConfig.DeleteKeyLength);

                    paste.Key = key;
                    paste.KeySize = config.PasteConfig.KeySize;
                    paste.IV = iv;
                    paste.BlockSize = config.PasteConfig.BlockSize;

                    paste.FileName = fileName;
                    paste.Content = string.Empty;

                    db.Entry(paste).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }
    }
}
