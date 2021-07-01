using System;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Teknik.IdentityServer.Data.Migrations.IdentityServer.PersistedGrantDb
{
    public partial class V3toV4PersistedGrantDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Migrate config db to v4
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Teknik.IdentityServer.Data.Migrations.IdentityServer.PersistedGrantDb.PersistedGrantDbContext.sql";

            using Stream stream = assembly.GetManifestResourceStream(resourceName);
            using StreamReader sr = new StreamReader(stream);
            {
                var sql = sr.ReadToEnd();
                migrationBuilder.Sql(sql);
            }
        }
    }
}
