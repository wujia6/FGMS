namespace FGMS.Models.Dtos
{
    public class ProductionOrderLogDto
    {
        public int Id { get; set; }
        public string Operator { get; set; }
        public string Operation { get; set; }
        public DateTime? OperationTime { get; set; }
    }
}
