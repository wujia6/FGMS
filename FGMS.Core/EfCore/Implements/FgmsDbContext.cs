using System.Runtime.Loader;
using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;

namespace FGMS.Core.EfCore.Implements
{
    public class FgmsDbContext : DbContext, IFgmsDbContext
    {
        public DbSet<Brand> Brands { get; set; } = default!;
        public DbSet<Equipment> Equipments { get; set; } = default!;
        public DbSet<Organize> Organizes { get; set; } = default!;
        public DbSet<RoleInfo> RoleInfos { get; set; } = default!;
        public DbSet<UserInfo> UserInfos { get; set; } = default!;
        public DbSet<WorkOrder> WorkOrders { get; set; } = default!;
        public DbSet<Element> Elements { get; set; } = default!;
        public DbSet<ElementEntity> ElementEntities { get; set; } = default!;
        public DbSet<Component> Components { get; set; } = default!;
        public DbSet<Standard> Standards { get; set; } = default!;
        public DbSet<TrackLog> TrackLogs { get; set; } = default!;
        public DbSet<ComponentLog> ComponentLogs { get; set; } = default!;
        public DbSet<CargoSpace> CargoSpaces { get; set; } = default!;
        public DbSet<AgvTaskSync> AgvTaskSyncs { get; set; } = default!;
        public DbSet<ProductionOrder> ProductionOrders { get; set; } = default!;
        public DbSet<MaterialIssueOrder> MaterialIssueOrders { get; set; } = default!;
        public DbSet<EquipmentChangeOrder> EquipmentChangeOrders { get; set; } = default!;
        public DbSet<WorkOrderStandard> WorkOrderStandards { get; set; } = default!;
        public DbSet<MaterialDiameter> MaterialDiameters { get; set; } = default!;
        public DbSet<MenuInfo> MenuInfos { get; set; } = default!;
        public DbSet<PermissionInfo> PermissionInfos { get; set; } = default!;

        public async Task<int> SaveChangesAsync() => await base.SaveChangesAsync();
        public async Task BeginTrans() => await base.Database.BeginTransactionAsync();
        public async Task CommitTrans() => await base.Database.CommitTransactionAsync();
        public async Task RollBackTrans() => await base.Database.RollbackTransactionAsync();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            optionsBuilder.UseSqlServer(config.GetConnectionString("FGMS"));
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(AppContext.BaseDirectory + $"FGMS.Models.dll");
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);
        }
    }
}
