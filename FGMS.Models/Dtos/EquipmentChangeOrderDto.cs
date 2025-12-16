namespace FGMS.Models.Dtos
{
    public class EquipmentChangeOrderDto
    {
        public int Id { get; set; }
        public int UserInfoId { get; set; }
        public string? UserInfoName { get; set; }
        public int ProductionOrderId { get; set; }
        public string? ProductionOrderNo { get; set; }
        public int EquipmentId { get; set; }
        public string? EquipmentCode { get; set; }
        public string? OrganizeCode { get; set; }
        public string OldEquipmentCode { get; set; }
        public string Reason { get; set; }
        public string? Status { get; set; }
        public DateTime? ChangeDate { get; set; }
    }
}
