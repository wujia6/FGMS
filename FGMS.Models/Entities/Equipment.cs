using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FGMS.Models.Entities
{
    public class Equipment
    {
        public int Id { get; set; }
        public int OrganizeId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public bool Mount { get; set; }
        public bool PoMount { get; set; }
        public virtual Organize? Organize { get; set; }
        public virtual IEnumerable<ProductionOrder>? ProductionOrders { get; set; }
    }

    public class EquipmentConfig : IEntityTypeConfiguration<Equipment>
    {
        public void Configure(EntityTypeBuilder<Equipment> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.OrganizeId).IsRequired();
            builder.Property(e => e.Code).IsRequired().HasMaxLength(20);
            builder.Property(e => e.Name).IsRequired().HasMaxLength(50);
            builder.Property(e => e.Enabled).HasDefaultValue(true);
            builder.Property(e => e.Mount).HasDefaultValue(false);
            builder.Property(e => e.PoMount).HasDefaultValue(false);
            builder.HasOne(e => e.Organize).WithMany(e => e.Equipments).HasForeignKey(e => e.OrganizeId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
