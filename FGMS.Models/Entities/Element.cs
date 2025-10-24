using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FGMS.Models.Entities
{
    public class Element
    {
        public int Id { get; set; }
        public int BrandId { get; set; }
        public ElementCategory Category { get; set; }
        public string MaterialNo { get; set; }
        public string ModalNo { get; set; } //型号
        public string Name { get; set; }
        public ElementUnit Unit { get; set; }
        public string Spec { get; set; }
        public string? Diameter { get; set; }  //直径
        public string? SmallRangle { get; set; } //小R角
        public string? PlaneWidth { get; set; }    //平位宽
        public string? BigRangle { get; set; } //大R角
        public string? WheelWidth { get; set; }//砂轮宽
        public string? RingWidth { get; set; } //砂轮环宽
        public string? Thickness { get; set; } //厚度
        public string? Angle { get; set; }     //角度
        public string? InnerBoreDiameter { get; set; } //内孔径
        public string? Granularity { get; set; }   //粒度
        public string? Binders { get; set; }   //结合剂
        public string? Desc { get; set; }
        public string? Lengths { get; set; }
        public virtual Brand? Brand { get; set; }
        public virtual IEnumerable<Standard>? MainStandards { get; set; }
        public virtual IEnumerable<Standard>? FirstStandards { get; set; }
        public virtual IEnumerable<Standard>? SecondStandards { get; set; }
        public virtual IEnumerable<Standard>? ThirdStandards { get; set; }
        public virtual IEnumerable<Standard>? FourthStandards { get; set; }
        public virtual IEnumerable<Standard>? FifthStandards { get; set; }
        public virtual IEnumerable<ElementEntity>? ElementEntities { get; set; }
    }

    public class ElementConfig : IEntityTypeConfiguration<Element>
    {
        public void Configure(EntityTypeBuilder<Element> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.BrandId).IsRequired();
            builder.Property(x => x.Category).IsRequired();
            builder.Property(x => x.MaterialNo).IsRequired();
            builder.Property(x => x.ModalNo).IsRequired();
            builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Unit).IsRequired();
            builder.Property(x => x.Spec).IsRequired().HasMaxLength(100);
            builder.Property(x => x.BigRangle).HasMaxLength(20);
            builder.Property(x => x.PlaneWidth).HasMaxLength(20);
            builder.Property(x => x.SmallRangle).HasMaxLength(20);
            builder.Property(x => x.Diameter).HasMaxLength(20);
            builder.Property(x => x.SmallRangle).HasMaxLength(20);
            builder.Property(x => x.PlaneWidth).HasMaxLength(20);
            builder.Property(x => x.BigRangle).HasMaxLength(20);
            builder.Property(x => x.WheelWidth).HasMaxLength(20);
            builder.Property(x => x.RingWidth).HasMaxLength(20);
            builder.Property(x => x.Thickness).HasMaxLength(20);
            builder.Property(x => x.Angle).HasMaxLength(20);
            builder.Property(x => x.InnerBoreDiameter).HasMaxLength(20);
            builder.Property(x => x.Granularity).HasMaxLength(20);
            builder.Property(x => x.Binders).HasMaxLength(20);
            builder.Property(x => x.Desc).HasMaxLength(100);
            builder.Property(x => x.Lengths).HasMaxLength(10);
            builder.HasOne(x => x.Brand).WithMany(x => x.Elements).HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
