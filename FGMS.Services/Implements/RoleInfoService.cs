using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;
using FGMS.Services.Interfaces;

namespace FGMS.Services.Implements
{
    internal class RoleInfoService : BaseService<RoleInfo>, IRoleInfoService
    {
        public RoleInfoService(IBaseRepository<RoleInfo> repo, IFgmsDbContext context) : base(repo, context)
        {
        }
    }
}
