using MiniExcelLibs.Attributes;

namespace FGMS.Models.Dtos
{
    public class ElementDto
    {
        public int Id { get; set; }
        public int BrandId { get; set; }
        public string? BrandName { get; set; }
        [ExcelColumnName("类型")]
        public string Category { get; set; }
        [ExcelColumnName("料号")]
        public string MaterialNo { get; set; }
        [ExcelColumnName("型号")]
        public string ModalNo { get; set; }
        [ExcelColumnName("品名")]
        public string Name { get; set; }
        [ExcelColumnName("单位")]
        public string Unit { get; set; }
        [ExcelColumnName("规格")]
        public string Spec { get; set; }
        [ExcelColumnName("小R角")]
        public string? SmallRangle { get; set; }
        [ExcelColumnName("平位宽")]
        public string? PlaneWidth { get; set; }
        [ExcelColumnName("大R角")]
        public string? BigRangle { get; set; }
        [ExcelColumnName("标准直径")]
        public string? Diameter { get; set; }
        [ExcelColumnName("标准宽度")]
        public string? WheelWidth { get; set; }
        [ExcelColumnName("砂环宽")]
        public string? RingWidth { get; set; }
        [ExcelColumnName("基体厚度")]
        public string? Thickness { get; set; }
        [ExcelColumnName("砂轮角度")]
        public string? Angle { get; set; }
        [ExcelColumnName("内孔径")]
        public string? InnerBoreDiameter { get; set; }
        [ExcelColumnName("砂轮粒度")]
        public string? Granularity { get; set; }
        [ExcelColumnName("粘合剂")]
        public string? Binders { get; set; }
        [ExcelColumnName("描述")]
        public string? Desc { get; set; }
        [ExcelColumnName("长度")]
        public string? Lengths { get; set; }
        public List<ElementEntityDto>? ElementEntityDtos { get; set; }
    }
}
