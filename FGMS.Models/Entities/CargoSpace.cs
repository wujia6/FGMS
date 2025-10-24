using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FGMS.Models.Entities
{
    public class CargoSpace
    {
        public int Id { get; set; }
        public int? OrganizeId { get; set; }
        public int? ParentId { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Remark { get; set; }
        public bool IsStandard { get; set; }
        [NotMapped]
        public int Quantity
        {
            get
            {
                if (this.Components != null && this.ElementEntities == null)
                    return this.Components.Count();
                else if (this.Components == null && this.ElementEntities != null)
                    return this.ElementEntities.Count();
                else
                    return 0;
            }
        }
        public virtual Organize? Organize { get; set; }
        public virtual CargoSpace? Parent { get; set; }
        public virtual IEnumerable<CargoSpace>? Childrens { get; set; }
        public virtual IEnumerable<ElementEntity>? ElementEntities { get; set; }
        public virtual IEnumerable<Component>? Components { get; set; }
    }

    public class CargoSpaceConfig : IEntityTypeConfiguration<CargoSpace>
    {
        public void Configure(EntityTypeBuilder<CargoSpace> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.OrganizeId);
            builder.Property(x => x.ParentId);
            builder.Property(x => x.Name).HasMaxLength(30);
            builder.Property(x => x.Code).HasMaxLength(20);
            builder.Property(x => x.Remark).HasMaxLength(100);
            builder.Property(x => x.IsStandard).HasDefaultValue(false);
            builder.HasOne(x => x.Organize).WithMany(x => x.CargoSpaces).HasForeignKey(x => x.OrganizeId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Parent).WithMany(x => x.Childrens).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
