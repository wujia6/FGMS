using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;
using FGMS.Services.Interfaces;

namespace FGMS.Services.Implements
{
    internal class UserInfoService : BaseService<UserInfo>, IUserInfoService
    {
        public UserInfoService(IBaseRepository<UserInfo> repo, IFgmsDbContext context) : base(repo, context)
        {
        }
    }
}
