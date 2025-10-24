namespace FGMS.Models.Dtos
{
    public class UserInfoDto
    {
        public int Id { get; set; }
        public int RoleInfoId { get; set; }
        public string? RoleInfoName { get; set; }
        public string WorkNo { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
    }
}
