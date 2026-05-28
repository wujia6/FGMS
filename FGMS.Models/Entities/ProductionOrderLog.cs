using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FGMS.Models.Entities
{
    public class ProductionOrderLog
    {
        public int Id { get; set; }
        public string Operator { get; set; }
        public string Operation { get; set; }
        public DateTime? OperationTime { get; set; }
    }

    public class ProductionOrderLogConfig: IEntityTypeConfiguration<ProductionOrderLog>
    {
        public void Configure(EntityTypeBuilder<ProductionOrderLog> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Operator).IsRequired().HasMaxLength(20);
            builder.Property(x => x.Operation).IsRequired().HasMaxLength(255);
            builder.Property(x => x.OperationTime).HasDefaultValueSql("getdate()");
        }
    }
}
