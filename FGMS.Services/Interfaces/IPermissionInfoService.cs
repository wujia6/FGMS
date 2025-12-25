using FGMS.Models.Entities;

namespace FGMS.Services.Interfaces
{
    public interface IPermissionInfoService : IBaseService<PermissionInfo>
    {
        public Task<bool> CheckPermissionAsync(string roleCode, string menuCode, string clientType, string permissionType);
    }
}
