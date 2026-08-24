using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;

namespace FGMS.Repositories.Implements
{
    internal class ProcessingStandardRepository : BaseRepository<ProcessingStandard>, IProcessingStandardRepository
    {
        public ProcessingStandardRepository(IFgmsDbRepository<ProcessingStandard> repository) : base(repository)
        {
        }
    }
}
