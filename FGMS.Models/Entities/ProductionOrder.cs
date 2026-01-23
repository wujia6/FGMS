using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FGMS.Models.Entities
{
    public class ProductionOrder
    {
        public int Id { get; set; }
        public int UserInfoId { get; set; }
        public int EquipmentId { get; set; }
        public int? WorkOrderId { get; set; }
        public string OrderNo { get; set; }
        public string FinishCode { get; set; }
        public string FinishName { get; set; }
        public string FinishSpec { get; set; }
        public string MaterialCode { get; set; }
        public string MaterialName { get; set; }
        public string MaterialSpec { get; set; }
        public int Quantity { get; set; }
        public ProductionOrderStatus Status { get; set; }
        public DateTime? CreateTime { get; set; }
        public string? Remark { get; set; }
        public double? WorkHours { get; set; }
        public DateTime? PlannedBeginTime { get; set; }
        public bool? IsDc { get; set; }

        public virtual UserInfo? UserInfo { get; set; }
        public virtual Equipment? Equipment { get; set; }
        public virtual WorkOrder? WorkOrder { get; set; }
        public virtual IEnumerable<MaterialIssueOrder>? MaterialIssueOrders { get; set; }
    }

    public class ProductionOrderConfig : IEntityTypeConfiguration<ProductionOrder>
    {
        public void Configure(EntityTypeBuilder<ProductionOrder> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.UserInfoId).IsRequired();
            builder.Property(x => x.EquipmentId).IsRequired();
            builder.Property(x => x.WorkOrderId);
            builder.Property(x => x.OrderNo).IsRequired().HasMaxLength(20);
            builder.Property(x => x.FinishCode).IsRequired().HasMaxLength(50);
            builder.Property(x => x.FinishName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.FinishSpec).IsRequired().HasMaxLength(100);
            builder.Property(x => x.MaterialCode).IsRequired().HasMaxLength(50);
            builder.Property(x => x.MaterialName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.MaterialSpec).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Quantity).IsRequired();
            builder.Property(x => x.Status).IsRequired().HasDefaultValue(ProductionOrderStatus.已排配);
            builder.Property(x => x.CreateTime).HasDefaultValueSql("getdate()");
            builder.Property(x => x.Remark).HasMaxLength(500);
            builder.Property(x => x.WorkHours).HasPrecision(6, 2);
            builder.Property(x => x.PlannedBeginTime);
            builder.Property(x => x.IsDc).HasDefaultValue(false);
            builder.HasOne(x => x.WorkOrder).WithOne(x => x.ProductionOrder).HasForeignKey<ProductionOrder>(x => x.WorkOrderId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Equipment).WithMany(x => x.ProductionOrders).HasForeignKey(x => x.EquipmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.UserInfo).WithMany().HasForeignKey(x => x.UserInfoId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
