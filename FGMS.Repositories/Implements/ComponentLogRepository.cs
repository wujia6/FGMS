using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;

namespace FGMS.Repositories.Implements
{
    internal class ComponentLogRepository : BaseRepository<ComponentLog>, IComponentLogRepository
    {
        public ComponentLogRepository(IFgmsDbRepository<ComponentLog> repository) : base(repository)
        {
        }
    }
}
