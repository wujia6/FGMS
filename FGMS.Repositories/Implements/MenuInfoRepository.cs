using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;

namespace FGMS.Repositories.Implements
{
    internal class MenuInfoRepository : BaseRepository<MenuInfo>, IMenuInfoRepositoy
    {
        public MenuInfoRepository(IFgmsDbRepository<MenuInfo> repository) : base(repository)
        {
        }
    }
}
