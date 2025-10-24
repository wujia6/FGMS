using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;

namespace FGMS.Repositories.Implements
{
    internal class RoleInfoRepository : BaseRepository<RoleInfo>, IRoleInfoRepository
    {
        public RoleInfoRepository(IFgmsDbRepository<RoleInfo> repository) : base(repository)
        {
        }
    }
}
