namespace FGMS.Models.Dtos
{
    public class ComponentLogDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string OrderNo { get; set; }
        public string MaterialNo { get; set; }
        public string MaterialSpec { get; set; }
        public string EquipmentCode { get; set; }
        public DateTime RequiredDate { get; set; }
        public string? UpperJson { get; set; }
        public string? DownJson { get; set; }
    }
}
