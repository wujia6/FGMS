using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FGMS.Models.Entities
{
    public class AgvTaskSync
    {
        public int Id { get; set; }
        public string AgvCode { get; set; }
        public string TaskCode { get; set; }
        public string WorkOrderNo { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
    }

    public class AgvTaskSyncConfig : IEntityTypeConfiguration<AgvTaskSync>
    {
        public void Configure(EntityTypeBuilder<AgvTaskSync> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.AgvCode).IsRequired().HasMaxLength(20);
            builder.Property(x => x.TaskCode).IsRequired().HasMaxLength(64);
            builder.Property(x => x.WorkOrderNo).IsRequired().HasMaxLength(20);
            builder.Property(x => x.Start).IsRequired().HasMaxLength(10);
            builder.Property(x => x.End).IsRequired().HasMaxLength(10);
        }
    }
}
