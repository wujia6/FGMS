using MiniExcelLibs.Attributes;

namespace FGMS.Models.Dtos
{
    /// <summary>
    /// 加工标准实体类
    /// </summary>
    public class ProcessingStandardDto
    {
        public int Id { get; set; }
        public int BomProcessorId { get; set; }
        public int HandlerId { get; set; }

        [ExcelColumn(Name = "接单日期")]
        public DateTime OrderDate { get; set; }

        [ExcelColumn(Name = "刀具品名")]
        public string ToolName { get; set; }

        [ExcelColumn(Name = "料号")]
        public string MaterialNumber { get; set; }

        [ExcelColumn(Name = "刀具规格")]
        public string ToolSpecification { get; set; }

        [ExcelColumn(Name = "图纸类型")]
        public string DrawingType { get; set; }

        [ExcelColumn(Name = "刀具类型")]
        public string ToolType { get; set; }

        [ExcelColumn(Name = "机型")]
        public string MachineModel { get; set; }

        [ExcelColumn(Name = "BOM工艺处理人")]
        public string BomProcessorName { get; set; }

        [ExcelColumn(Name = "BOM处理日期")]
        public DateTime? BomProcessDate { get; set; }

        [ExcelColumn(Name = "处理人")]
        public string HandlerName { get; set; }

        [ExcelColumn(Name = "程序处理情况")]
        public string? ProgramStatus { get; set; }

        [ExcelColumn(Name = "完成日期")]
        public DateTime? CompletionDate { get; set; }

        [ExcelColumn(Name = "工艺")]
        public string? Process { get; set; }

        [ExcelColumn(Name = "计划需求时间")]
        public DateTime? PlannedDemandTime { get; set; }

        [ExcelColumn(Name = "工时")]
        public double? WorkingHours { get; set; }

        [ExcelColumn(Name = "标准砂轮组名称")]
        public string? StandardGrindingWheelSet { get; set; }

        [ExcelColumn(Name = "客户图纸号")]
        public string? CustomerDrawingNumber { get; set; }

        [ExcelColumn(Name = "月份")]
        public int? Month { get; set; }
    }
}
