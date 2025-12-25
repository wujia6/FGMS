using FGMS.Mx.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;

namespace FGMS.Mx.Core
{
    public class MxDbContext: DbContext, IMxDbContext
    {
        public DbSet<OutboundMaterial> OutboundMaterials { get; set; } = default!;
        public DbSet<StoragePosition> StoragePositions { get; set; } = default!;

        public DatabaseFacade DataBase => base.Database;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            string connectionString = config.GetConnectionString("MoXin");
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OutboundMaterial>().HasNoKey();
            modelBuilder.Entity<StoragePosition>().HasNoKey();
            base.OnModelCreating(modelBuilder);
        }
    }
}
