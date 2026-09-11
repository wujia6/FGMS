using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FGMS.Models.Entities
{
    /// <summary>
    /// 加工标准实体类
    /// </summary>
    public class ProcessingStandard
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string ToolName { get; set; }
        public string MaterialNumber { get; set; }
        public string ToolSpecification { get; set; }
        public string DrawingType { get; set; }
        public string ToolType { get; set; }
        public string MachineModel { get; set; }
        public int BomProcessorId { get; set; }
        public DateTime? BomProcessDate { get; set; }
        public int? HandlerId { get; set; }
        public GeneralStatus? ProgramStatus { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string? Process { get; set; }
        public DateTime? PlannedDemandTime { get; set; }
        public double? WorkingHours { get; set; }
        public string? StandardGrindingWheelSet { get; set; }
        public string? CustomerDrawingNumber { get; set; }
        public int? Month { get; set; }

        public virtual UserInfo? BomProcessor { get; set; }
        public virtual UserInfo? Handler { get; set; }
    }

    public class ProcessingStandardConfig : IEntityTypeConfiguration<ProcessingStandard>
    {
        public void Configure(EntityTypeBuilder<ProcessingStandard> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.OrderDate).IsRequired();
            builder.Property(x => x.ToolName).IsRequired().HasMaxLength(200);
            builder.Property(x => x.MaterialNumber).IsRequired().HasMaxLength(100);
            builder.Property(x => x.ToolSpecification).IsRequired().HasMaxLength(200);
            builder.Property(x => x.DrawingType).IsRequired().HasMaxLength(200);
            builder.Property(x => x.ToolType).IsRequired().HasMaxLength(200);
            builder.Property(x => x.MachineModel).IsRequired().HasMaxLength(200);
            builder.Property(x => x.BomProcessorId);
            builder.Property(x => x.BomProcessDate);
            builder.Property(x => x.HandlerId).IsRequired(false);
            builder.Property(x => x.ProgramStatus).HasDefaultValue(GeneralStatus.未完成);
            builder.Property(x => x.CompletionDate);
            builder.Property(x => x.Process).HasMaxLength(200);
            builder.Property(x => x.PlannedDemandTime);
            builder.Property(x => x.WorkingHours);
            builder.Property(x => x.StandardGrindingWheelSet).HasMaxLength(200);
            builder.Property(x => x.CustomerDrawingNumber).HasMaxLength(200);
            builder.Property(x => x.Month);
            builder.HasOne(x => x.BomProcessor).WithMany().HasForeignKey(x => x.BomProcessorId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Handler).WithMany().HasForeignKey(x => x.HandlerId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        }
    }
}