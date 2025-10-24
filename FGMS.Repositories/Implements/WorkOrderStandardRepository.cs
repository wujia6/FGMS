using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;

namespace FGMS.Repositories.Implements
{
    internal class WorkOrderStandardRepository : BaseRepository<WorkOrderStandard>, IWorkOrderStandardRepository
    {
        public WorkOrderStandardRepository(IFgmsDbRepository<WorkOrderStandard> repository) : base(repository)
        {
        }
    }
}
