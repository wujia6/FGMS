using FGMS.Models.Entities;

namespace FGMS.Services.Interfaces
{
    public interface IEquipmentChangeOrderService : IBaseService<EquipmentChangeOrder>
    {
        public Task<dynamic> CreateAsync(int woId, int equipmentId, string oldEquipmentCode, string reason, int userId);

        public Task<dynamic> AuditAsync(int ecId, string status);
    }
}
