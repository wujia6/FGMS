using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;
using FGMS.Services.Interfaces;

namespace FGMS.Services.Implements
{
    internal class TrackLogService : BaseService<TrackLog>, ITrackLogService
    {
        public TrackLogService(IBaseRepository<TrackLog> repo, IFgmsDbContext context) : base(repo, context)
        {
        }
    }
}
