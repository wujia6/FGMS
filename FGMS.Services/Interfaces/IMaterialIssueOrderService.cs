using FGMS.Models.Entities;

namespace FGMS.Services.Interfaces
{
    public interface IMaterialIssueOrderService : IBaseService<MaterialIssueOrder>
    {
        public Task<dynamic> OutboundAsync(int[] mioIds, int userInfoId);

        public Task<dynamic> EquipmentReceiveAsync(int mioId, int userInfoId);

        public Task<dynamic> PrepareAsync(int[] mioIds);
    }
}
