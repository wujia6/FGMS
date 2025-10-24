using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;

namespace FGMS.Repositories.Implements
{
    internal class UserInfoRepository : BaseRepository<UserInfo>, IUserInfoRepository
    {
        public UserInfoRepository(IFgmsDbRepository<UserInfo> repository) : base(repository)
        {
        }
    }
}
