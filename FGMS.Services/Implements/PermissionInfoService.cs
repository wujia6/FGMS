using FGMS.Core.EfCore.Interfaces;
using FGMS.Models;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;
using FGMS.Services.Interfaces;

namespace FGMS.Services.Implements
{
    internal class PermissionInfoService : BaseService<PermissionInfo>, IPermissionInfoService
    {
        private readonly IPermissionInfoRepository permissionInfoRepository;

        public PermissionInfoService(IBaseRepository<PermissionInfo> repo, IFgmsDbContext context) : base(repo, context)
        {
            permissionInfoRepository = repo as IPermissionInfoRepository ?? throw new ArgumentNullException(nameof(repo));
        }

        public async Task<bool> CheckPermissionAsync(string roleCode, string menuCode, string clientType, string permissionType)
        {
            var permissionInfo = await permissionInfoRepository.GetEntityAsync(
                expression: src => src.RoleInfo!.Code.Equals(roleCode) && src.MenuInfo!.Client == Enum.Parse<ClientType>(clientType) && src.MenuInfo.Code.Equals(menuCode));

            if (permissionInfo == null)
                return false;

            return permissionType switch
            {
                "view" => permissionInfo.CanView,
                "management" => permissionInfo.CanManagement,
                "audit" => permissionInfo.CanAudits,
                _ => false
            };
        }
    }
}
