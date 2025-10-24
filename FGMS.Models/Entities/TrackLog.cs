using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FGMS.Models.Entities
{
    public class TrackLog
    {
        public int Id { get; set; }
        public LogType? Type { get; set; }
        public string Content { get; set; }
        public string? JsonContent { get; set; }
        public DateTime Date { get; set; }
    }

    public class TrackLogConfig : IEntityTypeConfiguration<TrackLog>
    {
        public void Configure(EntityTypeBuilder<TrackLog> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Type);
            builder.Property(x => x.Content).IsRequired().HasMaxLength(200);
            builder.Property(x => x.JsonContent).HasMaxLength(1000);
            builder.Property(x => x.Date).HasDefaultValueSql("getdate()");
        }
    }
}
