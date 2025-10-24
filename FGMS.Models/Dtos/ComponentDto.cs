namespace FGMS.Models.Dtos
{
    public class ComponentDto
    {
        public int Id { get; set; }
        public int? StandardId { get; set; }
        public string? StandardCode { get; set; }
        public int? WorkOrderId { get; set; }
        public string? WorkOrderNo { get; set; }
        public int? CargoSpaceId { get; set; }
        public string? CargoSpaceCode { get; set; }
        public string? CargoSpaceName { get; set; }
        public int? CargoSpaceQuantity { get; set; }
        public string? Code { get; set; }
        public bool IsStandard { get; set; }
        public string? Status { get; set; }
        public StandardDto? StandardDto { get; set; }
        public WorkOrderDto? WorkOrderDto { get; set; }
        public List<ElementEntityDto>? ElementEntityDtos { get; set; }
    }
}
