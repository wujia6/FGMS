namespace FGMS.Models.Dtos
{
    public class ElementEntityDto
    {
        public int Id { get; set; }
        public int ElementId { get; set; }
        public string? ElementCategory { get; set; }
        public string? ElementMaterialNo { get; set; }
        public string? ElementName { get; set; }
        public string? ElementSpec { get; set; }
        public int? ComponentId { get; set; }
        public string? ComponentCode { get; set; }
        public string? WorkOrderNo { get; set; }
        public int? CargoSpaceId { get; set; }
        public string? CargoSpaceCode { get; set; }
        public string? CargoSpaceName { get; set; }
        public int? CargoSpaceQuantity { get; set; }
        public string MaterialNo { get; set; }
        public string? Code { get; set; }
        public string? BigDiameter { get; set; }
        public string? SmallDiameter { get; set; }
        public string? InnerDiameter { get; set; } //内直径（成组后有值）
        public string? OuterDiameter { get; set; } //外直径（成组后有值）
        public string? AxialRunout { get; set; }   //轴向跳动（成组后有值）
        public string? RadialRunout { get; set; }  //径向跳动（成组后有值）
        public string? Width { get; set; }
        public string? SmallRangle { get; set; }
        public string? PlaneWidth { get; set; }    //平位宽
        public string? BigRangle { get; set; }
        public string? CurrentAngle { get; set; }
        public string? QrCodeImage { get; set; }
        public string Status { get; set; }
        public bool IsGroup { get; set; }
        public DateTime? BeginTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public decimal? UseDuration { get; set; }
        public string? Remark { get; set; }
        public string? Position { get; set; }
        public string? DiscardBy { get; set; }
        public DateTime? DiscardTime { get; set; }
        //public ElementDto? ElementDto { get; set; }
    }
}
