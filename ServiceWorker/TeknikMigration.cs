using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teknik.Areas.Paste;
using Teknik.Areas.Paste.Models;
using Teknik.Configuration;
using Teknik.Data;
using Teknik.StorageService;
using Teknik.Utilities;
using Teknik.Utilities.Cryptography;

namespace Teknik.ServiceWorker
{
    public static class TeknikMigration
    {
        public static bool RunMigration(TeknikEntities db, Config config)
        {
            bool success = false;
            

            return success;
        }
    }
}
