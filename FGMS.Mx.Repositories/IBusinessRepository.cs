using FGMS.Mx.Models;

namespace FGMS.Mx.Repositories
{
    public interface IBusinessRepository
    {
        public Task<List<OutboundMaterial>> GetBarcodesAsync(string codes);

        public Task<StoragePosition> GetStoragePositionsAsync(string code);
    }
}
