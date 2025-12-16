using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FGMS.Models.Entities
{
    public class EquipmentChangeOrder
    {
        public int Id { get; set; }
        public int UserInfoId { get; set; }
        public int ProductionOrderId { get; set; }
        public int EquipmentId { get; set; }
        public string OldEquipmentCode { get; set; }
        public string Reason { get; set; }
        public WorkOrderStatus? Status { get; set; }
        public DateTime? ChangeDate { get; set; }

        public virtual UserInfo? UserInfo { get; set; }
        public virtual ProductionOrder? ProductionOrder { get; set; }
        public virtual Equipment? Equipment { get; set; }
    }

    public class EquipmentChangeOrderConfig : IEntityTypeConfiguration<EquipmentChangeOrder>
    {
        public void Configure(EntityTypeBuilder<EquipmentChangeOrder> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.UserInfoId).IsRequired();
            builder.Property(x => x.ProductionOrderId).IsRequired();
            builder.Property(x => x.EquipmentId).IsRequired();
            builder.Property(x => x.OldEquipmentCode).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Reason).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Status).HasDefaultValue(WorkOrderStatus.待审);
            builder.Property(x => x.ChangeDate).HasDefaultValueSql("getdate()");
            builder.HasOne(x => x.UserInfo).WithMany().HasForeignKey(x => x.UserInfoId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ProductionOrder).WithMany().HasForeignKey(x => x.ProductionOrderId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Equipment).WithMany().HasForeignKey(x => x.EquipmentId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
