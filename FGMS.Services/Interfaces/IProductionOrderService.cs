using FGMS.Models.Entities;

namespace FGMS.Services.Interfaces
{
    public interface IProductionOrderService : IBaseService<ProductionOrder>
    {
        public Task<dynamic> MadeBeginAsync(int poid);
    }
}
