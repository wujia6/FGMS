using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;

namespace FGMS.Repositories.Implements
{
    internal class AgvTaskSyncRepository : BaseRepository<AgvTaskSync>, IAgvTaskSyncRepository
    {
        public AgvTaskSyncRepository(IFgmsDbRepository<AgvTaskSync> repository) : base(repository)
        {
        }
    }
}
