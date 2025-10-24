using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using FGMS.Models.Entities;

namespace FGMS.Core.EfCore.Interfaces
{
    public interface IFgmsDbContext : IUnitOfWork
    {
        EntityEntry<T> Entry<T>(T entity) where T : class;
        DbSet<T> Set<T>() where T : class;
        DbSet<Brand> Brands { get; set; }
        DbSet<Equipment> Equipments { get; set; }
        DbSet<Organize> Organizes { get; set; }
        DbSet<RoleInfo> RoleInfos { get; set; }
        DbSet<UserInfo> UserInfos { get; set; }
        DbSet<WorkOrder> WorkOrders { get; set; }
        DbSet<Element> Elements { get; set; }
        DbSet<ElementEntity> ElementEntities { get; set; }
        DbSet<Standard> Standards { get; set; }
        DbSet<Component> Components { get; set; }
        DbSet<CargoSpace> CargoSpaces { get; set; }
        DbSet<TrackLog> TrackLogs { get; set; }
        DbSet<ComponentLog> ComponentLogs { get; set; }
        DbSet<AgvTaskSync> AgvTaskSyncs { get; set; }
    }
}
