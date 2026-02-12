namespace FGMS.Models.Dtos
{
    public class ProductionOrderDto
    {
        public int Id { get; set; }
        public int UserInfoId { get; set; }
        public string? UserInfoName { get; set; }
        public int EquipmentId { get; set; }
        public string? EquipmentCode { get; set; }
        public string? OrganizeCode { get; set; }
        public int? WorkOrderId { get; set; }
        public string? WorkOrderNo { get; set; }
        public string OrderNo { get; set; }
        public string FinishCode { get; set; }
        public string FinishName { get; set; }
        public string FinishSpec { get; set; }
        public string MaterialCode { get; set; }
        public string MaterialName { get; set; }
        public string MaterialSpec { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
        public DateTime? CreateTime { get; set; }
        public string? Remark { get; set; }
        public double? WorkHours { get; set; }
        public DateTime? PlannedBeginTime { get; set; }
        public bool? IsDc { get; set; }
        public bool? Report { get; set; }
        public DateTime? PlannedEndTime { get; set; }
        public DateTime? CompletedTime { get; set; }

        public WorkOrderDto? WorkOrderDto { get; set; }
        public List<MaterialIssueOrderDto>? MaterialIssueOrderDtos { get; set; }
    }
}
