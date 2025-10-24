using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;
using FGMS.Services.Interfaces;

namespace FGMS.Services.Implements
{
    internal class ComponentLogService : BaseService<ComponentLog>, IComponentLogService
    {
        public ComponentLogService(IBaseRepository<ComponentLog> repo, IFgmsDbContext context) : base(repo, context)
        {
        }
    }
}
