using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FGMS.Models.Entities
{
    public class ElementEntity
    {
        public int Id { get; set; }
        public int ElementId { get; set; }
        public int? ComponentId { get; set; }
        public int? CargoSpaceId { get; set; }
        public int? CargoSpaceHistory { get; set; }
        public string MaterialNo { get; set; }
        public string? Code { get; set; }
        public string? BigDiameter { get; set; }
        public string? SmallDiameter { get; set; }
        public string? InnerDiameter { get; set; } //内直径（成组后有值）
        public string? OuterDiameter { get; set; } //外直径（成组后有值）
        public string? AxialRunout { get; set; }   //轴向跳动（成组后有值）
        public string? RadialRunout { get; set; }  //径向跳动（成组后有值）
        public string? Width { get; set; }
        public string? SmallRangle { get; set; }
        public string? PlaneWidth { get; set; }    //平位宽
        public string? BigRangle { get; set; }
        public string? CurrentAngle { get; set; }
        public string? QrCodeImage { get; set; }
        public ElementEntityStatus Status { get; set; }
        public bool IsGroup { get; set; }
        public DateTime? BeginTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public float UseDuration { get; set; }
        public string? Remark { get; set; }
        public string? Position { get; set; }
        public DiscardReason? DiscardBy { get; set; }
        public DateTime? DiscardTime { get; set; }
        public virtual Element? Element { get; set; }
        public virtual Component? Component { get; set; }
        public virtual CargoSpace? CargoSpace { get; set; }
    }

    public class ElementEntityConfig : IEntityTypeConfiguration<ElementEntity>
    {
        public void Configure(EntityTypeBuilder<ElementEntity> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd().HasAnnotation("SqlServer:Identity", "1000, 1");
            builder.Property(x => x.ElementId).IsRequired();
            builder.Property(x => x.ComponentId);
            builder.Property(x => x.CargoSpaceId);
            builder.Property(x => x.CargoSpaceHistory);
            builder.Property(x => x.MaterialNo).IsRequired();
            builder.Property(x => x.Code).HasMaxLength(100);
            builder.Property(x => x.BigDiameter).HasMaxLength(20);
            builder.Property(x => x.SmallDiameter).HasMaxLength(20);
            builder.Property(x => x.InnerDiameter).HasMaxLength(20);
            builder.Property(x => x.OuterDiameter).HasMaxLength(20);
            builder.Property(x => x.AxialRunout).HasMaxLength(20);
            builder.Property(x => x.RadialRunout).HasMaxLength(20);
            builder.Property(x => x.Width).HasMaxLength(20);
            builder.Property(x => x.SmallRangle).HasMaxLength(20);
            builder.Property(x => x.PlaneWidth).HasMaxLength(20);
            builder.Property(x => x.BigRangle).HasMaxLength(20);
            builder.Property(x => x.CurrentAngle).HasMaxLength(20);
            builder.Property(x => x.QrCodeImage).HasMaxLength(100);
            builder.Property(x => x.Status).HasDefaultValue(ElementEntityStatus.待入库);
            builder.Property(x => x.IsGroup).IsRequired().HasDefaultValue(false);
            builder.Property(x => x.BeginTime);
            builder.Property(x => x.FinishTime);
            builder.Property(x => x.UseDuration).HasDefaultValue(0);
            builder.Property(x => x.Remark).HasMaxLength(100);
            builder.Property(x => x.Position).HasMaxLength(10);
            builder.Property(x => x.DiscardBy);
            builder.Property(x => x.DiscardTime);
            builder.HasOne(x => x.Element).WithMany(x => x.ElementEntities).HasForeignKey(x => x.ElementId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Component).WithMany(x => x.ElementEntities).HasForeignKey(x => x.ComponentId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CargoSpace).WithMany(x => x.ElementEntities).HasForeignKey(x => x.CargoSpaceId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
