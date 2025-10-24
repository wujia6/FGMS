using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FGMS.Models.Entities
{
    public class Standard
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public int? MainElementId { get; set; }
        public int Total { get; set; }
        public string? FirstRangle { get; set; }
        public int? FirstElementId { get; set; }
        public string? SecondRangle { get; set; }
        public int? SecondElementId { get; set; }
        public string? ThirdRangle { get; set; }
        public int? ThirdElementId { get; set; }
        public string? FourthRangle { get; set; }
        public int? FourthElementId { get; set; }
        public string? FifthRangle { get; set; }
        public int? FifthElementId { get; set; }
        public virtual Element? MainElement { get; set; }
        public virtual Element? FirstElement { get; set; }
        public virtual Element? SecondElement { get; set; }
        public virtual Element? ThirdElement { get; set; }
        public virtual Element? FourthElement { get; set; }
        public virtual Element? FifthElement { get; set; }
        public virtual IEnumerable<WorkOrderStandard>? WorkOrderStandards { get; set; }
        public virtual IEnumerable<Component>? Components { get; set; }
    }

    public class StandardConfig : IEntityTypeConfiguration<Standard>
    {
        public void Configure(EntityTypeBuilder<Standard> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Code).IsRequired().HasMaxLength(30);
            builder.Property(x => x.MainElementId);
            builder.Property(x => x.Total).IsRequired();
            builder.Property(x => x.FirstRangle).HasMaxLength(20);
            builder.Property(x => x.FirstElementId);
            builder.Property(x => x.SecondRangle).HasMaxLength(20);
            builder.Property(x => x.SecondElementId);
            builder.Property(x => x.ThirdRangle).HasMaxLength(20);
            builder.Property(x => x.ThirdElementId);
            builder.Property(x => x.FourthRangle).HasMaxLength(20);
            builder.Property(x => x.FourthElementId);
            builder.Property(x => x.FifthRangle).HasMaxLength(20);
            builder.Property(x => x.FifthElementId);
            builder.HasOne(x => x.MainElement).WithMany(x => x.MainStandards).HasForeignKey(x => x.MainElementId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.FirstElement).WithMany(x => x.FirstStandards).HasForeignKey(x => x.FirstElementId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.SecondElement).WithMany(x => x.SecondStandards).HasForeignKey(x => x.SecondElementId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ThirdElement).WithMany(x => x.ThirdStandards).HasForeignKey(x => x.ThirdElementId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.FourthElement).WithMany(x => x.FourthStandards).HasForeignKey(x => x.FourthElementId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.FifthElement).WithMany(x => x.FifthStandards).HasForeignKey(x => x.FifthElementId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
