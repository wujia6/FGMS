using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;
using FGMS.Services.Interfaces;

namespace FGMS.Services.Implements
{
    internal class StandardService : BaseService<Standard>, IStandardService
    {
        public StandardService(IBaseRepository<Standard> repo, IFgmsDbContext context) : base(repo, context)
        {
        }
    }
}
