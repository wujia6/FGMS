using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FGMS.Models.Entities
{
    public class WorkOrderStandard
    {
        public int Id { get; set; }
        public int WorkOrderId { get; set; }
        public int StandardId { get; set; }
        public virtual WorkOrder? WorkOrder { get; set; }
        public virtual Standard? Standard { get; set; }
    }

    public class WorkOrderStandardConfig : IEntityTypeConfiguration<WorkOrderStandard>
    {
        public void Configure(EntityTypeBuilder<WorkOrderStandard> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.WorkOrderId).IsRequired();
            builder.Property(x => x.StandardId).IsRequired();
            builder.HasOne(x => x.WorkOrder).WithMany(x => x.WorkOrderStandards).HasForeignKey(x => x.WorkOrderId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Standard).WithMany(x => x.WorkOrderStandards).HasForeignKey(x => x.StandardId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
