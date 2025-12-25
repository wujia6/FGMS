namespace FGMS.Models.Dtos
{
    public class MenuInfoDto
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Client { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Path { get; set; }
        public string? Icon { get; set; }
        public bool IsVisible { get; set; }
        //public MenuInfoDto? ParentDto { get; set; }
        public List<MenuInfoDto>? ChildrenDtos { get; set; }
        public PermissionInfoDto? PermissionInfoDto { get; set; }
    }
}
