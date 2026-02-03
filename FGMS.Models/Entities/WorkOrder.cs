using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FGMS.Models.Entities
{
    public class WorkOrder
    {
        public int Id { get; set; }
        public int? Pid { get; set; }
        public int? ProductionOrderId { get; set; }
        public int UserInfoId { get; set; }
        public string OrderNo { get; set; }
        public WorkOrderType Type { get; set; }
        public WorkOrderPriority Priority { get; set; }
        public string MaterialNo { get; set; }
        public string MaterialSpec { get; set; }
        public WorkOrderStatus Status { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? RequiredDate { get; set; }
        public string? Remark { get; set; }
        public string AgvTaskCode { get; set; }
        public string AgvStatus { get; set; }
        public int? RenovateorId { get; set; }
        public string? RepairEquipmentCode { get; set; }

        public virtual ProductionOrder? ProductionOrder { get; set; }
        public virtual UserInfo? UserInfo { get; set; }
        public virtual WorkOrder? Parent { get; set; }
        public virtual UserInfo? Renovateor { get; set; }
        public virtual IEnumerable<WorkOrderStandard>? WorkOrderStandards { get; set; }
        public virtual IEnumerable<Component>? Components { get; set; }
        public virtual IEnumerable<WorkOrder>? Childrens { get; set; }
    }

    public class WorkOrderConfig : IEntityTypeConfiguration<WorkOrder>
    {
        public void Configure(EntityTypeBuilder<WorkOrder> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ProductionOrderId);
            builder.Property(x => x.UserInfoId).IsRequired();
            builder.Property(x => x.OrderNo).IsRequired().HasMaxLength(20);
            builder.Property(x => x.Type).IsRequired();
            builder.Property(x => x.Priority).IsRequired();
            builder.Property(x => x.MaterialNo).IsRequired().HasMaxLength(20);
            builder.Property(x => x.MaterialSpec).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Status).IsRequired().HasDefaultValue(WorkOrderStatus.待审);
            builder.Property(x => x.CreateDate).HasDefaultValueSql("getdate()");
            builder.Property(x => x.RequiredDate);
            builder.Property(x => x.Remark).HasMaxLength(200);
            builder.Property(x => x.AgvTaskCode).IsRequired().HasMaxLength(20);
            builder.Property(x => x.AgvStatus).IsRequired().HasMaxLength(20).HasDefaultValue("execute");
            builder.Property(x => x.RenovateorId);
            builder.Property(x => x.RepairEquipmentCode).HasMaxLength(10);
            builder.HasOne(x => x.Parent).WithMany(x => x.Childrens).HasForeignKey(x => x.Pid).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ProductionOrder).WithOne(x => x.WorkOrder).HasForeignKey<WorkOrder>(x => x.ProductionOrderId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.UserInfo).WithMany(x => x.WorkOrders).HasForeignKey(x => x.UserInfoId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Renovateor).WithMany().HasForeignKey(x => x.RenovateorId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);// Renovateor不建立反向导航属性
        }
    }
}
