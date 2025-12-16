namespace FGMS.Models.Dtos
{
    public class EquipmentDto
    {
        public int Id { get; set; }
        public int OrganizeId { get; set; }
        public string? OrganizeName { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool? Enabled { get; set; }
        public bool? Mount { get; set; }
        public List<ProductionOrderDto>? ProductionOrderDtos { get; set; }
    }
}
