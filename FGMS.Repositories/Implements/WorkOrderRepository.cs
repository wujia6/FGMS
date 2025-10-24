using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;

namespace FGMS.Repositories.Implements
{
    internal class WorkOrderRepository : BaseRepository<WorkOrder>, IWorkOrderRepository
    {
        public WorkOrderRepository(IFgmsDbRepository<WorkOrder> repository) : base(repository)
        {
        }
    }
}
