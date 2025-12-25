namespace FGMS.Models.Dtos
{
    public class PermissionInfoDto
    {
        public int Id { get; set; }
        public int RoleInfoId { get; set; }
        //public string? RoleName { get; set; }
        public int MenuInfoId { get; set; }
        //public string? MenuName { get; set; }
        public bool CanView { get; set; }
        public bool CanManagement { get; set; }
        public bool CanAudits { get; set; }
    }
}
