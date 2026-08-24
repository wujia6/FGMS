using MiniExcelLibs.Attributes;

namespace FGMS.Models.Dtos
{
    public class ElementEntityDto
    {
        [ExcelIgnore]
        public int Id { get; set; }

        [ExcelIgnore]
        public int ElementId { get; set; }

        [ExcelColumn(Name = "类型")]
        public string? ElementCategory { get; set; }

        [ExcelIgnore]
        public string? ElementMaterialNo { get; set; }

        [ExcelIgnore]
        public string? ElementName { get; set; }

        [ExcelIgnore]
        public string? ElementSpec { get; set; }

        [ExcelIgnore]
        public int? ComponentId { get; set; }

        [ExcelIgnore]
        public string? ComponentCode { get; set; }

        [ExcelIgnore]
        public string? WorkOrderNo { get; set; }

        [ExcelIgnore]
        public int? CargoSpaceId { get; set; }
        
        [ExcelIgnore]
        public string? CargoSpaceCode { get; set; }

        [ExcelColumn(Name = "货位")]
        public string? CargoSpaceName { get; set; }

        [ExcelIgnore]
        public int? CargoSpaceQuantity { get; set; }
        
        [ExcelColumn(Name = "料号")]
        public string MaterialNo { get; set; }
        
        [ExcelColumn(Name = "编码")]
        public string? Code { get; set; }

        [ExcelIgnore]
        public string? BigDiameter { get; set; }

        [ExcelIgnore]
        public string? SmallDiameter { get; set; }

        [ExcelIgnore]
        public string? InnerDiameter { get; set; } //内直径（成组后有值）

        [ExcelIgnore]
        public string? OuterDiameter { get; set; } //外直径（成组后有值）
        
        [ExcelIgnore]
        public string? AxialRunout { get; set; }   //轴向跳动（成组后有值）

        [ExcelIgnore]
        public string? RadialRunout { get; set; }  //径向跳动（成组后有值）

        [ExcelIgnore]
        public string? Width { get; set; }

        [ExcelIgnore]
        public string? SmallRangle { get; set; }

        [ExcelIgnore]
        public string? PlaneWidth { get; set; }    //平位宽

        [ExcelIgnore]
        public string? BigRangle { get; set; }
        
        [ExcelIgnore]
        public string? CurrentAngle { get; set; }

        [ExcelIgnore]
        public string? QrCodeImage { get; set; }

        [ExcelColumn(Name = "状态")]
        public string Status { get; set; }

        [ExcelColumn(Name = "是否成组")]
        public bool IsGroup { get; set; }

        [ExcelIgnore]
        public DateTime? BeginTime { get; set; }

        [ExcelIgnore]
        public DateTime? FinishTime { get; set; }

        [ExcelIgnore]
        public decimal? UseDuration { get; set; }

        [ExcelIgnore]
        public string? Remark { get; set; }

        [ExcelIgnore]
        public string? Position { get; set; }

        [ExcelIgnore]
        public string? DiscardBy { get; set; }

        [ExcelIgnore]
        public DateTime? DiscardTime { get; set; }
    }
}
