using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;
using FGMS.Services.Interfaces;

namespace FGMS.Services.Implements
{
    internal class MenuInfoService : BaseService<MenuInfo>, IMenuInfoService
    {
        public MenuInfoService(IBaseRepository<MenuInfo> repo, IFgmsDbContext context) : base(repo, context)
        {
        }
    }
}
