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
        //// 添加线程标识，用于调试和检测
        //private readonly int managedThreadId;
        //private readonly Guid instanceId = Guid.NewGuid();

        //// 使用 AsyncLocal 跟踪事务状态（线程安全）
        //private static readonly AsyncLocal<IDbContextTransaction?> currentTransaction = new();

        //public FgmsDbContext()
        //{
        //    managedThreadId = Environment.CurrentManagedThreadId;
        //}

        #region DbSets

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
        public DbSet<ProductionOrderLog> ProductionOrderLogs { get; set; } = default!;
        public DbSet<MaterialIssueOrder> MaterialIssueOrders { get; set; } = default!;
        public DbSet<WorkOrderStandard> WorkOrderStandards { get; set; } = default!;
        public DbSet<MaterialDiameter> MaterialDiameters { get; set; } = default!;
        public DbSet<MenuInfo> MenuInfos { get; set; } = default!;
        public DbSet<PermissionInfo> PermissionInfos { get; set; } = default!;

        #endregion

        //#region IUnitOfWork Implementation

        ///// <summary>
        ///// 同步提交更改（支持取消令牌）
        ///// </summary>
        //public int SaveChanges(CancellationToken cancellationToken = default)
        //{
        //    CheckThreadSafety();
        //    cancellationToken.ThrowIfCancellationRequested();
        //    return base.SaveChanges();
        //}

        ///// <summary>
        ///// 异步提交更改（支持取消令牌）
        ///// </summary>
        //public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        //{
        //    CheckThreadSafety();
        //    return await base.SaveChangesAsync(cancellationToken);
        //}

        ///// <summary>
        ///// 开始事务（支持取消令牌）
        ///// </summary>
        //public async Task BeginTrans(CancellationToken cancellationToken = default)
        //{
        //    CheckThreadSafety();
        //    cancellationToken.ThrowIfCancellationRequested();

        //    if (Database.CurrentTransaction != null)
        //        throw new InvalidOperationException($"DbContext实例 {instanceId} 已经有一个活动事务");

        //    var transaction = await Database.BeginTransactionAsync(cancellationToken);
        //    currentTransaction.Value = transaction;
        //}

        ///// <summary>
        ///// 提交事务（支持取消令牌）
        ///// </summary>
        //public async Task CommitTrans(CancellationToken cancellationToken = default)
        //{
        //    CheckThreadSafety();
        //    cancellationToken.ThrowIfCancellationRequested();
        //    var transaction = (Database.CurrentTransaction ?? currentTransaction.Value) ?? throw new InvalidOperationException("没有活动事务可提交");
        //    try
        //    {
        //        await transaction.CommitAsync(cancellationToken);
        //    }
        //    finally
        //    {
        //        await transaction.DisposeAsync();
        //        currentTransaction.Value = null;
        //    }
        //}

        ///// <summary>
        ///// 回滚事务（支持取消令牌）
        ///// </summary>
        //public async Task RollBackTrans(CancellationToken cancellationToken = default)
        //{
        //    CheckThreadSafety();
        //    cancellationToken.ThrowIfCancellationRequested();

        //    var transaction = Database.CurrentTransaction ?? currentTransaction.Value;
        //    if (transaction == null)
        //    {
        //        return;
        //    }

        //    try
        //    {
        //        await transaction.RollbackAsync(cancellationToken);
        //    }
        //    finally
        //    {
        //        await transaction.DisposeAsync();
        //        currentTransaction.Value = null;
        //    }
        //}

        //// 检查是否在错误的线程中使用
        //private void CheckThreadSafety()
        //{
        //    var currentThreadId = Environment.CurrentManagedThreadId;
        //    if (managedThreadId != currentThreadId)
        //    {
        //        throw new InvalidOperationException($"DbContext实例 {instanceId} 在创建线程 {managedThreadId} 上创建，" + $"但在线程 {currentThreadId} 上使用。DbContext 不是线程安全的。");
        //    }
        //}
        //#endregion

        #region 兼容旧代码的重载方法

        public async Task<int> SaveChangesAsync() => await base.SaveChangesAsync();
        public async Task BeginTrans() => await base.Database.BeginTransactionAsync();
        public async Task CommitTrans() => await base.Database.CommitTransactionAsync();
        public async Task RollBackTrans() => await base.Database.RollbackTransactionAsync();

        #endregion

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
