using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FGMS.Models.Entities
{
    public class ComponentLog
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string OrderNo { get; set; }
        public string MaterialNo { get; set; }
        public string MaterialSpec { get; set; }
        public string EquipmentCode { get; set; }
        public DateTime RequiredDate { get; set; }
        public string? UpperJson { get; set; }
        public string? DownJson { get; set; }
    }

    public class COmponentLogConfig : IEntityTypeConfiguration<ComponentLog>
    {
        public void Configure(EntityTypeBuilder<ComponentLog> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Code).IsRequired().HasMaxLength(20);
            builder.Property(x => x.OrderNo).IsRequired().HasMaxLength(20);
            builder.Property(x => x.MaterialNo).IsRequired().HasMaxLength(20);
            builder.Property(x => x.MaterialSpec).IsRequired().HasMaxLength(100);
            builder.Property(x => x.EquipmentCode).IsRequired().HasMaxLength(10);
            builder.Property(x => x.RequiredDate).IsRequired();
            builder.Property(x => x.UpperJson).HasMaxLength(5000);
            builder.Property(x => x.DownJson).HasMaxLength(5000);
        }
    }
}
