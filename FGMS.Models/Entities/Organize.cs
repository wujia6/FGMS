using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FGMS.Models.Entities
{
    public class Organize
    {
        public int Id { get; set; }
        public int? Pid { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Position { get; set; }
        public virtual Organize? Parent { get; set; }
        public virtual IEnumerable<Organize>? Childrens { get; set; }
        public virtual IEnumerable<RoleInfo>? RoleInfos { get; set; }
        public virtual IEnumerable<Equipment>? Equipments { get; set; }
        public virtual IEnumerable<CargoSpace>? CargoSpaces { get; set; }
    }

    public class OrganizeConfig : IEntityTypeConfiguration<Organize>
    {
        public void Configure(EntityTypeBuilder<Organize> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Pid);
            builder.Property(x => x.Code).IsRequired().HasMaxLength(20);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Position).HasMaxLength(50);
            builder.HasOne(x => x.Parent).WithMany(x => x.Childrens).HasForeignKey(x => x.Pid).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
