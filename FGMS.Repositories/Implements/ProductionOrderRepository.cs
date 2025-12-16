using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;

namespace FGMS.Repositories.Implements
{
    internal class ProductionOrderRepository : BaseRepository<ProductionOrder>, IProductionOrderRepository
    {
        public ProductionOrderRepository(IFgmsDbRepository<ProductionOrder> repository) : base(repository)
        {
        }
    }
}
