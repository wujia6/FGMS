using FGMS.Models.Dtos;

namespace FGMS.Models
{
    public class RepairParam
    {
        public int WorkOrderId { get; set; }
        public int EquipmentId { get; set; }
        public string EquipmentCode { get; set; }
        public string MaterialNo { get; set; }
        public string MaterialSpec { get; set; }
        //public List<WorkPieceGroupDto> Groups { get; set; }
    }
}
