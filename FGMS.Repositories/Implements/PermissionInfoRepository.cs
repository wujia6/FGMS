using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;

namespace FGMS.Repositories.Implements
{
    internal class PermissionInfoRepository : BaseRepository<PermissionInfo>, IPermissionInfoRepository
    {
        public PermissionInfoRepository(IFgmsDbRepository<PermissionInfo> repository) : base(repository)
        {
        }
    }
}
