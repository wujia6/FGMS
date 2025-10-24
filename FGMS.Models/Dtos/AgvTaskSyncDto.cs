namespace FGMS.Models.Dtos
{
    public class AgvTaskSyncDto
    {
        public int Id { get; set; }
        public string AgvCode { get; set; }
        public string TaskCode { get; set; }
        public string WorkOrderNo { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
    }
}
