using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;

namespace FGMS.Repositories.Implements
{
    internal class MaterialDiameterRepository : BaseRepository<MaterialDiameter>, IMaterialDiameterRepository
    {
        public MaterialDiameterRepository(IFgmsDbRepository<MaterialDiameter> repository) : base(repository)
        {
        }
    }
}
