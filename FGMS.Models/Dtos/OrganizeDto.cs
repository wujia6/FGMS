using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FGMS.Models.Dtos
{
    public class OrganizeDto
    {
        public int Id { get; set; }
        public int? Pid { get; set; }
        public string? ParentName { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Position { get; set; }
        public List<OrganizeDto>? ChildrenDtos { get; set; }
    }
}
