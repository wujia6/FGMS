using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FGMS.Models.Entities
{
    public class Component
    {
        public int Id { get; set; }
        public int? StandardId { get; set; }
        public int? WorkOrderId { get; set; }
        public int? CargoSpaceId { get; set; }
        public int? CargoSpaceHistory { get; set; }
        public string? Code { get; set; }
        public bool IsStandard { get; set; }
        public ElementEntityStatus Status { get; set; }
        public virtual Standard? Standard { get; set; }
        public virtual WorkOrder? WorkOrder { get; set; }
        public virtual CargoSpace? CargoSpace { get; set; }
        public virtual IEnumerable<ElementEntity>? ElementEntities { get; set; }
    }

    public class ComponentConfig : IEntityTypeConfiguration<Component>
    {
        public void Configure(EntityTypeBuilder<Component> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.StandardId);
            builder.Property(x => x.WorkOrderId);
            builder.Property(x => x.CargoSpaceId);
            builder.Property(x => x.CargoSpaceHistory);
            builder.Property(x => x.Code).HasMaxLength(100);
            builder.Property(x => x.IsStandard).IsRequired().HasDefaultValue(false);
            builder.Property(x => x.Status).HasDefaultValue(ElementEntityStatus.待入库);
            builder.HasOne(x => x.Standard).WithMany(x => x.Components).HasForeignKey(x => x.StandardId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.WorkOrder).WithMany(x => x.Components).HasForeignKey(x => x.WorkOrderId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CargoSpace).WithMany(x => x.Components).HasForeignKey(x => x.CargoSpaceId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
