using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FGMS.Models.Entities
{
    public class UserInfo
    {
        public int Id { get; set; }
        public int RoleInfoId { get; set; }
        public string WorkNo { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string? OperateRange { get; set; }
        public virtual RoleInfo? RoleInfo { get; set; }
        public virtual IEnumerable<WorkOrder>? WorkOrders { get; set; }
    }

    public class UserInfoConfig : IEntityTypeConfiguration<UserInfo>
    {
        public void Configure(EntityTypeBuilder<UserInfo> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.RoleInfoId).IsRequired();
            builder.Property(x => x.WorkNo).IsRequired().HasMaxLength(20);
            builder.Property(x => x.Password).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(30);
            builder.Property(x => x.OperateRange).HasMaxLength(100);
            builder.HasOne(x => x.RoleInfo).WithMany(x => x.UserInfos).HasForeignKey(x => x.RoleInfoId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
