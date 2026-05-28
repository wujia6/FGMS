using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;

namespace FGMS.Repositories.Implements
{
    internal class ProductionOrderLogRepository : BaseRepository<ProductionOrderLog>, IProductionOrderLogRepository
    {
        public ProductionOrderLogRepository(IFgmsDbRepository<ProductionOrderLog> repository) : base(repository)
        {
        }
    }
}
