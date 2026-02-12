using FGMS.Mx.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FGMS.Mx.Core
{
    public interface IMxDbContext
    {
        EntityEntry<T> Entry<T>(T entity) where T : class;
        DbSet<T> Set<T>() where T : class;
        DbSet<OutboundMaterial> OutboundMaterials { get; set; }
        DbSet<StoragePosition> StoragePositions { get; set; }
        DbSet<WorkReport> WorkReports { get; set; }
        DatabaseFacade DataBase { get; }
    }
}
