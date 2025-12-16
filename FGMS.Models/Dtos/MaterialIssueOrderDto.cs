namespace FGMS.Models.Dtos
{
    public class MaterialIssueOrderDto
    {
        public int Id { get; set; }
        public int ProductionOrderId { get; set; }
        public string? ProductionOrderNo { get; set; }
        public int CreateorId { get; set; }
        public string? CreateorName { get; set; }
        public int? SendorId { get; set; }
        public string? SendorName { get; set; }
        public string? EquipmentCode { get; set; }
        public string? OrganizeCode { get; set; }
        public string Type { get; set; }
        public string OrderNo { get; set; }
        public string MaterialNo { get; set; }
        public string MaterialName { get; set; }
        public string MaterialSpce { get; set; }
        public int Quantity { get; set; }
        public DateTime? IssueTime { get; set; }
        public string? Status { get; set; }
        public string? MxWareHouse { get; set; }
        public string? MxCargoSpace { get; set; }
        public string? MxBarCode { get; set; }
    }
}
