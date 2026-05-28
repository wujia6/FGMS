using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;
using FGMS.Services.Interfaces;

namespace FGMS.Services.Implements
{
    internal class ProductionOrderLogService : BaseService<ProductionOrderLog>, IProductionOrderLogService
    {
        public ProductionOrderLogService(IBaseRepository<ProductionOrderLog> repo, IFgmsDbContext context) : base(repo, context)
        {
        }
    }
}
