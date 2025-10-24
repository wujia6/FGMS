using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;

namespace FGMS.Repositories.Implements
{
    internal class OrganizeRepository : BaseRepository<Organize>, IOrganizeRepository
    {
        public OrganizeRepository(IFgmsDbRepository<Organize> repository) : base(repository)
        {
        }
    }
}
