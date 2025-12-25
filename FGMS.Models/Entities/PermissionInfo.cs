using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FGMS.Models.Entities
{
    public class PermissionInfo
    {
        public int Id { get; set; }
        public int RoleInfoId { get; set; }
        public int MenuInfoId { get; set; }
        public bool CanView { get; set; }
        public bool CanManagement { get; set; }
        public bool CanAudits { get; set; }
        public virtual RoleInfo? RoleInfo { get; set; }
        public virtual MenuInfo? MenuInfo { get; set; }
    }

    public class RoleMenuPermissionConfig : IEntityTypeConfiguration<PermissionInfo>
    {
        public void Configure(EntityTypeBuilder<PermissionInfo> builder)
        {
            builder.HasKey(rmp => rmp.Id);
            builder.Property(rmp => rmp.RoleInfoId).IsRequired();
            builder.Property(rmp => rmp.MenuInfoId).IsRequired();
            builder.Property(rmp => rmp.CanView).HasDefaultValue(false);
            builder.Property(rmp => rmp.CanManagement).HasDefaultValue(false);
            builder.Property(rmp => rmp.CanAudits).HasDefaultValue(false);
            builder.HasOne(rmp => rmp.RoleInfo).WithMany(ri => ri.PermissionInfos).HasForeignKey(rmp => rmp.RoleInfoId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(rmp => rmp.MenuInfo).WithMany(m => m.PermissionInfos).HasForeignKey(rmp => rmp.MenuInfoId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
