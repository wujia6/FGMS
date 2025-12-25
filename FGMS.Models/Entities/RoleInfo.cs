using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FGMS.Models.Entities
{
    public class RoleInfo
    {
        public int Id { get; set; }
        public int OrganizeId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public virtual Organize? Organize { get; set; }
        public virtual IEnumerable<UserInfo>? UserInfos { get; set; }
        public virtual IEnumerable<PermissionInfo>? PermissionInfos { get; set; }
    }

    public class RoleInfoConfig : IEntityTypeConfiguration<RoleInfo>
    {
        public void Configure(EntityTypeBuilder<RoleInfo> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.OrganizeId).IsRequired();
            builder.Property(x => x.Code).IsRequired().HasMaxLength(20);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(30);
            builder.HasOne(x => x.Organize).WithMany(x => x.RoleInfos).HasForeignKey(x => x.OrganizeId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
