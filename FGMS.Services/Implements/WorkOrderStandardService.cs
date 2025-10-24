using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;
using FGMS.Services.Interfaces;

namespace FGMS.Services.Implements
{
    internal class WorkOrderStandardService : BaseService<WorkOrderStandard>, IWorkOrderStandardService
    {
        public WorkOrderStandardService(IBaseRepository<WorkOrderStandard> repo, IFgmsDbContext context) : base(repo, context)
        {
        }
    }
}
