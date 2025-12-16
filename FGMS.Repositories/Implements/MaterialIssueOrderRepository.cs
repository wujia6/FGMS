using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;

namespace FGMS.Repositories.Implements
{
    internal class MaterialIssueOrderRepository : BaseRepository<MaterialIssueOrder>, IMaterialIssueOrderRepository
    {
        public MaterialIssueOrderRepository(IFgmsDbRepository<MaterialIssueOrder> repository) : base(repository)
        {
        }
    }
}
