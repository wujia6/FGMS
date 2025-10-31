namespace FGMS.Models.Dtos
{
    public class WorkOrderDto
    {
        public int Id { get; set; }
        public int? Pid { get; set; }
        public int EquipmentId { get; set; }
        public string? EquipmentCode { get; set; }
        public string? OrganizeCode { get; set; }
        public int UserInfoId { get; set; }
        public string? UserInfoName { get; set; }
        public string? OrderNo { get; set; }
        public string? ParentNo { get; set; }
        public string? Type { get; set; }
        public string Priority { get; set; }
        public string MaterialNo { get; set; }
        public string MaterialSpec { get; set; }
        public string? Status { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? RequiredDate { get; set; }
        public string? Remark { get; set; }
        public string? AgvTaskCode { get; set; }
        public string? AgvStatus { get; set; }
        public string? Reason { get; set; }
        public string? RenovateorName { get; set; }
        public List<WorkOrderDto>? ChildrenDtos { get; set; }
        public List<ComponentDto>? ComponentDtos { get; set; }
        public List<StandardDto>? StandardDtos { get; set; }
    }
}
