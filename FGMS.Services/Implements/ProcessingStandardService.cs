using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;
using FGMS.Services.Interfaces;

namespace FGMS.Services.Implements
{
    internal class ProcessingStandardService : BaseService<ProcessingStandard>, IProcessingStandardService
    {
        public ProcessingStandardService(IBaseRepository<ProcessingStandard> repo, IFgmsDbContext context) : base(repo, context)
        {
        }
    }
}
