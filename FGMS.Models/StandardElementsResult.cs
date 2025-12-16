using FGMS.Models.Dtos;

namespace FGMS.Models
{
    public class StandardElementsResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<ElementEntityDto>? Mains { get; set; }
        public List<ElementEntityDto>? Firsts { get; set; }
        public List<ElementEntityDto>? Seconds { get; set; }
        public List<ElementEntityDto>? Thirds { get; set; }
        public List<ElementEntityDto>? Fourths { get; set; }
        public List<ElementEntityDto>? Fifths { get; set; }
    }
}
