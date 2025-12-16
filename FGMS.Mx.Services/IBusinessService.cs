using FGMS.Mx.Models;

namespace FGMS.Mx.Services
{
    public interface IBusinessService
    {
        public Task<List<OutboundMaterial>> GetBarcodesAsync(string codes);

        public Task<StoragePosition> GetStoragePositionsAsync(string code);
    }
}
