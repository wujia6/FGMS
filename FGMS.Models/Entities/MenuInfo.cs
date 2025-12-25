using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FGMS.Models.Entities
{
    public class MenuInfo
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public ClientType Client { get; set; }
        public string Name { get; set; }
        public string Code { get; set; } // 菜单代码，用于权限验证
        public string? Path { get; set; } // 前端路由路径
        public string? Icon { get; set; }
        public bool IsVisible { get; set; }
        public virtual MenuInfo? Parent { get; set; }
        public virtual ICollection<MenuInfo>? Childrens { get; set; }
        public virtual ICollection<PermissionInfo>? PermissionInfos { get; set; }
    }

    public class MenuConfig:IEntityTypeConfiguration<MenuInfo>
    {
        public void Configure(EntityTypeBuilder<MenuInfo> builder)
        {
            builder.HasKey(m => m.Id);
            builder.Property(m => m.ParentId);
            builder.Property(m => m.Client).IsRequired();
            builder.Property(m => m.Name).IsRequired().HasMaxLength(100);
            builder.Property(m => m.Code).IsRequired().HasMaxLength(50);
            builder.Property(m => m.Path).HasMaxLength(200);
            builder.Property(m => m.Icon).HasMaxLength(100);
            builder.Property(m => m.IsVisible).HasDefaultValue(true);
            builder.HasOne(m => m.Parent).WithMany(m => m.Childrens).HasForeignKey(m => m.ParentId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
