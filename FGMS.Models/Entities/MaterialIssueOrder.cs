using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FGMS.Models.Entities
{
    public class MaterialIssueOrder
    {
        public int Id { get; set; }
        public int ProductionOrderId { get; set; }
        public int CreateorId { get; set; }
        public int? SendorId { get; set; }
        public MioType Type { get; set; }
        public string OrderNo { get; set; }
        public string MaterialNo { get; set; }
        public string MaterialName { get; set; }
        public string MaterialSpce { get; set; }
        public int Quantity { get; set; }
        public DateTime? IssueTime { get; set; }
        public MioStatus? Status { get; set; }
        public string? MxWareHouse { get; set; }
        public string? MxCargoSpace { get; set; }
        public string? MxBarCode { get; set; }
        public string? MxOutStoreOrderNo { get; set; }

        public virtual ProductionOrder? ProductionOrder { get; set; }
        public virtual UserInfo? Createor { get; set; }
        public virtual UserInfo? Sendor { get; set; }
    }

    public class MaterialIssueOrderConfig: IEntityTypeConfiguration<MaterialIssueOrder>
    {
        public void Configure(EntityTypeBuilder<MaterialIssueOrder> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ProductionOrderId).IsRequired();
            builder.Property(x => x.CreateorId).IsRequired();
            builder.Property(x => x.SendorId);
            builder.Property(x => x.Type).IsRequired();
            builder.Property(x => x.OrderNo).IsRequired().HasMaxLength(20);
            builder.Property(x => x.MaterialNo).IsRequired().HasMaxLength(50);
            builder.Property(x => x.MaterialName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.MaterialSpce).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Quantity).IsRequired();
            builder.Property(x => x.IssueTime).HasDefaultValueSql("getdate()");
            builder.Property(x => x.Status).HasDefaultValue(MioStatus.待备料);
            builder.Property(x => x.MxWareHouse).HasMaxLength(200);
            builder.Property(x => x.MxCargoSpace).HasMaxLength(200);
            builder.Property(x => x.MxBarCode).HasMaxLength(500);
            builder.HasOne(x => x.ProductionOrder).WithMany(x => x.MaterialIssueOrders).HasForeignKey(x => x.ProductionOrderId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Createor).WithMany().HasForeignKey(x => x.CreateorId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Sendor).WithMany().HasForeignKey(x => x.SendorId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
