﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teknik.Configuration;

namespace StorageService
{
    public static class StorageServiceFactory
    {
        public static StorageService GetStorageService(StorageConfig config)
        {
            switch (config.Type)
            {
                case StorageType.Local:
                    return new LocalStorageService(config);
                case StorageType.S3:
                default:
                    return null;
            }
        }
    }
}