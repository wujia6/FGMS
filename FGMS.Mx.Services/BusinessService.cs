using FGMS.Mx.Models;
using FGMS.Mx.Repositories;

namespace FGMS.Mx.Services
{
    internal class BusinessService : IBusinessService
    {
        private readonly IBusinessRepository businessRepository;

        public BusinessService(IBusinessRepository businessRepository)
        {
            this.businessRepository = businessRepository;
        }

        public async Task<List<OutboundMaterial>> GetBarcodesAsync(string codes)
        {
            return await businessRepository.GetBarcodesAsync(codes);
        }

        public async Task<StoragePosition> GetStoragePositionsAsync(string code)
        {
            return await businessRepository.GetStoragePositionsAsync(code);
        }
    }
}
