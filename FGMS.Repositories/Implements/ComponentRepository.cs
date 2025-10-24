using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;

namespace FGMS.Repositories.Implements
{
    internal class ComponentRepository : BaseRepository<Component>, IComponentRepository
    {
        public ComponentRepository(IFgmsDbRepository<Component> repository) : base(repository)
        {
        }
    }
}
