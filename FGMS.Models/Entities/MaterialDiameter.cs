using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FGMS.Models.Entities
{
    public class MaterialDiameter
    {
        public int Id { get; set; }
        public double Diameter { get; set; }
        public double StandardMinutes { get; set; }
    }

    public class MaterialDiameterConfig : IEntityTypeConfiguration<MaterialDiameter>
    {
        public void Configure(EntityTypeBuilder<MaterialDiameter> builder)
        {
            builder.HasKey(md => md.Id);
            builder.Property(md => md.Diameter).HasPrecision(6,3).IsRequired();
            builder.Property(md => md.StandardMinutes).HasPrecision(6, 2).IsRequired();
        }
    }
}
