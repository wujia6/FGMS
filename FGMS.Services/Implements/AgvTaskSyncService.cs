using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;
using FGMS.Services.Interfaces;

namespace FGMS.Services.Implements
{
    internal class AgvTaskSyncService : BaseService<AgvTaskSync>, IAgvTaskSyncService
    {
        public AgvTaskSyncService(IBaseRepository<AgvTaskSync> repo, IFgmsDbContext context) : base(repo, context)
        {
        }
    }
}
