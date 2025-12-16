using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;
using FGMS.Services.Interfaces;

namespace FGMS.Services.Implements
{
    internal class MaterialDiameterService : BaseService<MaterialDiameter>, IMaterialDiameterService
    {
        public MaterialDiameterService(IBaseRepository<MaterialDiameter> repo, IFgmsDbContext context) : base(repo, context)
        {
        }
    }
}
