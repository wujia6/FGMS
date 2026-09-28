using MiniExcelLibs.Attributes;

namespace FGMS.Models.Dtos
{
    public class WorkOrderDto
    {
        [ExcelIgnore]
        public int Id { get; set; }

        [ExcelIgnore]
        public int? Pid { get; set; }

        [ExcelIgnore]
        public int? EquipmentId { get; set; }

        [ExcelColumn(Name ="机台")]
        public string? EquipmentCode { get; set; }

        [ExcelIgnore]
        public int ProductionOrderId { get; set; }

        [ExcelIgnore]
        public int[]? ProductionOrderIds { get; set; }

        [ExcelColumn(Name = "制令单")]
        public string? ProductionOrderNo { get; set; }

        [ExcelIgnore]
        public string[]? ProductionOrderNos { get; set; }

        [ExcelColumn(Name = "区域")]
        public string? OrganizeCode { get; set; }

        [ExcelIgnore]
        public int UserInfoId { get; set; }

        [ExcelColumn(Name = "创建人")]
        public string? UserInfoName { get; set; }

        [ExcelColumn(Name = "砂轮工单")]
        public string? OrderNo { get; set; }

        [ExcelIgnore]
        public string? ParentNo { get; set; }

        [ExcelColumn(Name = "类型")]
        public string? Type { get; set; }

        [ExcelColumn(Name = "优先级")]
        public string Priority { get; set; }

        [ExcelColumn(Name = "物料号")]
        public string MaterialNo { get; set; }

        [ExcelColumn(Name = "物料规格")]
        public string MaterialSpec { get; set; }

        [ExcelColumn(Name = "状态")]
        public string? Status { get; set; }

        [ExcelColumn(Name = "创建日期")]
        public DateTime? CreateDate { get; set; }

        [ExcelColumn(Name = "需求日期")]
        public DateTime? RequiredDate { get; set; }

        [ExcelColumn(Name = "备注")]
        public string? Remark { get; set; }

        [ExcelIgnore]
        public string? AgvTaskCode { get; set; }

        [ExcelIgnore]
        public string? AgvStatus { get; set; }

        [ExcelIgnore]
        public string? Reason { get; set; }

        [ExcelIgnore]
        public string? RenovateorName { get; set; }

        [ExcelIgnore]
        public string? RepairEquipmentCode { get; set; }

        [ExcelIgnore]
        public string? PreAllocationEquipmentCode { get; set; }

        [ExcelIgnore]
        public DateTime? ReceiveDate { get; set; }

        [ExcelIgnore]
        public string? StandardGrindingWheelSet { get; set; }

        [ExcelIgnore]
        public List<WorkOrderDto>? ChildrenDtos { get; set; }

        [ExcelIgnore]
        public List<ComponentDto>? ComponentDtos { get; set; }

        [ExcelIgnore]
        public List<StandardDto>? StandardDtos { get; set; }
    }
}
