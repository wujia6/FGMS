namespace FGMS.Models.Dtos
{
    public class TrackLogDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string? JsonContent { get; set; }
        public DateTime Date { get; set; }
    }
}
