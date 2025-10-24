using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;

namespace FGMS.Repositories.Implements
{
    internal class StandardRepository : BaseRepository<Standard>, IStandardRepository
    {
        public StandardRepository(IFgmsDbRepository<Standard> repository) : base(repository)
        {
        }
    }
}
