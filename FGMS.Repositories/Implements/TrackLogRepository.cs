using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;

namespace FGMS.Repositories.Implements
{
    internal class TrackLogRepository : BaseRepository<TrackLog>, ITrackLogRepository
    {
        public TrackLogRepository(IFgmsDbRepository<TrackLog> repository) : base(repository)
        {
        }
    }
}
