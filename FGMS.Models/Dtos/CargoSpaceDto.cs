namespace FGMS.Models.Dtos
{
    public class CargoSpaceDto
    {
        public int Id { get; set; }
        public int? OrganizeId { get; set; }
        public string? OrganizeCode { get; set; }
        public string? OrganizeName { get; set; }
        public int? ParentId { get; set; }
        public string? ParentName { get; set; }
        public string? ParentCode { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Remark { get; set; }
        public bool? IsStandard { get; set; }
        public List<CargoSpaceDto>? ChildrenDtos { get; set; }
        public List<ComponentDto>? ComponentDtos { get; set; }
        public List<ElementEntityDto>? ElementEntityDtos { get; set; }
    }
}
