namespace FGMS.Models.Dtos
{
    public class WorkOrderStandardDto
    {
        public int Id { get; set; }
        public int WorkOrderId { get; set; }
        public int StandardId { get; set; }
        public StandardDto? StandardDto { get; set; }
    }
}
