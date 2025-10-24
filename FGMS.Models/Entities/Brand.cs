using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FGMS.Models.Entities
{
    public class Brand
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public IEnumerable<Element>? Elements { get; set; } 
    }

    public class BrandConfig : IEntityTypeConfiguration<Brand>
    {
        public void Configure(EntityTypeBuilder<Brand> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(30);
            builder.Property(x => x.Description).HasMaxLength(50);
        }
    }
}
